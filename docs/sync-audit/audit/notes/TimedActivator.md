# TimedActivator (+ActivatorPlatform, CoralCrustTreeBranchStates) [note by agent 2.11]
Hit (HitResponse or harpoon) on the activator makes its child ActivatingBase objects (tree branches, platforms, spike hazards) grow outward in distance-delayed waves for `duration` seconds, warn, then retract; deactivateOthers shuts other activators first. ActivatorPlatform platforms are breakable by hits (hitsToBreak).
Touches: scene objects only (timers, colliders, hazards). No PlayerData, no persistence.
