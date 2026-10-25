# AirZone [note by agent 2.12]
Steam blow region on exhaust columns: every FixedUpdate adds a random upward force (Random.Range) to particle rigidbodies and tells corpses (layer 26) they are in an air zone. Does not touch the hero (the hero is pushed by the column's Damager and other components).
Coop: visual and corpse physics only.
