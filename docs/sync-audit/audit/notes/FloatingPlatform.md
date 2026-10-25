# FloatingPlatform / FloatingObject / HeroPlatformCollisionSplit [note by agent 2.07]
FloatingPlatform bobs by sine and dips when a collider tagged Player lands on top (visual only, random phase in Start). FloatingObject is a sine bob. HeroPlatformCollisionSplit gives the hero one platform collider and everything else another (Physics2D.IgnoreCollision with the hero collider).
Coop: purely visual, except the collision split which assumes only one hero collider.
