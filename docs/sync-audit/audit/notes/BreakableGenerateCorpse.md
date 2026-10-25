# BreakableGenerateCorpse [note by agent 2.07]
Companion to Breakable: pre-instantiates a corpse object and flings it (random z, direction from hit) on BrokenHit; places it already-fallen when the Breakable loads as AlreadyBroken. No PlayerData; broken state is the Breakable's PersistentBoolItem.
Coop: corpse fling follows the break, no separate state.
