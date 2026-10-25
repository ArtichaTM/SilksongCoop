# CrankPlat [note by agent 2.17]
Platform driven by a crank HitResponse: each nail hit adds hitForce speed, the platform slides toward endPoint and falls back by returnForce; reaching the end completes it (optional PersistentBoolItem, can re-arm when the hero drops below). Hit direction reads the local hero's x for up/down hits.
Touches: platform position (time-integrated), optional scene bool. No PlayerData, no RNG.
Coop: position depends on hit timing, so each hit must reach both games or the platform diverges; the riding hero moves with it.
