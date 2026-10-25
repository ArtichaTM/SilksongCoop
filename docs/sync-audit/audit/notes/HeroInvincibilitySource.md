# HeroInvincibilitySource [note by agent 2.09]
While any instance is enabled (or Add()ed), static HeroInvincibilitySource.IsActive is true and HeroController.TakeDamage / damage checks ignore all damage to the local hero. Used on boss corpses/cutscene objects so Hornet cannot be hurt during death sequences.
No PlayerData writes; purely a per-process flag read by HeroController.
