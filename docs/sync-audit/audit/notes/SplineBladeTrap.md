# SplineBladeTrap [note by agent 2.16]
Trap: TriggerBladeMove (from a TrapPressurePlate's OnPressed) pops a pooled blade, which after bladeEmergeDelay runs along a spline at bladeSpeed, then returns to the pool. Blade has DamageHero and DamageEnemies plus a WallDust FSM (camera shake, particles).
Touches only its own blades; no PlayerData, no RNG, nothing persistent. Trigger = plate press only, so the whole run replays from that single event.
