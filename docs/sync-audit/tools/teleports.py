"""Build teleports.tsv for the dev teleport menu: every verdict row, its scene, a door and its objects' positions.

In-game ids are `<verdict file>.<row>`: row 3.01 of verdicts/02.md is `02.01`.
Additive scenes without doors (Bone_05_boss) load through the nearest prefix scene that has one (Bone_05).
One line per object; a row whose objects have no dump position gets x and y empty, one with no object gets path empty.
FSM refs expand to every object in the scene running the same FSM element (index.tsv); component refs stay as given.
Without the dump the existing teleports.tsv is kept.
"""

import os
import re
import sys
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
DUMP = Path(os.environ.get("SILKSONG_DUMP", ROOT.parent.parent / "data" / "FsmDump"))
SCENES = DUMP / "scenes"
OUT = ROOT / "teleports.tsv"

ROW_ID = re.compile(r"^(?P<scene>[A-Za-z0-9_]+)\.(?P<n>\d+)$")
OBJECT_LINE = re.compile(r"^(?P<indent> *)(?P<name>.*?)(?: \(inactive\))? @\((?P<x>-?[\d.]+), (?P<y>-?[\d.]+)\)")
FSM_HEADER = re.compile(r"^FSM (?P<scene>[^:]+):(?P<path>.*) \[[^\]]*\] ")
REF = re.compile(r"scenes/(?P<scene>[^ ,;|]+)\.(?P<kind>fsm|objects)\.txt:(?P<line>\d+)")
DOOR_NAME = re.compile(r"^(left|right|top|bot|door)\d*$")
FSM_ID = re.compile(r"^[0-9a-f]{10}$")


def table_rows(path: Path):
    """Cells of every markdown table body row."""
    for line in path.read_text().splitlines():
        if line.startswith("| ") and not line.startswith("| #") and not line.startswith("|---"):
            yield [cell.strip() for cell in line.strip().strip("|").split("|")]


def expand_rows(cell: str) -> list[str]:
    """`Bone_01.3, .4`, `Bone_01.5 .. .7` and `Tut_02.3, Tut_03.6` into audit row ids."""
    ids: list[str] = []
    scene = ""
    for part in cell.split(","):
        bounds = [b.strip() for b in part.split("..")]
        numbers = []
        for bound in bounds:
            if match := ROW_ID.match(bound):
                scene = match["scene"]
                numbers.append(int(match["n"]))
            elif bound.startswith(".") and bound[1:].isdigit():
                numbers.append(int(bound[1:]))
            else:
                sys.exit(f"cannot read row cell: {cell!r}")
        span = range(numbers[0], numbers[-1] + 1)
        ids.extend(f"{scene}.{n}" for n in span)
    return ids


class Scene:
    """One scene's object tree (path -> positions), FSM headers and first door."""

    def __init__(self, name: str):
        self.positions: dict[str, list[tuple[str, str]]] = defaultdict(list)
        self.object_lines: dict[int, str] = {}
        self.door = ""
        stack: list[str] = []
        objects = SCENES / f"{name}.objects.txt"
        doors: list[str] = []
        for number, line in enumerate(objects.read_text().splitlines() if objects.exists() else [], 1):
            if not (match := OBJECT_LINE.match(line)):
                continue
            depth = len(match["indent"]) // 2
            del stack[depth:]
            stack.append(match["name"])
            path = "/".join(stack)
            self.positions[path].append((match["x"], match["y"]))
            self.object_lines[number] = path
            if "TransitionPoint(" in line:
                doors.append(match["name"])
        named = [door for door in doors if DOOR_NAME.match(door)]
        self.door = (named or doors or [""])[0]
        self.fsm_headers: list[tuple[int, str]] = []
        fsm = SCENES / f"{name}.fsm.txt"
        for number, line in enumerate(fsm.read_text().splitlines() if fsm.exists() else [], 1):
            if match := FSM_HEADER.match(line):
                self.fsm_headers.append((number, match["path"]))

    def fsm_path(self, line: int) -> str:
        """Path of the FSM whose block holds this line."""
        found = ""
        for number, path in self.fsm_headers:
            if number > line:
                break
            found = path
        return found


def entry(name: str, scenes: dict[str, Scene]) -> tuple[str, str]:
    """Scene to load and door to enter by, so that `name` is loaded."""
    parts = name.split("_")
    for cut in range(len(parts), 0, -1):
        candidate = "_".join(parts[:cut])
        if candidate not in scenes and (SCENES / f"{candidate}.objects.txt").exists():
            scenes[candidate] = Scene(candidate)
        if candidate in scenes and scenes[candidate].door:
            return candidate, scenes[candidate].door
    return name, ""


def main() -> None:
    if not SCENES.is_dir():
        print(f"no dump at {DUMP}; keeping {OUT.name}")
        return
    audit: dict[str, tuple[str, list[re.Match]]] = {}
    for path in sorted((ROOT / "audit").glob("[0-9][0-9]_*.md")):
        for cells in table_rows(path):
            if ROW_ID.match(cells[0]):
                audit[cells[0]] = (cells[1], list(REF.finditer(cells[-1])))

    verdicts: list[tuple[str, str, list[str]]] = []
    for path in sorted((ROOT / "verdicts").glob("[0-9][0-9].md")):
        for cells in table_rows(path):
            if re.match(r"^\d+\.\d+$", cells[0]):
                verdicts.append((f"{path.stem}.{cells[0].split('.')[1]}", cells[2], expand_rows(cells[1])))

    needed = {ROW_ID.match(row)["scene"] for _, _, rows in verdicts for row in rows}
    ids_at: dict[tuple[str, str], set[str]] = defaultdict(set)
    uses: dict[tuple[str, str], list[str]] = defaultdict(list)
    with (DUMP / "index.tsv").open() as index:
        next(index)
        for line in index:
            element, scene, obj, ref = line.rstrip("\n").split("\t")
            if scene in needed:
                ids_at[(scene, ref)].add(element)
                if FSM_ID.match(element):
                    uses[(element, scene)].append(obj)
    scenes = {name: Scene(name) for name in sorted(needed)}
    entries = {name: entry(name, scenes) for name in sorted(needed)}

    lines = ["id\trow\telement\tscene\tload\tdoor\tpath\tx\ty"]
    for verdict, element, rows in verdicts:
        for row in rows:
            scene_name = ROW_ID.match(row)["scene"]
            scene = scenes[scene_name]
            name, refs = audit.get(row, (element, []))
            paths: list[str] = []
            for ref in refs:
                ref_scene = scenes.get(ref["scene"])
                if ref["scene"] != scene_name or ref_scene is None:
                    continue
                line = int(ref["line"])
                if ref["kind"] == "objects":
                    found = [ref_scene.object_lines.get(line, "")]
                else:
                    text = f"scenes/{scene_name}.fsm.txt:{line}"
                    fsm_ids = [i for i in ids_at.get((scene_name, text), ()) if FSM_ID.match(i)]
                    found = [p for i in fsm_ids for p in uses[(i, scene_name)]] or [ref_scene.fsm_path(line)]
                paths.extend(p for p in found if p and p not in paths)
            prefix = [verdict, row, name.replace("\t", " "), scene_name, *entries[scene_name]]
            if not paths:
                lines.append("\t".join([*prefix, "", "", ""]))
            for path in paths:
                for x, y in scene.positions.get(path) or [("", "")]:
                    lines.append("\t".join([*prefix, path, x, y]))
    OUT.write_text("\n".join(lines) + "\n")
    print(f"{OUT.name}: {len(lines) - 1} targets for {len(verdicts)} verdict rows in {len(scenes)} scenes")


if __name__ == "__main__":
    main()
