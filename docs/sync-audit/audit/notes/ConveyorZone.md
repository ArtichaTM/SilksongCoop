# ConveyorZone / ConveyorSpeedZone [note by agent 2.10]
Trigger zones that push anything with ConveyorMovement, and the hero (cState.inConveyorZone, SetConveyorSpeed), at a fixed speed. Armed only after the hero is in position.
Touches: hero movement only, no PlayerData. Each game applies it to its own hero; a remote avatar is not driven by it.
