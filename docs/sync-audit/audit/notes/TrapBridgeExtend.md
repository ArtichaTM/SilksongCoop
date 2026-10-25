# TrapBridge / TrapBridgeExtend [note by agent 2.06]
C# bridge that extends when Open() is called (by a Lever or trigger via UnityEvent), plays an open anim, holds open while a TrackTriggerObjects tracker sees something inside (else a hold timer), then retracts. Extender variant also unlocks hidden levers once (persistent bool `activated`).
Touches: only animator, colliders, and its persistent bool. The hold test (insideTracker.IsInside) looks at objects in the trigger, so a remote player standing on the bridge in the other game may not hold it open.
