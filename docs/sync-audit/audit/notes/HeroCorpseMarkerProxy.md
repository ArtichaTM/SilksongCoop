# HeroCorpseMarkerProxy [note by agent 2.05]
Singleton marker in a scene the hero cannot leave normally (Bell Centipede arena): on death the cocoon is placed in the fast-travel NPC's scene (PlayerData.FastTravelNPCLocation) at a position read from a static variable.
Per-player death bookkeeping; no coop sync.
