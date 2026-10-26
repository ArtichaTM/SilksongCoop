# GeoRock + FSM "Geo Rock" [note by agent 2.02]
Breakable-by-nail rock: ReceivedDamage -> flings Geo Small/Med pickups per hit (random), counts Hits down; hits left is saved in SceneData.geoRocks (on SavePersistentObjects). Not under HealthManager, no HitResponse component.
Coop: each hit is local and loot is rolled per game, so both players can farm their own copy; hits left reach the other game only on its next room load. Share hits (or accept per-game loot).
Hits [main 2026-10-26]: every hit that counts enters state Hit (nail, Hit Skip Effects from a breaker) or Heavy Break (all hits left at once); both read Attack Direction. Replaying those states with the hitter's direction keeps hits left equal in both games; each game flings its own geo.
