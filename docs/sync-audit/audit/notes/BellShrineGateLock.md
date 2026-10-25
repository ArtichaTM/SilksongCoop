# BellShrineGateLock [note by agent 2.11]
Grand Gate lock display: one bell per bell shrine, each tied to a PlayerData bool (shrine rung). While the hero plays the needolin at the gate it jitters the bells in sequence and, if all PD bools are true, fires GATE LOCKS COMPLETE so the Boss Scene FSM opens the gate. Reads PlayerData only.
Coop: gate opens only for a hero whose own PD has every bell shrine bool true.
