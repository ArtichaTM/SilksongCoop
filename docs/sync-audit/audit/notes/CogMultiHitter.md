# CogMultiHitter [note by agent 2.13]
Spike cog hazard collider: OnTriggerEnter2D of the local hero (layer 20, not invincible) starts a 0.35s grind (WOUND START to the hero, cog freezes) then DamageHeroDirectly. Sends COG DAMAGE / HeroDamaged events.
Coop: hurts only the local hero; the partner's mirrored body is not a hero collider, so it does not trigger it.
