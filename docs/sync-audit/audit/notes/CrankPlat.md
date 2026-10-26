# CrankPlat [note by agent 2.17]
Platform driven by a crank HitResponse: each nail hit adds hitForce speed, the platform slides toward endPoint and falls back by returnForce; reaching the end completes it (optional PersistentBoolItem, can re-arm when the hero drops below). Hit direction reads the local hero's x for up/down hits.
Touches: platform position (time-integrated), optional scene bool. No PlayerData, no RNG.
Coop: position depends on hit timing, so each hit must reach both games or the platform diverges; the riding hero moves with it.
Coop [fix 2026-10-26]: hits are replayed (HitResponseSync), but each game integrates its own climb, so a borderline hit completes it in one game only. ReplaySync.OnLocalCrank relays the completion (CrankPatches on SetComplete, only from HitRoutine); the partner's copy gets posT 1 and finishes through its own HitRoutine (end audio, OnCompleted, returnOnHeroBelow re-arm).
