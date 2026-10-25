# LoopMovementTrack [note by agent 2.14]
Moves its child transforms (e.g. spinning cog platforms) along a start-end line every frame, phase = Time.time / loopTime.
Time.time is time since the game process started, so two games show the same track at different phases.
Touches: transforms only (often solid platforms). No hero, no RNG, no saved state.
