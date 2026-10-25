# FrostRegion (TrackTriggerObjects) [note by agent 2.03]
Cold zone: while the local hero is inside, frost builds at frostSpeed (modified by cloak/tools) and eventually damages; addOnEnter adds or removes frost on entry. Warmth regions (negative values or disabled frost) cancel it. Calls HeroController.AddFrost only.
Purely per-hero state; no scene or PlayerData writes.
