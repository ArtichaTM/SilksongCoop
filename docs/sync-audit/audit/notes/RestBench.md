# RestBench (+ RestBenchHelper, FSM "Bench Control") [note by agent 2.01]
Bench: C# only tells the hero it is near a bench; the FSM does the sit: blocks input, sets PD atBench, respawn marker (respawnScene/MarkerName/Type), refills health/silk/tools, updates map, may SaveGame, clears maggots, crow summons. Interact by the local hero.
Coop: respawn point and heal are per-player (own PlayerData); the other player only sees the sit animation. Do not share the respawn marker between players.
