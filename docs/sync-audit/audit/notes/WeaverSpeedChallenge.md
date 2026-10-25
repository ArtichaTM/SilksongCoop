# WeaverSpeedChallenge / WeaverSpeedPanel [note by agent 2.07]
Panels call RecordSpeed on HeroController.instance (abs linearVelocity.x, Quickening, Sprintmaster tool) and light up; the challenge takes the minimum threshold across panels and sends a completion event to its FSM, which sets PlayerData CompletedWeaveSprintChallenge / ...Max and opens the reward.
Coop: reads only the local hero's speed; a partner running through lights nothing in the other game.
