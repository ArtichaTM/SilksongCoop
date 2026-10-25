# SuspendedPlatform / SuspendedPlatformBase / SuspendedDropPropeller / SuspendedSwayPlat [note by agent 2.08]
Platform hung on a vine; the sibling SuspendedPlatformCut (see VinePlatform note) calls CutDown() on a nail hit. SuspendedPlatform disables its collider and fires onBreak; SuspendedDropPropeller drops a platform to dropTarget, then swaps in an active propeller and sends ACTIVATE to an updraft FSM.
State is one PersistentBoolItem (`activated`); on scene load OnSetSaveState replays the finished result. Touches only itself, the updraft FSM and camera shake (no PlayerData).
