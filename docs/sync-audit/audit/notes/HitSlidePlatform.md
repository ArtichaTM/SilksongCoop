# HitSlidePlatform / HitSlidePlatformNode [note by agent 2.16]
Nail-hit puzzle platform: a nail hit on its TinkEffect moves it along a node graph in the hit direction (node.connectedNodes per direction), with a camera shake on arrival. While sliding it activates a DamageHero and a DamageEnemies collider; end nodes stop it.
State is a PersistentIntItem holding the current node index, restored on scene load (no PlayerData). Raises FirstHit event once. The hit is a local nail hit, so only the hitter's game moves the platform.
