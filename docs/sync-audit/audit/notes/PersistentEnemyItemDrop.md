# PersistentEnemyItemDrop [note by agent 2.02]
On the enemy: remembers in SceneData.PersistentBools (droppedItemID) whether the enemy's one-time item drop (e.g. Rock Roller loot) was already given; checks at spawn, sets the flag when the drop happens.
Coop: if enemy death is shared, both games drop the one-time item, so the pickup must be taken once; the flag is saved per game. Low risk once death is shared.
