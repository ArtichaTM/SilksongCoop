# ManualLift (+ SimpleButton, TempPressurePlate, HeroPlatformStick) [note by agent 2.03]
C# carriage moving between leftTargetPos/rightTargetPos by moveTransform with its own velocity: a hero standing on SimpleButton (trigger) or on a call plate drives it; HeroPlatformStick parents the local hero to it. State lives only in Update (currentPosT/velocity), tilt via the "tilt_plat" FSM.
Coop: position lives only in the local Update and buttons see only the local hero, so each game moves its own carriage. The players ride different copies unless one game's position (or button state) is shared.
