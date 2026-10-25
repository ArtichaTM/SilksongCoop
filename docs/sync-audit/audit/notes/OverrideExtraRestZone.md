# OverrideExtraRestZone [note by agent 2.15]
On Start sets the scene manager's extraRestZone; when the player rests at a bench here, GameManager copies it into PlayerData.extraRestZone (respawn zone, used for the save-slot background). Cosmetic bookkeeping, per player; no hero state, no gameplay effect.
