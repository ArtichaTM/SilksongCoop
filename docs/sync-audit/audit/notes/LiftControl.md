# LiftControl (+ LiftPlatform, TempPressurePlate call plates) [note by agent 2.08]
Multi-stop lift: a SimpleButton / doorCloseTrigger inside it (hero stands there) or a call plate at a stop runs MoveToStop; doors open per stop. Optional unlock by ItemReceptacle or a CallPlateUnlocks plate. Persists `isUnlocked` (PersistentBoolItem) and `currentStop` (PersistentIntItem); entry gate can override the start stop.
Camera is clamped while moving (controlCamera). Similar to ManualLift but multi-stop and persisted; position lives only in the local game's coroutine.
