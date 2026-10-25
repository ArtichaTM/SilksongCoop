# TrapBridgeFlip [note by agent 2.13]
TrapBridge subclass: Open() plays a flip animation (direction chosen by which side of the bridge the LOCAL hero stands on), then swaps closed/opened collider sets and toggles activateOnOpened objects. No PlayerData.
Coop: opened by a trigger or lever in one game only; the flip direction depends on the local hero position.
