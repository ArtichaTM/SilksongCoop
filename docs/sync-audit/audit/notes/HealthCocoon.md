# HealthCocoon [note by agent 2.02]
Hit responder (nail or spell). First hit breaks it and flings pooled health pickups (UnityEngine.Random count, speed, angle); the broken flag is kept in a PersistentBoolItem and the cocoon stays gone on reload.
Coop angle: health pickups heal whoever touches them; one hit consumes the cocoon for the whole scene state, so the break needs sharing or each player gets their own copy.
