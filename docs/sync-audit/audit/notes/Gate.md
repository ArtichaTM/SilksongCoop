# Gate (UnlockablePropBase) [note by agent 2.03]
Animated gate/door prop. Open() (from a Lever, plate or FSM) sets activated, plays openAnim after an optional random delay and opens child props; Opened() snaps it to the open pose (used on scene load from a PersistentBoolItem, or DeactivateIfPlayerdataTrue hides it).
Touches only itself and its Animator; the persistent flag is written on scene save. Whoever calls Open() must run in each game for the gate to open live.
