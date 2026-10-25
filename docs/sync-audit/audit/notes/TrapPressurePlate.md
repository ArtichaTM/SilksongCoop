# TrapPressurePlate [note by agent 2.16]
Hazard plate: a TrackTriggerObjects tracker sees something stand on it, the plate sinks, then OnPressed (UnityEvent) fires once; it rises after raiseDelay once nothing is inside. Not persistent, no PlayerData.
Wired to SplineBladeTrap.TriggerBladeMove (Slab_13) or a spike trap (Slab_05); the trap has a reset timer that blocks the plate. Only the game whose collider stands on the plate runs the press, the trap output damages (DamageHero and DamageEnemies) only in that game.
