# SilkRationMachine [note by agent 2.13]
Toll machine that sells silk rations for rosaries: stock = startCount minus purchasedRationsCount (PersistentIntItem), may jam by UnityEngine.Random after minPurchaseToJam. FSM Behaviour takes currency from the local hero and drops one ration pickup.
Coop: stock count, the jam roll and the rosary payment are local; two players can both buy the last ration.
