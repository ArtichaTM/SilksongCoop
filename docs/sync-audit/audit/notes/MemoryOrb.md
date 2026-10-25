# MemoryOrb / SilkflyCloud / MemoryOrbGroup [note by agent 2.06]
C# needolin memory-orb challenge. A silkfly cloud leads along a chain of orbs; hero touch (OnTriggerEnter2D) collects each orb, with a time limit that makes uncollected orbs dissipate. MemoryOrbGroup stores collected orbs as a PlayerData ulong bitmask; completing sets a PlayerData bool and grants a collectable (Attunement Increase).
Touches: PlayerData bitmask/bool/counter (CollectedCloverMemoryOrbs), screen flash, hero touch triggers.
