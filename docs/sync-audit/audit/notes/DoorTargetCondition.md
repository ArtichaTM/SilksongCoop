# DoorTargetCondition [note by agent 2.17]
On Start sets a TransitionPoint's target scene to targetIfTrue or targetIfFalse according to a PlayerDataTest.
Touches: reads PlayerData, rewrites the door destination once.
Coop: two players with different PlayerData walk the same door to different scenes.
