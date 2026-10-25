# Sync audit ledger

Every scene element of Silksong, audited for coop: what each element needs, what our code does about it, and what has
been tested. The tables are the per-element truth; `mechanics.md` (added with the status columns) summarises the shared
causes and links to the rows each one covers.

## Tracked here

- `audit/<NN>_<Area>.md`: per-scene tables (what coop needs). Columns: `#`, Object, FSM / C#, Does, Verdict
  (`sure` / `maybe` / `no`), Why, What to sync, Design choice, Ref. Each table ends with `## Summary`.
- `audit/notes/<Type>.md`: short explanations of game C# types and PlayMaker actions.
- `catalog/<Area>.md`: each element listed once, in the first scene it appears in (`scene_order.txt` order).
- `areas.tsv`: in-game area name for each scene prefix (Blasted Steps and Grand Gate are not mapped yet).
- `scene_order.txt`: scene walk order used by the catalog.
- `tools/merge.py`: rebuilds `catalog/`, `areas.tsv` and the dump's `index.tsv`.
- `tools/audit_lint_all.sh`: lints every table against its catalog slice (`tools/audit_check.sh` checks one).
- `tools/agent_prompt.md`: the prompt the audit agents ran with.

## Not tracked: the dump (`data/FsmDump/`)

Every `Ref` cell (`scenes/<Scene>.fsm.txt:<line>`) points into the dump, so refs are valid only for this build:

| What | Value |
|---|---|
| Game build | `1.0.30000` (`Constants.GAME_VERSION`; Steam and GOG are the same build) |
| Assembly-CSharp.dll sha256 | starts with `70713bb2b1f1b0c4` |
| Dump tool | `spikes/FsmDump`, commit `97b70b0` |
| Dumped | 2026-10-24, GOG copy |

- `scenes/`: FSM and object dump of every scene (245 MB). Regenerate: install FsmDump (`make install`), press F8 in game.
- `decomp/`: decompiled Assembly-CSharp (28 MB). Not shared: it is the game's code. Regenerate with any C# decompiler.
- `index.tsv`: every use of every element (52 MB). Regenerate: `uv run docs/sync-audit/tools/merge.py`.

`merge.py` reads the dump from `data/FsmDump/`; set `SILKSONG_DUMP` to use another path.
