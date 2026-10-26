# HitRigidbody2D [note 2026-10-26, from a playtest]
IHitResponder on swinging bodies (hanging sacks, chains, rings; 1449 uses): a hit pushes the Rigidbody2D by a fixed impulse, optionally recoils the hero and passes the hit on to a Breakable (`passHitToBreakable`).
Coop: hanging sacks are pogo targets, so a swing seen in one game only puts the partner's sack elsewhere. Every hit is replayed on the partner's copy (HitResponseSync); under a HealthManager it is left to enemy sync.
