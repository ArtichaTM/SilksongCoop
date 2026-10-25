"""Collapse the per-scene FSM/object dumps into first-seen elements, walking scenes in scene_order.txt.

Element kinds:
  fsm  - a PlayMaker FSM, keyed by name + states + action types + transitions (field values ignored)
  comp - a game C# component type (one that exists in decomp/), keyed by type name

Writes:
  scene_order.txt  - seeded once from AREA_ORDER, then edited by hand
  areas.tsv        - per area: scene count, new fsm elements, new component types
  catalog/<area>.md - per scene in order: its new elements with a pointer into the dump
  index.tsv        - every use of every element (id, scene, object path, dump ref), written to the dump dir
"""

import hashlib
import os
import re
import sys
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
# Untracked dump (scenes/, decomp/, index.tsv); refs in the catalog are relative to it.
DUMP = Path(os.environ.get("SILKSONG_DUMP", ROOT.parent.parent / "data" / "FsmDump"))
SCENES = DUMP / "scenes"
DECOMP = DUMP / "decomp"
ORDER = ROOT / "scene_order.txt"
CATALOG = ROOT / "catalog"

# Progression order from the user's route (2026-10-24). Act 2 areas are unordered among themselves; the Abyss is last.
# Each entry: (key, scene-name pattern, in-game name). The in-game names are what bugs get reported by.
# Not mapped to a prefix yet: Blasted Steps, Grand Gate.
AREA_ORDER = [
    ("Tut", r"Tut_", "Moss Grotto"),
    ("Bonetown", r"Bonetown", "Bone Bottom"),
    ("Mosstown", r"Mosstown", "Mosshome"),
    ("Bonegrave", r"Bonegrave", "Bonegrave (near Bone Bottom?)"),
    ("Bone", r"Bone_(?!East)", "The Marrow"),
    ("Bellway", r"Bellway_|Bellshrine", "Bellways and Bell Shrines"),
    ("Dock", r"Dock_", "Deep Docks"),
    ("Bone_East", r"Bone_East", "Far Fields"),
    ("Wisp", r"Wisp_", "Wisp Thicket"),
    ("Greymoor", r"Greymoor_|Halfway_", "Greymoor (with Halfway Home)"),
    ("Ant", r"Ant_", "Hunter's March"),
    ("Crawl", r"Crawl_", "The Wormways"),
    ("Shellwood", r"Shellwood_", "Shellwood"),
    ("Belltown", r"Belltown", "Bellhart"),
    ("Room", r"Room_", "Rooms (caravan, crow court, diving bell, forge...)"),
    ("Peak", r"Peak_", "Mount Fay"),
    ("Coral", r"Coral_", "Sands of Karak"),
    ("Dust", r"Dust_", "Sinner's Road (Dust_Maze = the Mist)"),
    ("Shadow", r"Shadow_", "Bilewater"),
    ("Weave", r"Weave_", "Weavenests"),
    ("Song", r"Song_|City_", "Choral Chambers / Citadel"),
    ("Cog", r"Cog_", "Cogwork Core"),
    ("Ward", r"Ward_", "Whiteward"),
    ("Under", r"Under_", "Underworks"),
    ("Aqueduct", r"Aqueduct_", "Putrified Ducts"),
    ("Slab", r"Slab_", "The Slab"),
    ("Library", r"Library_", "Whispering Vaults"),
    ("Hang", r"Hang_", "High Halls"),
    ("Arborium", r"Arborium_", "Memorium"),
    ("Cradle", r"Cradle_", "The Cradle"),
    ("Clover", r"Clover_", "Verdania"),
    ("Optional", r"Chapel_|Shellgrave|Aspid_|Sprintmaster|Organ_|Memory_|Abandoned_|Tube_",
     "Optional and memory scenes (not on the main route)"),
    ("Abyss", r"Abyss_|Last_", "The Abyss (Act 3, always last)"),
    ("Cinematic", r"Cinematic_", "Cinematics"),
    ("Meta", r"End_|Opening_|Menu_|Pre_|Quit_|PermaDeath|Demo ", "Menus, intro, endings, credits"),
]

AREA_NAMES = {key: name for key, _, name in AREA_ORDER}


def area_of(scene: str) -> str:
    for area, pattern, _ in AREA_ORDER:
        if re.match(pattern, scene):
            return area
    return "Other"


def natural(text: str) -> list:
    return [int(part) if part.isdigit() else part.lower() for part in re.split(r"(\d+)", text)]


def scene_order() -> list[str]:
    names = [line.strip() for line in (SCENES / "_scenes.txt").read_text().splitlines() if line.strip()]
    if not ORDER.exists():
        rank = {area: index for index, (area, _, _) in enumerate(AREA_ORDER)}
        names.sort(key=lambda name: (rank.get(area_of(name), len(rank)), natural(name)))
        ORDER.write_text("\n".join(names) + "\n")
    order = [line.strip() for line in ORDER.read_text().splitlines() if line.strip() and not line.startswith("#")]
    missing = set(names) - set(order)
    if missing:
        sys.exit(f"scene_order.txt lacks: {sorted(missing)}")
    return order


FSM_HEAD = re.compile(r"^FSM (?P<scene>[^:]*):(?P<path>.*) \[(?P<name>[^\]]*)\] (?:active|inactive), now")


def parse_fsms(scene: str):
    """Yields (path, fsm name, signature text, action types, first line number)."""
    lines = (SCENES / f"{scene}.fsm.txt").read_text(errors="replace").splitlines()
    head = None
    body: list[str] = []
    actions: set[str] = set()

    def flush():
        if head is not None:
            yield head[0], head[1], "\n".join(body), actions, head[2]

    for number, line in enumerate(lines, 1):
        match = FSM_HEAD.match(line)
        if match:
            yield from flush()
            head = (match["path"], match["name"], number)
            body, actions = [match["name"]], set()
        elif head is None or not line.strip():
            continue
        elif line.startswith("    on ") or line.startswith("  global ") or line.startswith("  state "):
            body.append(line.strip())
        elif line.startswith("    "):
            action = line.split(" ", 5)[4].split(" ")[0]
            body.append(action)
            actions.add(action)
    yield from flush()


OBJECT_LINE = re.compile(r"^(?P<indent> *)(?P<name>.*?)(?: \(inactive\))? @\([^)]*\) \[(?P<comps>.*)\]$")


def parse_components(scene: str, game_types: set[str]):
    """Yields (object path, component type, line number) for game-defined components."""
    stack: list[str] = []
    lines = (SCENES / f"{scene}.objects.txt").read_text(errors="replace").splitlines()
    for number, line in enumerate(lines, 1):
        match = OBJECT_LINE.match(line)
        if not match:
            continue
        depth = len(match["indent"]) // 2
        del stack[depth:]
        stack.append(match["name"])
        for comp in filter(None, (part.strip() for part in re.split(r",\s*(?![^()]*\))", match["comps"]))):
            kind = comp.split("(", 1)[0]
            if kind in game_types and kind != "PlayMakerFSM":
                yield "/".join(stack), kind, number


def main() -> None:
    order = scene_order()
    game_types = {path.stem for path in DECOMP.glob("*.cs")}
    seen: dict[str, str] = {}
    uses: defaultdict[str, list[tuple[str, str, str]]] = defaultdict(list)
    per_scene: dict[str, list[dict]] = {}
    for scene in order:
        new: dict[str, dict] = {}
        for path, name, signature, actions, line in parse_fsms(scene):
            key = "fsm:" + hashlib.sha1(signature.encode()).hexdigest()[:10]
            uses[key].append((scene, path, f"scenes/{scene}.fsm.txt:{line}"))
            if key in seen:
                continue
            if key not in new:
                new[key] = dict(kind="fsm", id=key[4:], name=name, paths=[], line=line, actions=sorted(actions))
            new[key]["paths"].append(path)
        for path, kind, line in parse_components(scene, game_types):
            key = "comp:" + kind
            uses[key].append((scene, path, f"scenes/{scene}.objects.txt:{line}"))
            if key in seen:
                continue
            if key not in new:
                new[key] = dict(kind="comp", id=kind, name=kind, paths=[], line=line, actions=[])
            new[key]["paths"].append(path)
        for key in new:
            seen[key] = scene
        per_scene[scene] = list(new.values())
    write_outputs(order, per_scene, uses)
    write_index(uses)


def write_outputs(order, per_scene, uses) -> None:
    CATALOG.mkdir(exist_ok=True)
    by_area: dict[str, list[str]] = defaultdict(list)
    for scene in order:
        by_area[area_of(scene)].append(scene)
    rows = ["area\tname\tscenes\tnew_fsm\tnew_comp"]
    for area, scenes in by_area.items():
        elements = [element for scene in scenes for element in per_scene[scene]]
        fsm = sum(element["kind"] == "fsm" for element in elements)
        name = AREA_NAMES.get(area, "?")
        rows.append(f"{area}\t{name}\t{len(scenes)}\t{fsm}\t{len(elements) - fsm}")
        out = [f"# {area}: {name}", ""]
        for scene in scenes:
            out.append(f"## {scene}")
            for element in per_scene[scene]:
                count = len(uses[f"{element['kind']}:{element['id']}"])
                paths = ", ".join(element["paths"][:3]) + (" ..." if len(element["paths"]) > 3 else "")
                kind = "objects" if element["kind"] == "comp" else "fsm"
                where = f"scenes/{scene}.{kind}.txt:{element['line']}"
                label = f"FSM [{element['name']}] id={element['id']}" if element["kind"] == "fsm" else f"C# {element['name']}"
                out.append(f"- {label} at {paths} ({count} uses in game) -> {where}")
            out.append("")
        (CATALOG / f"{area}.md").write_text("\n".join(out))
    (ROOT / "areas.tsv").write_text("\n".join(rows) + "\n")
    print("\n".join(rows))


def write_index(uses) -> None:
    """index.tsv: every use of every element, so a fix to one catalog row can list all the places it touches."""
    rows = ["id\tscene\tobject\tref"]
    for key, places in uses.items():
        element = key.split(":", 1)[1]
        rows.extend(f"{element}\t{scene}\t{path}\t{ref}" for scene, path, ref in places)
    (DUMP / "index.tsv").write_text("\n".join(rows) + "\n")


if __name__ == "__main__":
    main()
