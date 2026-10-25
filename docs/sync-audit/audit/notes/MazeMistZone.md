# MazeMistZone (SceneTransitionZoneBase) [note by agent 2.12]
Trigger zone in Dust Maze scenes: when the local hero touches it, MazeController.ResetSaveData(), sets tempRespawnType/tempRespawnMarker "Death Respawn Marker", RespawningHero=true, then transitions to Dust_Maze_09_entrance (the maze fail/reset).
Coop: moves only the hero that touches it; the other stays in the maze with counters already reset in shared PlayerData.
