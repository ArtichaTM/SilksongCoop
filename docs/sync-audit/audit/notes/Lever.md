# Lever [note by agent 2.03]
C# nail-hit lever (IHitResponder, no FSM). On first nail hit: sets `activated`, optionally a PlayerData bool, animates, then after openGateDelay calls Open() on `unlockables` and sends OPEN/ACTIVATE to `fsmGates` FSMs and an optional EventRegister event. Saves via a PersistentBoolItem (SceneData.persistentBools).
Coop: the hit is seen only in the hitting game; the saved bool is written on scene exit or bench, so the other game opens the gate only on its next load of the room. Share the first hit; each game then plays its own open.
