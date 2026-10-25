# CogPlatController + CogPlat + CogPlatArm [note by agent 2.14]
Rotating cog platform: two TimedTickers (timers start at OnEnable) fire main and arm ticks; on each arm tick all CogPlatArms rotate and each CogPlat drops (collider off) and rises again at the end.
CogPlat also plays a "depressed" animation while something stands on it. Timing depends on when the scene loaded in each game.
Touches: platform colliders, animators. No hero reads, no RNG, no saved state.
