# MemoryOrbSource (+ MemoryOrbSourceGroup) [note by agent 2.17]
Spawns a needolin screen-edge/on-screen glow pointing at this object while the local hero plays the needolin and IsActive is true. The Group variant is active while any linked MemoryOrbGroup is not fully collected (reads the PlayerData bitmask).
Touches: reads PlayerData, spawns visuals. Coop: visual only.
