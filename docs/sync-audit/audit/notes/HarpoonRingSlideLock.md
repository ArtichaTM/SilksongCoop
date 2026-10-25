# HarpoonRingSlideLock [note by agent 2.14]
A ring lock: when the hero hangs on the ring for dropDelay seconds it drops once, sends RING DROP IMPACT to the local hero, then calls Open() on its UnlockablePropBase list (gates, CogEnergyTimeline, etc.).
Completion is saved in a PersistentBoolItem; on reload it calls Opened() on the unlockables.
Touches: local hero, scene bool, unlockable props, camera shake.
