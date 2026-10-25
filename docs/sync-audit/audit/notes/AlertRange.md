# AlertRange (TrackTriggerObjects) [note by agent 2.01]
Trigger box that reports whether the LOCAL hero (HeroController.instance) is inside, optionally with line of sight; read by FSM actions CheckAlertRange(ByName) to wake/aggro enemies.
Coop: patched by the mod's AlertPatches for enemies (per other notes); non-enemy users (chest "Hero Region", vine "Hit Cancel Range", Song gate "Leave Range") still read only the local hero.
