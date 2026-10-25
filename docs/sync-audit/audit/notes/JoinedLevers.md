# JoinedLevers [note by agent 2.13]
C# that links several Lever children: a hit on any lever sets all of them activated (SetActivatedInert) and plays the shared cog animator; state is read back from one PersistentBoolItem. Used for the paired Pipe Vent Hatch levers.
Touches: lever animations and the persistent bool only, no PlayerData. Coop: one lever hit must open the hatch in both games.
