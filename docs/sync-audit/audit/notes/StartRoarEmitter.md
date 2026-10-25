# StartRoarEmitter (+V2) [note by agent 2.10]
PlayMaker action: moves the camera-attached "Roar Wave Emitter" to a spawn point, sends ROAR WOUND CANCEL to the LOCAL hero when stunHero is set (stuns it), and plays a shockwave/burst, with optional camera shake. Boss intros and flame waves use it.
Touches: local hero control state and camera only. A hero that is far away is still stunned if the action runs with stunHero=True.
