# ExtenderPlatsController / ExtenderPlatLink [note by agent 2.03]
UnlockablePropBase driven by a Lever: Open() -> Unfold() coroutine unfolds a chain of rotating links and then platforms (timers only, no RNG, no hero reference); isUnfolded saved in a PersistentBoolItem (SetUnfoldedInstant on load).
Coop: follows whoever triggers the Lever; if the lever press is synced (see Lever.md) the partner's chain unfolds identically. Standing platforms are local colliders (hero rides them in each game), nothing else to send.
