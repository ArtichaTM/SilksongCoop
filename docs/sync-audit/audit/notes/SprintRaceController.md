# SprintRaceController / SprintRaceLapMarker / SplineRunner [note by agent 2.18]
Sprintmaster race minigame. SprintRaceController counts laps for the LOCAL hero (HeroController.instance) through SprintRaceLapMarker triggers (wrong marker order disqualifies) and for an AI SplineRunner that runs a spline path and rubber-bands its speed to the hero distance. On finish it fires RaceCompleted(didHeroWin) and sends raceEnd events.
Touches: hero position/control/silk (via the Runner FSM), HazardRespawnTrigger.IsSuppressed, PlayerData (WasInSceneRace, SprintMasterCurrentRace bitmask, quest), reward SavedItem.
Coop: the runner follows one hero only; a second hero entering markers is ignored (markers only accept HeroController/SplineRunner), so the race is single-racer.
