# DialDoorBridge [note by agent 2.13]
Rotating door bridge: a hit on a clockwise or counterclockwise Lever (OnHitDelayed) rotates the bridge 90 degrees (Door close anim, rotate, slam shake, door open). The rotation state is saved in a PersistentBoolItem. Locks out further rotation while a rotation runs.
Coop: a lever hit in one game rotates only that game; both games must agree on rotation state or the bridge geometry (colliders) differs.
