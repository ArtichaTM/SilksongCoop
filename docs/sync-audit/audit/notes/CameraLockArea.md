# CameraLockArea [note by agent 2.01]
Camera bounds volume: when the LOCAL hero is inside, the main camera clamps to the box (min/max x/y). No PlayerData, no gameplay effect. Used in arenas together with BattleScene.camLocks.
Coop: per-player camera only; no sync needed.
