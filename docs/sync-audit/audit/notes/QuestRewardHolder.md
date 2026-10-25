# QuestRewardHolder (+ GenericPickup) [note by agent 2.13]
Holder that shows a reward pickup once its quest IsCompleted; GenericPickup is an interact-to-take prompt (hero plays Collect animation 0.75s, then PickupAction gives quest.RewardItem and sets the PlayerData bool pickupPdBool). Empty after pickup.
Coop: the pickup gives the reward to the interacting hero only; the other game keeps showing it until the PD bool arrives and the scene reloads.
