# SilksongCoop

Two-player coop mod for Hollow Knight: Silksong (BepInEx plugin, `src/SilksongCoop`).

## Sync ledger: the source of truth for what gets synced

Every scene element of the game was dumped and audited for coop. The ledger records what each element needs, what
our code does about it, and what has been tested. It currently lives in `data/FsmDump/` (gitignored); it moves to
`docs/sync-audit/` once the audit agents finish.

- `scenes/<Scene>.fsm.txt`, `.objects.txt`: full dump of every scene's FSMs and objects (regenerate with FsmDump F8, GOG copy).
- `catalog/<Area>.md`: each element listed once, in the first scene it appears in (`scene_order.txt` order).
- `areas.tsv`: in-game area name for each scene prefix (report bugs by area name; Blasted Steps and Grand Gate are
  not mapped yet).
- `index.tsv`: every use of every element (`id`, scene, object, dump ref). `grep <id> index.tsv` lists all places a fix touches.
- `audit/<NN>_<Area>.md`: per-scene tables written by the audit agents (what coop needs).
- `audit/notes/<Type>.md`: short explanations of game C# types and actions.
- `decomp/`: decompiled Assembly-CSharp.
- `tools/merge.py` rebuilds catalog and index; `tools/agent_prompt.md` is the audit agents' prompt.

### When a coop problem is reported

1. Find the element(s) in the dump and their catalog/audit rows.
2. Look up every other use in `index.tsv`, and any shared cause (targeting action, needolin, Player-tag triggers,
   room variants) in the mechanics table, which is created after the audit.
3. Fix the code.
4. Update the ledger for every affected row and mechanic: our status (open / covered / partial / tested / broken /
   won't do) and the `file:line` that handles it.

Audit agents never see or edit our status columns; those are added to their tables afterwards by a script.
