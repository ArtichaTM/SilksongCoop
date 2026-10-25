# CheckHeroPerformanceRegion (+V2, base) [note by agent 2.02]
Enemy reacts to the hero playing the needolin: asks the singleton HeroPerformanceRegion (lives on the LOCAL hero) for the affected state at the enemy's position and fires SING/ALERT/etc events; also listens to NoiseMaker.NoiseCreatedInScene.
Touches only local hero state; no PlayerData. Coop: the room owner only sees its own needolin, the partner's needolin never affects owner-run enemies (not patched in TargetingPatches/AlertPatches).
