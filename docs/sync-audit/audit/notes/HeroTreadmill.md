# HeroTreadmill (+ ConveyorBelt, CurrencyObjectLimitRegion) [note by agent 2.15]
Treadmill in Under_03d: the hero captured by the ConveyorBelt (collision) running right drives belt speed and a needle gauge; the needle FSM spits Rosary pickups on a fair-random roll, up to a payout cap kept in a PersistentIntItem ("Hero Treadmill Rosaries Paid").
Reads only the colliding HeroController (CapturedHero event), writes the persistent payout count and spawns currency pickups. CurrencyObjectLimitRegion only caps how many rosary objects may exist in a box on low graphics tier (no gameplay effect).
