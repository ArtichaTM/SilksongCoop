# VinePlatform / SuspendedPlatformCut [note by agent 2.02]
Hanging platform held by a vine. A nail hit on SuspendedPlatformCut (IHitResponder, cutsToBreak hits) calls CutDown(): the platform falls, lands (camera shake, slam effect) and an enemy detector kills any HealthManager it touches. Saved via PersistentBoolItem so it stays down.
Coop angle: the cut is a world event any player can cause; the platform is walkable for both only after it has fallen in both games.
