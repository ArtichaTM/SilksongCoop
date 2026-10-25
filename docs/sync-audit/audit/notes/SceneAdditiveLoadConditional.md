# SceneAdditiveLoadConditional [note by agent 2.02]
On scene Start evaluates PlayerDataTest/QuestTest (plus entry-gate white/black lists) and additively loads either sceneNameToLoad or altSceneNameToLoad (boss/event overlay scene, e.g. Bonetown -> Bonetown_boss). Optionally sets a PD bool on load.
Coop angle: the decision reads the LOCAL PlayerData, so two players with different flags load different overlays for the same room.
