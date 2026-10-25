# PersistentBoolItem / PersistentIntItem [note by agent 2.03]
Per-object saved flag: reads SceneData on Start (OnSetSaveState / FSM var "Activated"), writes it back only when GameManager.SavePersistentObjects fires (scene exit, bench, save) - not at the moment of change.
Coop: a flag flipped in one game reaches the other only as saved scene data, applied on its next load of that room; the live object never changes. A live effect needs the triggering event shared.
