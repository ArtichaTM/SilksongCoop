# SilksongCoop

Two-player coop mod for Hollow Knight: Silksong (BepInEx plugin, `src/SilksongCoop`).

The host must be reachable on its UDP port (`[Host] Port`, 26950 by default): forward that port on the router,
or play over a LAN or a virtual LAN (ZeroTier, Tailscale, Radmin VPN). There is no NAT traversal or relay.

## Building

`make verify` runs every check. To install into a game, copy `local.mk.example` to `local.mk` and list the game
root folders in it. `make install` builds the plugin into each game's `BepInEx/scripts` (ScriptEngine
hot-reloads it), and `make install-dev` also installs the dev tools (FsmDump). Players use the loader
(`make loader`), which downloads the nightly release built from `main`.

Needs the .NET SDK from `global.json`, `uv` and GNU make. On Windows run make from Git Bash (make itself:
`winget install ezwinports.make`).

## Sync ledger: the source of truth for what gets synced

Every scene element of the game was dumped and audited for coop. The ledger records what each element needs, what
our code does about it, and what has been tested. It lives in `docs/sync-audit/` (layout and dump details in its
`README.md`); the raw dump it points into stays untracked in `data/FsmDump/`.

- `docs/sync-audit/audit/<NN>_<Area>.md`: per-scene tables (what coop needs); `audit/notes/<Type>.md`: game type notes.
- `docs/sync-audit/catalog/<Area>.md`: each element listed once, in the first scene it appears in.
- `docs/sync-audit/areas.tsv`: in-game area name for each scene prefix (report bugs by area name).
- `data/FsmDump/scenes/`: full dump of every scene's FSMs and objects; `Ref` cells point here.
- `data/FsmDump/index.tsv`: every use of every element. `grep <id> index.tsv` lists all places a fix touches.
- `data/FsmDump/decomp/`: decompiled Assembly-CSharp.
- `docs/sync-audit/verdicts/<NN>.md`: how to patch each audited element and whether it is needed.

### Recommended way to develop

Work slice by slice through four stages: **Dump** (`data/FsmDump/`) -> **Audit** (what each element does,
`audit/<NN>_<Area>.md`) -> **Verdicts** (how to patch it and whether it is needed, `verdicts/<NN>.md`) -> **Code**.
Verdict file rules and column values: `docs/sync-audit/README.md`, "Verdict files".

### Refer to places by verdict number

In test steps, reports, notes and summaries, name a place by its verdict item (`4.23`), not by scene or row
(`Bone_15`, `Bone_15.7`). The dev teleport menu (F5 in dev mode) jumps straight to every verdict item (listed as
`04.23`, table in `docs/sync-audit/teleports.tsv`), so any claim can be checked in game. Add the scene name only
next to the number, never instead of it.

### When a coop problem is reported

1. Find the element(s) in the dump and their catalog/audit rows.
2. Look up every other use in `index.tsv`, and any shared cause (targeting action, needolin, Player-tag triggers,
   room variants) in the mechanics table, which is created after the audit.
3. Fix the code.
4. Update the ledger for every affected row and mechanic: our status (open / covered / partial / tested / broken /
   won't do) and the `file:line` that handles it.

Audit agents never see or edit our status columns; those are added to their tables afterwards by a script.
