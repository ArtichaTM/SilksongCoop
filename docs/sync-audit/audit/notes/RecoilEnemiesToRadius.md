# RecoilEnemiesToRadius [note by agent 2.15]
Trigger sphere on grinder "Enemy Sucker": every object with a Recoil component inside the outer radius is pulled to the inner radius each frame (rigidbody push). Touches enemies only, no hero, no PlayerData.
Paired with a DamageEnemies hitbox and CorpseSplatter on the same grinder, so grinders drag and kill enemies and splat corpses.
