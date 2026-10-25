# WeaverLift [note by agent 2.11]
Weaver shaft teleporter: hero stands on plate, lift charges, then the hero is teleported (HeroController moved, control relinquished, camera shift) to the paired lift. Availability/active state is a PersistentBoolItem (weaver_lift_power_chamber) set by the power-chamber read; static _isTeleportingLift allows one transport at a time.
Touches: hero position/control, camera, persistent scene bool.
