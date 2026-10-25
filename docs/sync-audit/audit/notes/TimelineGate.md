# TimelineGate [note by agent 2.06]
C# UnlockablePropBase gate: Open() (called by Lever/ItemReceptacle/plate) sets `activated`, waits a random openDelay, then plays a PlayableDirector timeline (gate animation, camera shake, collider removal). Saves `activated` through its PersistentBoolItem; on scene load a saved true jumps straight to the end of the timeline.
Touches: scene persistent bool only, no PlayerData, no hero.
