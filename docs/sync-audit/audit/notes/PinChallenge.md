# PinChallenge [note by agent 2.04]
Small helper for the Ladybug pin gallery minigame: on events PIN CHALLENGE START/END it tracks the LOCAL hero's bound attack tool use (ToolItemManager.BoundAttackToolUsed) and temporarily disables the rest-bench interactable for 2 s. The game itself is the "Pin Challenge" FSM (camlock, MoveHeroToPosX, hit counter, rosary payout, PD pinGalleriesCompleted/PinGalleryWallet).
Coop: whole minigame is single-player by design (one hero thrown to a start pos, one target bug); needs a decision: independent per player, or only one player at a time.
