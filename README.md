# SilksongCoop

Two-player coop mod for Hollow Knight: Silksong (BepInEx plugin, `src/SilksongCoop`).

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

### When a coop problem is reported

1. Find the element(s) in the dump and their catalog/audit rows.
2. Look up every other use in `index.tsv`, and any shared cause (targeting action, needolin, Player-tag triggers,
   room variants) in the mechanics table, which is created after the audit.
3. Fix the code.
4. Update the ledger for every affected row and mechanic: our status (open / covered / partial / tested / broken /
   won't do) and the `file:line` that handles it.

Audit agents never see or edit our status columns; those are added to their tables afterwards by a script.
