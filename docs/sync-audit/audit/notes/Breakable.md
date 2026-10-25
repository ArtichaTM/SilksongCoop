# Breakable [note by agent 2.01]
Nail-breakable prop: on hit spawns debris and optional random item/geo/silk drops (UnityEngine.Random), AddSilk on the local hero, optional PersistentBoolItem so it stays broken (written on SavePersistentObjects). Hit comes from a nail/spell hit in the local game.
Coop: partner hits are not replayed live; the breakable stays whole in the other game until it reloads the persisted flag. Pure decor ones are harmless; blockers matter.
