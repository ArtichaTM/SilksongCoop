# BreakableTriggerBreak [note by agent 2.10]
Trigger collider that calls Breakable.BreakSelf() when a collider on layer 9 or 20 (the hero) enters; HeroSlide mode additionally needs SlideSurface.IsHeroSliding. Only the local hero's collider qualifies.
Touches: the linked Breakable (debris, optional PersistentBoolItem). Peak_01 uses it for white ball props the hero rolls through.
