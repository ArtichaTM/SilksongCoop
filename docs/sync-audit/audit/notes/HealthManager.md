# HealthManager [note by agent 2.01]
Enemy HP/damage/death component: takes HitInstance, sends FSM events (TOOK DAMAGE, HIT, ZERO HP, EXTRACT, STUN DAMAGE), spawns corpse/loot (UnityEngine.Random), reads PlayerData.nailUpgrades for scaling, gives the LOCAL hero silk/recoil. With a PersistentBoolItem it saves isDead and stays dead on reload.
Coop: damage and death are only seen in the game where the hit landed; loot and silk go to the hitter's game.
