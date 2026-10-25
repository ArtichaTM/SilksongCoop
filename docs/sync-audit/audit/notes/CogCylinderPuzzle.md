# CogCylinderPuzzle + CogCylinderPuzzleCog [note by agent 2.14]
Each cylinder has several CogCylinderPuzzleCog; a nail hit on a cog lever calls HitMove(), rotating that cog one section. When every cog sits on its target section the cylinder completes, breaks its scaffold and sends CYLINDER COMPLETED to the puzzle FSM.
Completion is saved in a PersistentBoolItem (scene bool) on the cylinder; on reload it sends CYLINDER ALREADY COMPLETE. Cog positions before completion are NOT saved (local, reset on reload).
Touches: scene bool, puzzle FSM, camera shake. No hero, no RNG.
