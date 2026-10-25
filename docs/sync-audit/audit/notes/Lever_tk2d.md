# Lever_tk2d [note by agent 2.02]
tk2d nail-hit lever (IHitResponder on the lever object itself, not a HitResponse component). First nail hit: activated=true, optional PlayerData bool, hit anim, after openGateDelay calls Open() on connectedGates and sends OPEN to fsmGates FSMs (and camLocks off, optional EventRegister event). Saved via PersistentBoolItem; on load OnSetSaveState replays Opened()/ACTIVATE.
Coop: the hit is seen only in the hitting game, so the other game opens the gate only on its next load of the room. Share the first hit; each game then replays Hit and its own gate open.
