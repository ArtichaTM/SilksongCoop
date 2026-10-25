# MovePlatform (UnlockablePropBase) [note by agent 2.12]
Platform that, on Open(), moves from inactivePos to activePos (with optional before/after offsets, gears, sway fsm), and snaps straight to activePos via Opened() when the unlock is already persisted. Driven by a lever/gate/unlock trigger elsewhere; no hero or PlayerData access itself.
Coop: both players ride it, so the Open trigger must run in both games; the motion itself is deterministic.
