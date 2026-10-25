# SmashableTile [note by agent 2.11]
Floor tile that smashes when a collider tagged "Tile Smasher" (boss stomp, falling object) enters it, with smashChance (Random.Range) per tile; may be persistent (PersistentBoolItem) or pre-smashed by a PlayerData condition.
Touches: tile sprite/collider, optional persistent bool. RNG decides which tiles break.
