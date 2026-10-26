# hornet_multi_wounder (FSM template) [note 2026-10-26, from a playtest]
Chomp/bite collider on enemy parts (958 uses, Bone Worm, Crypt Worm and others): on trigger with the hero layer it checks invincibility and parry, sends a MULTI WOUND event to the local hero and MULTI HIT CONNECT to its enemy (which starts the hold).
Coop: it sits under the enemy's HealthManager, so a mirrored copy replayed the owner's Hit and wounded the mirror's hero too: one bite hurt both players. Each game runs its own (`SyncedObjects.HeroWounders`); the hold reaches the owner as a relayed event.
