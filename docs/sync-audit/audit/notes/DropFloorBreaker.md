# DropFloorBreaker [note by agent 2.11]
Collapsing tower stalactite/floor: Break() (called from a Breakable/trigger) makes the rigidbody fall, breaks floor segments at Y thresholds and activates a camlock; Broken() restores the final state on load. Has a PersistentBoolItem, so the collapse is a one-time persistent event.
Touches: scene geometry, camlock, persistent scene bool.
