# CorpseItems [note by agent 2.12]
On a corpse Start(), rolls loot with UnityEngine.Random from itemPickupGroups (probability, amount) and spawns a GenericPickup; picking up gives SavedItems (rosaries, materials, tools) to the local hero via DoPickupItems.
Coop: each game rolls its own loot for the same enemy, and the pickup is local, so the partner can see a different or no drop; one pickup per corpse is not shared.
