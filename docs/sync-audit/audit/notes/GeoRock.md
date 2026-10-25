# GeoRock + FSM "Geo Rock" [note by agent 2.02]
Breakable-by-nail rock: ReceivedDamage -> flings Geo Small/Med pickups per hit (random), counts Hits down; hits left is saved in SceneData.geoRocks (on SavePersistentObjects). Not under HealthManager, no HitResponse component.
Coop: each hit is local and loot is rolled per game, so both players can farm their own copy; hits left reach the other game only on its next room load. Share hits (or accept per-game loot).
