# ProjectileBreakTrigger [note by agent 2.09]
Trigger collider: any IBreakableProjectile entering it gets QueueBreak (isWall flag). Used on walls (Bell Wall Tall) so thrown/fired projectiles shatter there. Touches only the projectile; no PlayerData, no hero. Wall persistence is on a separate PersistentBoolItem.
