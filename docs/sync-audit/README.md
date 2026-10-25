# Sync audit ledger

Every scene element of Silksong, audited for coop: what each element needs, what our code does about it, and what has
been tested. The tables are the per-element truth; `mechanics.md` (added with the status columns) summarises the shared
causes and links to the rows each one covers.

## Tracked here

- `audit/<NN>_<Area>.md`: per-scene tables (what coop needs). Columns: `#`, Object, FSM / C#, Does, Verdict
  (`sure` / `maybe` / `no`), Why, What to sync, Design choice, Ref. Each table ends with `## Summary`.
- `audit/notes/<Type>.md`: short explanations of game C# types and PlayMaker actions.
- `verdicts/<NN>.md`: how to patch each audited element and whether it is needed (rules: "Verdict files" below).
- `catalog/<Area>.md`: each element listed once, in the first scene it appears in (`scene_order.txt` order).
- `areas.tsv`: in-game area name for each scene prefix (Blasted Steps and Grand Gate are not mapped yet).
- `scene_order.txt`: scene walk order used by the catalog.
- `tools/merge.py`: rebuilds `catalog/`, `areas.tsv` and the dump's `index.tsv`.
- `teleports.tsv`: the dev teleport menu's table (F5 in dev mode), built by `tools/teleports.py` on `make`.
- `tools/audit_lint_all.sh`: lints every table against its catalog slice (`tools/audit_check.sh` checks one).
- `tools/agent_prompt.md`: the prompt the audit agents ran with.

## Verdict files (`verdicts/<NN>.md`)

The rules live here; a verdict file holds only a one-line summary of what it covers, then its table.
- Only `sure` and `maybe` audit rows; `no` rows need nothing. Rows with one shared answer are grouped.
- One file per slice, titled `# Verdicts 2.NN: <Area>`, items `N.MM` with N = the file number (`02.md` holds `2.01` ..), unique across files. Columns: `#`, Row,
  Element, Trigger, Runs, Partner sees, Result, Partner, Status, Code, Note.
- Cells hold the target; Status says whether the code does it. An empty cell is the common case (Trigger `either`,
  Partner `free`). Note is short and only for gap and check rows. Hard limit: 5 values per column.

| Column | Short description | Values |
|---|---|---|
| Trigger | who can start it | `local` (only that game's hero), `either`, `auto` (timer, NPC, room load, boss) |
| Runs | who simulates it | `each` (both games run it, the event is replayed), `owner` (room owner, the other mirrors), `host` |
| Partner sees | the event in their own world (not your Hornet) | `live`, `save` (after a room reload, shared save), `never` |
| Result | what it writes | `shared`, `per-player`, `one-take` (a copy per game, only the first taker gets it), `none` |
| Partner | what happens to them | `free`, `wait` (start waits for both), `wait*` (starts locally, held until both are in: arena gates close for the early player, no fight yet; every boss arena), `pull`, `both-play` (each game plays it all: bellway travel, memories) |
| Status | | `done` (code exists, mostly untested), `tested` (works in game), `check` (covered as a side effect), `gap`, `later` (design delayed) |
| Code | which module does it | `Enemy-A` / `Enemy-S` = `EnemySync` Authority / Shared mode, `Waits` = `PartnerWaits` (+ `WaitPoints`); the rest are module names without `Sync` |

Fill Runs and Status from facts, not guesses:
- The object's components in `scenes/<Scene>.objects.txt` and its FSM path in `.fsm.txt`, run through
  `EnemySync.Classify`: HealthManager or a `Boss Scene` parent = Authority; a PersistentBool/IntItem on the FSM's
  object, `BG Control`, or an FSM name containing Lever/Gate/Lift/Elevator/Platform/Bridge = Shared; NPCs and
  hero-control FSMs = None. The keyword test reads the FSM name, so plain `Control` FSMs miss it.
- Then the other syncs: Breakable, HitResponse, Lever, Plate, Platform, Toll, Shop, Pickup, Memory (`memory_orb`
  only), BellBeast, WaitPoints, Pulls.
- All PlayerData is shared except the `FieldRules` per-player and loadout fields, so "quest per player or shared" is
  already shared.
- A mirror applies states with `PlayMakerFSM.SetState`, which runs their actions, so random rolls inside them roll
  again. Hazards without a HealthManager get no positions, so their random X or velocity can differ.

Gap families seen so far: unsynced IHitResponder types (`BreakableHolder`, `RosaryCache*`, `HealthCocoon`,
`ThreadSpinner`, `LifebloodPustule`, `GeoRock`) and `VinePlatform`; `Control` FSMs that write PlayerData without a
persistent item; FSM pickups with a hero cutscene (granted twice); memories without `memory_orb`; allies without a
HealthManager (double damage); random rolls run again by mirrors; per-game spawners.

After a verdict file, list the gaps as numbered groups and pick which to implement before writing code.

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
