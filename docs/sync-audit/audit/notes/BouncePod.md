# BouncePod (+HarpoonHook) [note by agent 2.02]
Bounce bloom: a nail/hit from the local hero pulls the hero to it and launches them (bouncePull -> DoBounce); it goes inert briefly, then re-arms. Static _currentBouncer ensures one pod at a time. HarpoonHook only raises hook start/end/cancel UnityEvents.
Reads/writes only the local hero's motion; no PlayerData or scene bools.
Coop angle: purely local per-hero movement; the only shared thing is the pod animation.
