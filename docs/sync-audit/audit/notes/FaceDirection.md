# FaceDirection / FaceObject / FlipScale families [note by main 2026-10-25]
PlayMaker facing actions flip the owner's localScale.x every frame: FaceDirection by the body's velocity (optionally playing a turn animation such as "TurnToIdle"), FaceObject* toward a target (usually the hero variable), FlipScale* outright. FaceAngle* rotates instead, FaceObjectTk2dSpriteScale flips the sprite.
Coop: in a game that only copies an enemy, its chase actions aim at that game's own hero, so these actions turn the enemy toward the wrong player and fight the facing copied from the other game.
