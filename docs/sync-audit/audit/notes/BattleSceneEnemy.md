# BattleSceneEnemy [note by agent 2.13]
Tiny switch on enemies that belong to a battle wave: SetActive(bool) enables or disables every PlayMakerFSM, AlertRange and LineOfSightDetector under it. No PlayerData, no hero reads.
Coop: the enemy stays frozen until the local BattleScene calls SetActive, so it only wakes in the game whose hero started the battle.
