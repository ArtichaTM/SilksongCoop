# TestGameObjectActivator [note by agent 2.01]
Chooses scene variants at load: evaluates a PlayerDataTest, quest states, equipped tools, entry-gate whitelist/blacklist and activates/deactivates two GameObjects (e.g. "Black Thread States" Normal World vs Black Thread World, "States/Intro Scene" vs "Outro Scene").
Coop: if the two players' PlayerData/quests differ they load different variants of the same room (different enemies, bosses, NPCs). Needs a rule (host decides, or use PD of the room owner).
