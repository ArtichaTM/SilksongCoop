# SimpleQuestsShopOwner [note by agent 2.10]
Shop-style list of quests (Courier quest giver) instead of goods. Builds the offered list from PlayerData (quest states, genericQuestsList, purchasedDlgBitmask, lastCompletedQuestNameString); buying a "quest" starts it and hands the delivery item to the buyer.
Touches: PlayerData quests/collectables of the interacting hero. Quest state is per-save, so each player has their own list and progress.
