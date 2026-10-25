# TickedPendulum + TimedTicker [note by agent 2.12]
TimedTicker fires a registered EventRegister event ("TICK" / "TICK HALF") every tickDelay seconds (or a multiple of a parent ticker); TickedPendulum swings a spike pendulum one way per tick over swingTime. No hero or PlayerData access; the pendulum's DamageHero only hurts the local hero.
Coop: phase depends on when each game loaded the scene, so the two games show different swing positions; only visual disagreement, damage is local.
