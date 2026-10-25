# TempGate [note by agent 2.06]
C# gate bound to a TempPressurePlate: opens when the plate fires, closes again after closeDelay unless the keep-open tracker still sees something inside. Optional brokenPDBool (PlayerData) keeps it permanently open; optional PersistentBoolItem read at scene start.
Touches: animator and colliders, reads PlayerData/persistent bool. The keep-open check is a trigger volume, so only bodies present in that game hold it open.
