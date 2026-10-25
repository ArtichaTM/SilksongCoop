# ShopOwner (ShopOwnerBase) [note by agent 2.02]
Holds a shop stock list (ShopItem[]). The NPC dialogue FSM sends "SHOP UP", which opens the shop menu for the interacting hero. Purchases take currency from that hero and mark stock as bought in PlayerData (limited-stock items disappear after one sale).
Coop angle: limited stock bought by one player is not bought in the other's save; shop UI and wallet are strictly per-player.
