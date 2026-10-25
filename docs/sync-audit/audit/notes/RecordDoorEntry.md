# RecordDoorEntry [note by agent 2.12]
Hooks TransitionPoint.OnBeforeTransition: writes the current scene name into a PlayerData string field (pdFromSceneName); for maze entrances also sets PreviousMazeScene/PreviousMazeDoor/PreviousMazeTargetDoor.
Coop: written per player at the moment that player leaves; the partner's value stays as it was.
