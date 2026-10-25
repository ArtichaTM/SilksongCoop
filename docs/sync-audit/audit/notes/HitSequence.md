# HitSequence [note by agent 2.07]
Counts hits on an ordered array of HitRigidbody2D targets (optionally requiring one tool and the same hit source); wrong order resets, full order fires OnSequenceComplete and an EventRegister event after a delay. Touches no PlayerData itself.
Coop: only hits that reach the local targets count, so the sequence completes in the game where the hits were made.
