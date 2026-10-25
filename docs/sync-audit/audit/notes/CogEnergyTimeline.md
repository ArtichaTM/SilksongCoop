# CogEnergyTimeline [note by agent 2.14]
UnlockablePropBase that scrubs a PlayableDirector by an "energy" value (0..360). Open() animates to full energy (from a lever or ring-drop unlock); Opened() snaps to full (scene reload with the unlock already saved).
At full energy it activates the real bench interactable (Cog_Bench retractor) and an extra object. No PlayerData itself; the unlock state comes from whoever calls Open/Opened.
Touches: bench interactable, timeline visuals, audio. No hero, no RNG.
