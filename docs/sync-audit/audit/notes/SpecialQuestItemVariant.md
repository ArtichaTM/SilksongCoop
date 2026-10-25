# SpecialQuestItemVariant [note by agent 2.13]
On Start of a corpse chunk: when UnityEngine.Random passes probability and the local quest is accepted but not completed or completable, it spawns a CollectableItemPickup for the quest item and flashes the sprite.
Coop: depends on local quest state and a roll, so each game shows its own item or none.
