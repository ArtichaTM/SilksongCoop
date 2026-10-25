# AdditiveLoreSceneController [note by agent 2.05]
While the local hero plays Needolin inside a trigger (and a PlayerDataTest passes) it fades to a "lore" vision: additively loads another scene, offsets the scene roots, sends start/end EventRegister events. Ends when the hero stops playing.
Touches: HeroPerformanceRegion (local hero only), scene loading, ambient light, screen fader. Coop: purely per-player presentation; the partner only sees the performer's needolin animation.
