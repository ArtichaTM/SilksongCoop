# SlideSurface [note by agent 2.10]
Slope the local hero attaches to on contact and slides along (moves the hero body along a spline, jump or damage detaches). Sets static IsHeroSliding/IsHeroInside, used by BreakableTriggerBreak.
Touches: hero body/velocity and input only. No PlayerData, no scene bools. Per-player.
