# StateChangeSequence [note by agent 2.05]
Ordered state machine saved in a PersistentIntItem: SetState(i) runs OnEnter (or OnReturn when loaded from save) UnityEvents and EventRegister events for every state up to i; the last state sets an optional PlayerData bool (isCompleteBool) and awards an achievement.
Used by Bell Shrines (bell released -> lever -> bench). Touches PlayerData, SceneData ints, events. Coop: the state advances only in the game that drove it; the other game replays it only after reload, so share the step index.
