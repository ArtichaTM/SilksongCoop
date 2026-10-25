# TransferVelocityOnCollide / Rb2dFollowWithVelocity / CharacterTint [note by agent 2.17]
TransferVelocityOnCollide: physics prop that bounces off terrain (wall ray) or passes velocity from a source body to a target; raises OnBounce. Rb2dFollowWithVelocity: rigidbody chases a target transform with velocity/TargetJoint. CharacterTint: sets a shader tint from the scene appearance region.
Touches: local physics bodies and shader props only; no hero, PlayerData or RNG.
Coop: decorative physics (laundry baskets) may drift apart, no sync needed.
