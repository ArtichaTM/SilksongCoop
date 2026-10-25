# ShopMenuStock / ShopItem / ShopItemStats [note by agent 2.03]
Shop UI list owned by a ShopOwner NPC. Buying (ShopItem.SetPurchased) takes geo or shards from the local hero (CurrencyManager), sets the item's PlayerData bool/int (maps, tools, upgrades) and extra PD bools, may spawn items, and fires an event after purchase. Items hide once purchased or when conditions fail.
Everything is the buyer's own PlayerData and currency; the menu locks hero input and pauses nothing for others. Limited stock is tracked in PlayerData, not in the scene.
