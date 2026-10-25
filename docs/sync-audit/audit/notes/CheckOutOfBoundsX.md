# CheckOutOfBoundsX [note by agent 2.18]
Update-polls its own transform.x once per enable; outside [xMin,xMax] it invokes onOutOfBounds (UnityEvent) a single time. Used on spawned ground spears in the Hunter Queen memory to clean them up when they fly off the arena.
Touches: only its own object. Coop: no hero reads, nothing to sync.
