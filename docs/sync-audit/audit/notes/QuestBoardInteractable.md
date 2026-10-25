# QuestBoardInteractable [note by agent 2.02]
Interactable quest board (Bonetown). Reads the local hero's quest states from PlayerData/QuestManager; "Hand In Sequence" FSM deposits quest items, takes donation currency (rosaries/shards) and grants the quest reward item.
Writes: quest completion, currency removal, reward items, all on the interacting hero's own save. Control is taken from that hero during the deposit animation.
Coop angle: per-player economy and quests; the other player sees only the board animation, if anything.
