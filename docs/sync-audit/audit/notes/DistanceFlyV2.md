# DistanceFly / DistanceFlyV2 / V3 / Smooth / Horizontal [note by agent 2.02]
Flying enemy keeps a distance/height from `target` (an FsmGameObject, usually var:Hero from GetHero) every frame by setting velocity. Hero-targeting movement action.
Coop: NOT in TargetingPatches.Actions (only ChaseObject*, FaceObject*, GetDistance*, CheckTargetDirection, GetAngleToTarget2D), so flyers using it always orbit the room owner's own hero, even if the partner is nearer / the only one alive.
