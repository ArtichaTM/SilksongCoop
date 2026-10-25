# SilkcatcherPlant [note by agent 2.07]
Breakable plant (PersistentBoolItem). A nail hit destroys it and calls HeroController.AddSilk(2) on the local hero; non-nail breakers destroy it without silk. Random sprite/rotation/flip on Start is cosmetic.
Coop: each game destroys its own copy; a hit in one game gives silk only to the hitter and leaves the plant in the other game.
