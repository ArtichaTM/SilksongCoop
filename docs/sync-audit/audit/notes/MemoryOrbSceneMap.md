# MemoryOrbSceneMap (+ MemoryOrbSceneMapConditional) [note by agent 2.17]
Needolin hint map: on scene Start it reads PlayerData bools/bitmasks per other scene (child name = PD field, "!" prefix inverts) and, while the hero plays the needolin, spawns screen-edge arrows pointing toward other scenes that still have uncollected orbs.
Touches: reads PlayerData only; visual effects only.
Coop: per-player hint, no sync needed.
