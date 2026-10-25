# SilkGrubCocoon [note by agent 2.06]
C# breakable cocoon hit with the nail: each hit gives the hero silk (HeroController.SilkGain), after hitsToBreak it breaks, flings a CollectableItem pickup, shakes camera, optionally sets/unsets PlayerData bools, and saves broken state in a PersistentBoolItem.
Touches: hero silk, item drop, PlayerData bools, persistent scene bool.
