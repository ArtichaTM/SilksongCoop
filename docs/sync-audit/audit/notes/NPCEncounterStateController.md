# NPCEncounterStateController (+ BasicNPCRepeatDialogueLeaveCondition) [note by agent 2.13]
Per-NPC encounter state stored in PlayerData (NotMet, Met, ReadyToLeave, AuthorisedToLeave, HasLeft). On scene load it hides the NPC when HasLeft, and may roll UnityEngine.Random against leaveProbability to leave. The Leave condition marks Met after the first dialogue and ReadyToLeave after a repeat.
Coop: the state advances in the game where the dialogue happened; a roll on load can make the NPC leave in one game only.
