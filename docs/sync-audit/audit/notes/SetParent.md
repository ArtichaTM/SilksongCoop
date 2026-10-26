# PlayMaker SetParent [note by main 2026-10-26]
FSMs move objects in and out of their parent mid-fight: BoneBeast DropBell (3.30) goes to the scene root in Position and back under the boss arena in Dormant; its Pt Rock leaves on Init. The random drop offset (RandomFloat in Position) and bounce side (RandomFloatEither in Drop) are rolled after the move.
Coop: an object named by its hierarchy path has a different path after the move, so a name taken before it no longer finds it.
