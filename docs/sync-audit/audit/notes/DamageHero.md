# DamageHero [note by agent 2.01]
Contact/hazard damage: on trigger or collision with the hero layer calls HeroController.instance.TakeDamage (local hero only); also tinks with nail/terrain. Used by enemies, spikes, falling stalactites.
Coop: damages only the local hero, so each game only hurts its own player; the other player is never hit by it in this game.
