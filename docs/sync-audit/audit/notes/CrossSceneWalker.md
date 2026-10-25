# CrossSceneWalker [note by agent 2.09]
Fisher NPC that walks between scenes on a global clock: static Tick() advances PlayerData FisherWalkerTimer (0..68.5, with random 60-120 s idle at each end) while a scene with a walker is loaded; position is derived from the timer. Talking pauses the walker (PauseWalker/ResumeWalker).
Touches only PlayerData timer/direction/idle fields; no hero targeting.
