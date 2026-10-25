# HeroShamanRuneEffect [note by agent 2.18]
On enable it calls Refresh: reads Gameplay.SpellCrest.IsEquipped and ZapImbuementTool.IsEquipped (local equipment), then scales the attached DamageEnemies damage and attack type (Spell), toggles a rune child, spawns a rune particle and tints sprites. Sits on hero slash animations (here the Phantom parry slash).
Touches: its own damager and visuals; reads the local hero's equipped crest/tool. Coop: visual and damage-multiplier only.
