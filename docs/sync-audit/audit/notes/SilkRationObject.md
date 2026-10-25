# SilkRationObject [note by agent 2.13]
Collectable silk ration (CurrencyObject): on pickup contact by the local hero it calls AddSilk (hitSilk[] per hit), spawns effects, may take several hits. Dropped from Citadel enemy corpses (Silk Ration Dropper rolls RandomFloat 1-100 for full or empty ration, none in Act 3).
Coop: the drop roll and the pickup are per game, so the partner can see a different ration; whoever touches it gets the silk.
