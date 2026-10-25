# 18. Optional scenes (agent 2.18)

Slice: `catalog/Optional.md`. Common background for the memory scenes (Memory_*): a memory is a private scene the hero is sent into by a needolin thread-memory burst or an ability shrine, and the hero's health, silk and tools are stored then restored on exit (see `audit/notes/MemoryScene.md`). Rows marked `maybe` inside a memory scene all share one reason: they only matter if both players are in the same memory at the same time; if memories stay private, those rows drop to `no`. The scene-level rows (entry, rewards, exit) are `sure` either way.

## Abandoned_town

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abandoned_town.1 | Memory Group | Memory Sequence 3af905eddd | Drops memento item after thread memory completes | sure | The memory is played by one hero but the dropped item and its saved Activated flag exist per save, so the partner would never see or reach it. | The thread-memory-completed event; each game then plays the drop animation and keeps the normal pickup rules. | Memento is one shared item (first to grab wins) or one per player; whether either player playing the needolin counts | scenes/Abandoned_town.fsm.txt:28 |
| Abandoned_town.2 | Surface Scuttler ×10 | Behaviour 08b5d2937b | Ambient critter flees hero, digs away, respawns | no | Reacts only to the local hero and has no loot or saved state. | | | scenes/Abandoned_town.fsm.txt:199 |
| Abandoned_town.3 | Surface World Edge Old | Control 773d9c4f52 | Stops hero at world edge, forced dialogue | maybe | Locks and moves only the local hero, but writes encounteredSurfaceEdge which picks first versus repeat dialogue. | The encounteredSurfaceEdge flag through normal save-data sharing. | If both reach the edge together, each gets own walk-back (recommended) or only the first | scenes/Abandoned_town.fsm.txt:1579 |
| Abandoned_town.4 | town_grass ×8 types | TownGrass (C#) | Cuttable grass, vanishes when slashed | no | Cosmetic, not saved, resets on reload. | | | scenes/Abandoned_town.objects.txt:325 |

## Aspid_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Aspid_01.1 | Start Blanker (2) | Blanker Control f2084c34be | Screen-cover fade for roof area | no | Local screen effect only. | | | scenes/Aspid_01.fsm.txt:4468 |

## Chapel_Wanderer

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Chapel_Wanderer.1 | Pilgrim Trap Boulder | Control 8cec66b9b3 | Waits for TRAP, then activates boulder | sure | The trip wire fires in the game where it was cut (and only if it is on that hero's camera), so the other player never gets the boulder. | The wire-cut event (TRAP); each game replays the spikes and boulder itself. | | scenes/Chapel_Wanderer.fsm.txt:4524 |
| Chapel_Wanderer.2 | Bone_Boulder Summon | Control a6bd911631 | Boulder drops, damages hero 2 and enemies | sure | Hits whoever stands under it in each game, with random sprite and timing, so a missed trip leaves the partner unwarned. | Covered by the TRAP event in row 1. | Whether the trap re-arms on every scene entry (it does now) | scenes/Chapel_Wanderer.fsm.txt:4531 |

## Memory_Ant_Queen

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Memory_Ant_Queen.1 | Boss Scene | End Memory 0330f43521 | On boss defeat fades white, returns to Ant_Queen | sure | Scene change for one hero; the partner is left behind or the game states diverge. | The boss-defeated event so both leave together, if the memory is shared. | Pull partner out with the player, or leave partner where they are | scenes/Memory_Ant_Queen.fsm.txt:4 |
| Memory_Ant_Queen.2 | Audio Loop Carmelita Singing | Control 40abe8e2af | Singing audio loop with lowpass | no | Audio only. | | | scenes/Memory_Ant_Queen.fsm.txt:63 |
| Memory_Ant_Queen.3 | Bone Hunter Fly Chief ×2 | Control 5747a61d0c | Flying javelin thrower, aims at and evades hero | maybe | Aims at the local hero; see memory note above. | Wave enemy deaths if shared. | Private memory (recommended) or shared fight | scenes/Memory_Ant_Queen.fsm.txt:3767 |
| Memory_Ant_Queen.4 | Bone Hunter Fly Chief ×2 | Harpoon Evade (inactive for now) b69efac585 | Dodges incoming harpoon, marked inactive | no | Inactive in the dump and only reacts to the harpoon throw event. | | | scenes/Memory_Ant_Queen.fsm.txt:4043 |
| Memory_Ant_Queen.5 | right side colliders | FSM 612328faca | Removes side colliders when gates close | no | Arena geometry only. | | | scenes/Memory_Ant_Queen.fsm.txt:4483 |
| Memory_Ant_Queen.6 | Hunter_Queen_ground_spear_grind ×50 | Control 05e70804c6, CheckOutOfBoundsX (C#) | Ground spears rise when touched, then clean up | maybe | Hazard triggered by the boss dash and hurts whoever is there; see memory note above. | The boss grind-dash attack if shared. | | scenes/Memory_Ant_Queen.fsm.txt:4489, scenes/Memory_Ant_Queen.objects.txt:348 |
| Memory_Ant_Queen.7 | Hunter Queen Boss | Control de873286a2 | Carmelita boss: random dances and attacks at hero | maybe | Phases by HP and chases the local hero; fight is only local unless both are inside. | Boss HP and attack choice if shared. | Private memory per player (recommended), or true two-player boss | scenes/Memory_Ant_Queen.fsm.txt:5882 |
| Memory_Ant_Queen.8 | Hunter Queen Boss | Music Start 9d09676f01 | Starts boss music cue | no | Audio only. | | | scenes/Memory_Ant_Queen.fsm.txt:6994 |
| Memory_Ant_Queen.9 | Corpse Hunter Queen(Clone) | Death 7378832fb1 | Death sequence; sets defeatedAntQueen, metAntQueenNPC, afterRedMemory flag | sure | Writes story flags that change Ant_Queen later; one player's win leaves the other's save without them. | The three flags (defeatedAntQueen, metAntQueenNPC, defeatedAntQueenAfterRedMemory) through save-data sharing. | | scenes/Memory_Ant_Queen.fsm.txt:7113 |
| Memory_Ant_Queen.10 | NPC (on corpse) | Control 451a6496e7 | Interact: Hornet grabs the corpse's heart, cutscene | sure | Hides and moves the interacting hero through a scripted grab, and leads to the memory end. | The interact event, so the partner does not stand through it. | Freeze partner during the grab, or leave free | scenes/Memory_Ant_Queen.fsm.txt:7179 |

## Memory_Coral_Tower

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Memory_Coral_Tower.1 | Audio Spear Antic Controller | Control bbbabfbd47 | Loops spear windup audio | no | Audio only. | | | scenes/Memory_Coral_Tower.fsm.txt:4 |
| Memory_Coral_Tower.2 | Battle Gate Coral ×20 | BG Control a1aa3f53f5 | Arena gate closes at start, opens at end | maybe | Locks only the hero in the game that started the battle. | BG CLOSE and BG OPEN with battle start and end if shared. | | scenes/Memory_Coral_Tower.fsm.txt:40 |
| Memory_Coral_Tower.3 | Battle Outro ×4 | coral_tower_outro_roar 1997e79fef | Battle-end roar, opens gates, swaps camlocks | maybe | Battle end only reaches the game that ran the battle. | Battle-end event if shared. | | scenes/Memory_Coral_Tower.fsm.txt:1288 |
| Memory_Coral_Tower.4 | Completion Camlock ×3 | FSM ffbc71403a | Enables camera lock after battle | no | Per-player camera only. | | | scenes/Memory_Coral_Tower.fsm.txt:1314 |
| Memory_Coral_Tower.5 | Intro Roar ×4 | coral_tower_intro_roar 2ae7aa07a1 | Roar stuns hero, then starts the waves | maybe | Stuns only the local hero and starts the battle there. | Battle-start event if shared. | Stun the partner too when a battle starts, or not | scenes/Memory_Coral_Tower.fsm.txt:9873 |
| Memory_Coral_Tower.6 | Coral Warrior ×17 | Control d37ea9fd24, 650aa36b2d | Melee: slashes, blocks, evades, jumps at hero | maybe | Chases the local hero; see memory note above. | Enemy deaths if shared. | Private memory or shared fight | scenes/Memory_Coral_Tower.fsm.txt:1529, scenes/Memory_Coral_Tower.fsm.txt:12793 |
| Memory_Coral_Tower.7 | Corpse variants ×50 | Crust Away 4998069956, Shove From Wall bbe99db7dc, Custom Corpse e72ca54441 | Corpse crust falls, shoves, lands | no | Corpse cosmetics. | | | scenes/Memory_Coral_Tower.fsm.txt:2403, scenes/Memory_Coral_Tower.fsm.txt:14266, scenes/Memory_Coral_Tower.fsm.txt:47370 |
| Memory_Coral_Tower.8 | Coral Flyer Throw ×8 | Control 6dd4ec85fd | Flies, throws spears, slashes | maybe | Targets the local hero; see memory note above. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:2440 |
| Memory_Coral_Tower.9 | Coral Swimmer Fat ×17 | Notify Protector 920a0f4d1d, Control 6af15262d4 | Swimmer charges; alerts protector when hurt | maybe | Aggro and charge aim at the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:5958, scenes/Memory_Coral_Tower.fsm.txt:5965 |
| Memory_Coral_Tower.10 | Coral Flyer ×7 | Behaviour 3a25e294f4 | Rests on wall, wakes, flies and slashes | maybe | Wakes on and chases the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:6488 |
| Memory_Coral_Tower.11 | Coral Hunter ×8 | Control a26b6e969d | Spear thrower, wall-clings, jumps over hero | maybe | Aim and wall choice follow the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:10139 |
| Memory_Coral_Tower.12 | Coral Brawler ×5 | Control 727894ac30 | Stomp, charge, jump brute, summons spires | maybe | Charge and jump aim at the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:11009 |
| Memory_Coral_Tower.13 | Stomp Spire L/R ×10 | Control ea90345d8c, 108e82f66c; Break Spikes 615fdd132e, b9f2254b1f | Brawler stomp spires rise; break relays | maybe | Spawned by the brawler stomp and hurt whoever stands in them. | Follows the brawler if shared. | | scenes/Memory_Coral_Tower.fsm.txt:11536, scenes/Memory_Coral_Tower.fsm.txt:11631, scenes/Memory_Coral_Tower.fsm.txt:10997, scenes/Memory_Coral_Tower.fsm.txt:11587 |
| Memory_Coral_Tower.14 | Coral Big Jellyfish ×5 | Control f65f410da9 | Chases and charges, spawns small swimmers | maybe | Chases the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:14134 |
| Memory_Coral_Tower.15 | Coral Swimmer Small ×40 | Control 7efa66aca0 | Small jelly bursts out and chases | maybe | Chases the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:14271 |
| Memory_Coral_Tower.16 | Spike Set ×6 | Control 8cfa0749eb; FSM f76bba43ea | Floor spike wave; sets hero hazard respawn point | maybe | Writes the hero's hazard-respawn point, so it must stay inside the memory and not leak to the partner. | Nothing beyond keeping memory-only respawn data local. | | scenes/Memory_Coral_Tower.fsm.txt:14411, scenes/Memory_Coral_Tower.fsm.txt:14435 |
| Memory_Coral_Tower.17 | coral_spike_floor_clump ×55 | Break e3db089ef5; FSM 2eb51810b7 | Spike clumps break and regrow on hit | maybe | Break and reform timing follow local hits; hazard for whoever stands there. | CORAL BREAK event if shared. | | scenes/Memory_Coral_Tower.fsm.txt:14444, scenes/Memory_Coral_Tower.fsm.txt:33867 |
| Memory_Coral_Tower.18 | Coral Spike Swimmer ×4 | Control 5ceed4f115; Tink Recoil Control 65b3b55384 | Spiky fish aggros and lunges; tink recoil | maybe | Aggro targets the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:17453, scenes/Memory_Coral_Tower.fsm.txt:17743 |
| Memory_Coral_Tower.19 | Platform Set HazA ×3 and arena set-ups | Haz Plat Control 70472bf48f; Start Hazard Plat A bd08f8b0d2; FSM 8659d255f3, 3c4fa2e98a; Start Activated 4f7f7e60d8; Deactivate Trigger 0b96d5406c; Arena Setup 06db3358c1, 92df35f728 | Hazard platforms grow; arena platforms set up | maybe | Platform states follow wave progress and spike hits seen locally. | Wave-start events if shared. | | scenes/Memory_Coral_Tower.fsm.txt:20688, scenes/Memory_Coral_Tower.fsm.txt:33764, scenes/Memory_Coral_Tower.fsm.txt:34699, scenes/Memory_Coral_Tower.fsm.txt:42364, scenes/Memory_Coral_Tower.fsm.txt:35068, scenes/Memory_Coral_Tower.fsm.txt:36023, scenes/Memory_Coral_Tower.fsm.txt:36030, scenes/Memory_Coral_Tower.fsm.txt:40879 |
| Memory_Coral_Tower.20 | Coral Bubble Brute ×3 | Control 63e3f93210; Break Spikes 45c9f78928 | Bubble cloud, stomp and charge; breaks spikes | maybe | Attack choice aims at the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:24109, scenes/Memory_Coral_Tower.fsm.txt:24338 |
| Memory_Coral_Tower.21 | Coral Poke Swimmer ×4 | Control 72f3c55be7 | Fish aggros and charges at hero | maybe | Charges the local hero. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:32277 |
| Memory_Coral_Tower.22 | End Scene (Chamber 4) | Control 4c4dab28fa | Final chamber end: roof opens, kill spikes | maybe | Rumble and kill spikes run only in the game where chamber 4 ended. | Chamber-4 battle-end event if shared. | | scenes/Memory_Coral_Tower.fsm.txt:34871 |
| Memory_Coral_Tower.23 | Boss Scene | Control 98470e7a8e | Trigger starts king intro; sets encounteredCoralKing; exits memory | sure | Entry trigger fires only for the local hero; the exit is a scene change back to Coral_Tower_01. | The boss-start and END KING SCENE events if shared; the encounteredCoralKing flag. | Pull partner in and out with the player, or leave free | scenes/Memory_Coral_Tower.fsm.txt:42402 |
| Memory_Coral_Tower.24 | Coral King | Control 16a72648be | Coral King boss; death grants Coral Heart | sure | Death grants a memento and a story flag, and the fight targets one hero. | defeatedCoralKing and the Coral Heart award through save-data sharing; boss HP if shared. | Each player earns own Coral Heart (recommended) or one shared | scenes/Memory_Coral_Tower.fsm.txt:42847 |
| Memory_Coral_Tower.25 | Coral King | Crust Up 33426cd9b7 | Coral crust grows over the king | no | Cosmetic. | | | scenes/Memory_Coral_Tower.fsm.txt:42830 |
| Memory_Coral_Tower.26 | Boss spear attacks | Air Spear 07e16b3009; Interrupt Control ecd0a65570; Cross Spears a269059ea8; Long Spear 9399da6b99; Uppercut Spear bcf06d2ac3 | Spawn king's spear attacks | maybe | Fired by the king's attack events and hurt whoever is hit. | Follows the king's attack events if shared. | | scenes/Memory_Coral_Tower.fsm.txt:42461, scenes/Memory_Coral_Tower.fsm.txt:42477, scenes/Memory_Coral_Tower.fsm.txt:43613, scenes/Memory_Coral_Tower.fsm.txt:44300, scenes/Memory_Coral_Tower.fsm.txt:47074 |
| Memory_Coral_Tower.27 | Boss spike hazards | Roar Spikes 95a5cff548 and children 6b3e5febbb, b86b426e1b; Shoot Spikes 1774e49cd0 and 5593caa0dd, 1418d1f743, f9d0ed75e0; Death Spikes b759d92c9c, ef49365656; Ascent Spike c8f2cd05a2 (deprecated) | Spike patterns from roar, shots, death and ascent | maybe | Patterns are chosen randomly per game and hurt whoever stands there. | The pattern choice event if shared. | | scenes/Memory_Coral_Tower.fsm.txt:44355, scenes/Memory_Coral_Tower.fsm.txt:44390, scenes/Memory_Coral_Tower.fsm.txt:44399, scenes/Memory_Coral_Tower.fsm.txt:45242, scenes/Memory_Coral_Tower.fsm.txt:45274, scenes/Memory_Coral_Tower.fsm.txt:45318, scenes/Memory_Coral_Tower.fsm.txt:45324, scenes/Memory_Coral_Tower.fsm.txt:43709, scenes/Memory_Coral_Tower.fsm.txt:43715, scenes/Memory_Coral_Tower.fsm.txt:42492 |
| Memory_Coral_Tower.28 | Pt Death | Follow Cam 2b658f63e1 | Death particles follow camera | no | Visual. | | | scenes/Memory_Coral_Tower.fsm.txt:44348 |
| Memory_Coral_Tower.29 | Door Get Up | FSM 6e0bbd65dd | Fades scenery in on arrival | no | Visual. | | | scenes/Memory_Coral_Tower.fsm.txt:47149 |
| Memory_Coral_Tower.30 | Coral Goomba ×4 | Behaviour a0e9e428e8 | Wall crawler, sings along with needolin | maybe | Contact enemy; only the local hero's needolin makes it sing. | Enemy deaths if shared. | | scenes/Memory_Coral_Tower.fsm.txt:47324 |
| Memory_Coral_Tower.31 | Coral Bubble ×24 | Control 972f91cfd4 | Drifting bubble pops on hit, hurts hero | maybe | Pops only for the hitter, hazard for whoever touches it. | Bubble pop event if shared. | | scenes/Memory_Coral_Tower.fsm.txt:47875 |
| Memory_Coral_Tower.32 | Jellyfish tint | NonTinter (C#) | Excludes sprite from tinting | no | Visual. | | | scenes/Memory_Coral_Tower.objects.txt:6421 |

## Memory_First_Sinner

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Memory_First_Sinner.1 | _SceneManager | FSM ce0a710861 | Disables dream-gate use | no | Hero-local flag. | | | scenes/Memory_First_Sinner.fsm.txt:4 |
| Memory_First_Sinner.2 | Memory Control | Memory Control c522bb0273 | Orb-by-orb cutscene; grants Silk Bomb, sets defeatedFirstWeaver | sure | Grants a skill, auto-equips it and saves; one player gets it, the other does not. | The skill grant (hasSilkBomb) and defeatedFirstWeaver. | Skill goes to both players or only the one who played the memory | scenes/Memory_First_Sinner.fsm.txt:51 |
| Memory_First_Sinner.3 | door_wakeOnGround | Remove darkness b4fb2d822e | Sets darkness level 1 | no | Visual. | | | scenes/Memory_First_Sinner.fsm.txt:128 |
| Memory_First_Sinner.4 | door_wakeOnGround | Wake Up 94df19efd1 | Arrival: control locked, set death respawn, autosave | sure | Locks hero, replaces the death respawn marker and saves; must stay private to the memory hero. | Nothing, but keep the memory respawn marker out of shared data. | | scenes/Memory_First_Sinner.fsm.txt:135 |
| Memory_First_Sinner.5 | memory_orb_inspect | Control d456e8b3b4 | Orb interact: kneel, flash, advance to next scene | maybe | Locks only the interacting hero; see memory note above. | Orb collected if shared. | | scenes/Memory_First_Sinner.fsm.txt:220 |
| Memory_First_Sinner.6 | Memory Spider Hang ×6 / Scuttle ×8 | Control c66c4f45ca, 02f98a925d | Decorative spiders retreat from hero | no | Visual, local. | | | scenes/Memory_First_Sinner.fsm.txt:320, scenes/Memory_First_Sinner.fsm.txt:500 |
| Memory_First_Sinner.7 | memory_ground_plat ×43 | Control 8af84fdf51 | Platform breaks after landing, then reforms | maybe | Breaks after the local hero lands; see memory note above. | Platform break event if shared. | | scenes/Memory_First_Sinner.fsm.txt:888 |
| Memory_First_Sinner.8 | Slope Shover | FSM 545cf25e1d | Nudges hero down a slope | no | Moves only the local hero. | | | scenes/Memory_First_Sinner.fsm.txt:2760 |

## Memory_Needolin

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Memory_Needolin.1 | Memory Control | Memory Control 5b18459577 | Memory cutscene; sets hasNeedolin, spinnerDefeated, autosave | sure | Grants the Needolin; one player gets it, the other does not. | hasNeedolin and spinnerDefeated through save-data sharing. | Needolin to both players or only the one who played the memory | scenes/Memory_Needolin.fsm.txt:11 |
| Memory_Needolin.2 | door_wakeOnGround | Remove darkness 6658433e5b | Sets darkness level 1 | no | Visual. | | | scenes/Memory_Needolin.fsm.txt:82 |
| Memory_Needolin.3 | Memory Weaver Hang ×13 / Scuttle ×17 | Control 74ada1e476, 7a3dfc241d | Decorative weavers retreat from hero | no | Visual, local. | | | scenes/Memory_Needolin.fsm.txt:273, scenes/Memory_Needolin.fsm.txt:676 |
| Memory_Needolin.4 | Reminder Needolin - Memory | Control 44538dae1c | Shows needolin reminder near closed gate | no | Local UI hint. | | | scenes/Memory_Needolin.fsm.txt:3711 |
| Memory_Needolin.5 | Song_Gate_set/Audio Loop Idle Tone | deactivate if entered from left1 28c711f9cf | Mutes tone if entered from left1 | no | Audio only. | | | scenes/Memory_Needolin.fsm.txt:3980 |

## Memory_Red

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Memory_Red.1 | Memory Control | Memory Control b12753dca4 | Forces temporary Hunter crest, disables inventory | sure | Changes the hero's crest and inventory for the memory only; if equipment is shared, it must not leak. | Nothing; keep memory-temporary equipment out of shared data. | | scenes/Memory_Red.fsm.txt:11 |
| Memory_Red.2 | Memory Control, Music Test, root_memory, Audio Memory Music ×3 | Music Start 9c87ae1a78; FSM 44b832a75f; Music Control 5e47bc693d; Fade Out 387be5c301 | Music cues and fades | no | Audio only. | | | scenes/Memory_Red.fsm.txt:20, scenes/Memory_Red.fsm.txt:626, scenes/Memory_Red.fsm.txt:631, scenes/Memory_Red.fsm.txt:33 |
| Memory_Red.3 | door_enterRedMemory ×3 | Enter Wipe 07c99ef94b | Red screen wipe, holds transition | no | Local transition visual. | | | scenes/Memory_Red.fsm.txt:40 |
| Memory_Red.4 | door_wakeInRedMemory ×4 | Wake Up 44d732a118 | Arrival timeline; end grants White Flower, exits | sure | Writes a story flag, an item and a journal record, then leaves the scene. | CompletedRedMemory and the White Flower and journal grants through save-data sharing. | White Flower per player or shared | scenes/Memory_Red.fsm.txt:143 |
| Memory_Red.5 | top1 | Enter Memory 51e733aae7 | Hero falls into memory; sets death respawn, saves | sure | Hides and moves the hero, replaces the respawn marker and saves; must stay private to the memory hero. | Nothing, but keep the memory respawn out of shared data. | Pull partner into the memory, or leave free | scenes/Memory_Red.fsm.txt:645 |
| Memory_Red.6 | Control Lever | Control Lever 149129641a | Nail-hit lever moves a memory lift | maybe | Lever and lift in a scripted scenery; see memory note above. | Lever hit if shared. | | scenes/Memory_Red.fsm.txt:737 |
| Memory_Red.7 | Hero Check | FSM a8f7e73d65 | Tells lever when hero is in range | no | Local trigger relay. | | | scenes/Memory_Red.fsm.txt:809 |
| Memory_Red.8 | White Palace Fly Red Memory ×10 | Control b6e26be4cf | Fly flees, wounds and reforms when hit | no | Decorative, HP 9999, not saved. | | | scenes/Memory_Red.fsm.txt:1138 |
| Memory_Red.9 | hive_plat_brk ×5 | Smash 6797a809cc; Check 314ac2ffa2 | Platform smashed when a Boss-tagged object touches it | no | Scripted scenery, hero-independent. | | | scenes/Memory_Red.fsm.txt:1824, scenes/Memory_Red.fsm.txt:1855 |
| Memory_Red.10 | Return HUD On Dream Wake | Return HUD 1808aa097d | Shows HUD again | no | Local UI. | | | scenes/Memory_Red.fsm.txt:2175 |

## Memory_Silk_Heart_BellBeast

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Memory_Silk_Heart_BellBeast.1 | Memory Control | Memory Control ec8af494ac | Memory cutscene ending in Silk Heart prompt | sure | Ends a Silk Heart reward scene for one hero; the stat change sits in the end template, not visible here. | The Silk Heart reward event. | Each player gets own Silk Heart or one shared | scenes/Memory_Silk_Heart_BellBeast.fsm.txt:51 |
| Memory_Silk_Heart_BellBeast.2 | Memory Msg Player | Control e23057f07a | Shows needolin text during memory | no | Local UI. | | | scenes/Memory_Silk_Heart_BellBeast.fsm.txt:170 |
| Memory_Silk_Heart_BellBeast.3 | memory_orb_large 01 | Control 026b264895 | Touch orb: hero locked, scream, scene advances | maybe | Locks only the touching hero; see memory note above. | Orb touched if shared. | | scenes/Memory_Silk_Heart_BellBeast.fsm.txt:177 |

## Memory_Silk_Heart_LaceTower

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Memory_Silk_Heart_LaceTower.1 | Memory Control | Memory Control 0f0b7f163f | Memory cutscene: white flash, memory message, end | sure | Ends a Silk Heart reward scene for one hero; the reward sits in the end template. | The Silk Heart reward event. | Same as BellBeast row 1 | scenes/Memory_Silk_Heart_LaceTower.fsm.txt:51 |

## Memory_Silk_Heart_WardBoss

no new elements

## Organ_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Organ_01.1 | _SceneManager | Music Control e678450d34 | Organ music unless Phantom defeated | no | Audio only. | | | scenes/Organ_01.fsm.txt:4 |
| Organ_01.2 | Boss Scene | Control bee2adb1b4 | Arena entry: gates, camlock, starts Phantom | sure | Entry trigger fires only for the local hero, and defeatedPhantom decides at load whether the boss exists. | The fight-start event so the partner is locked in, and defeatedPhantom before scene load. | Pull partner to the arena when the fight starts, hold partner out, or let partner join freely | scenes/Organ_01.fsm.txt:21 |
| Organ_01.3 | Fog Floor, Fog Follow Hornet, Fog Follow Phantom | Control fec88ffa0c, 44d6167e2c, 910c580cea | Fog particles follow walkers | no | Visual. | | | scenes/Organ_01.fsm.txt:209, scenes/Organ_01.fsm.txt:216, scenes/Organ_01.fsm.txt:231 |
| Organ_01.4 | Phantom | Death Control ee862e39bd | At zero HP, only a block ends fight | sure | Damage from either player can empty HP, but only the parry duel with the local hero finishes it. | The final-block event once, so all games end together. | Which player does the final parry | scenes/Organ_01.fsm.txt:454 |
| Organ_01.5 | Phantom | Control ac06217ce8 | Phantom boss: stabs, throws, parry duel | sure | Chases and aims at the local hero, picks attacks by RNG, and ends in a timed parry cutscene that sets hasParry and defeatedPhantom. | Phantom HP and attack choice if both fight; hasParry and defeatedPhantom through save-data sharing. | Duelist is the first player in, or both play a duel; Parry given to both or only the duelist | scenes/Organ_01.fsm.txt:468 |
| Organ_01.6 | Phantom | Set Cross Stitch Connected c623def921 | Flags that a multi-hit attack connected | maybe | Tracks hits on the local hero only. | Nothing. | | scenes/Organ_01.fsm.txt:1665 |
| Organ_01.7 | Hit ParryStab | Hit Hero e7525c22c5 | Reports a parry-stab hitting the hero | maybe | Reacts to the local hero collider only. | Nothing. | | scenes/Organ_01.fsm.txt:1690 |
| Organ_01.8 | Shake Test | Control f4092ef057 | Debug camera shakes | no | Camera effect. | | | scenes/Organ_01.fsm.txt:4602 |
| Organ_01.9 | Slash Anim (Hornet Parry Slash) | HeroShamanRuneEffect (C#) | Hero slash visual with rune tint | no | Visual and local equipment look. | | | scenes/Organ_01.objects.txt:1105 |

## Shellgrave

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellgrave.1 | Event Control | Control 77d6546f29 | Picks scene variant from PlayerData flags | sure | If the two saves differ (savedPlinney, sawPlinneyLeft, defeatedShellwoodRosaryPilgrim, blackThreadWorld), each player loads a different scene. | Those flags before the scene loads. | | scenes/Shellgrave.fsm.txt:80 |
| Shellgrave.2 | Enemy Deparenter | Deparent c2c273ffd4 | Unparents enemies entering trigger | no | Scene plumbing. | | | scenes/Shellgrave.fsm.txt:858 |
| Shellgrave.3 | Plinney Outside | Dialogue 8b90fa1aed | Scared NPC, stands at battle end, talks | sure | Waits for the rescue battle's end event, which only the game that ran the battle receives. | The battle-end event (savedPlinney) so he stands in both games. | Dialogue per player (recommended) or single shared | scenes/Shellgrave.fsm.txt:5323 |

## Sprintmaster_Cave

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Sprintmaster_Cave.1 | Sprintmaster Companion | Behaviour 908793efa7 | Bell fly: race countdown, carries hero to restart | sure | Fades out and repositions the hero it picks up, and rings the countdown for one racer. | Race reset and countdown events. | Whether the partner can use this while a race is running | scenes/Sprintmaster_Cave.fsm.txt:155 |
| Sprintmaster_Cave.2 | Sprintmaster Runner | Idle Anim 204b66ffd0 | Jog, stretch and victory animations | no | Visual. | | | scenes/Sprintmaster_Cave.fsm.txt:1130 |
| Sprintmaster_Cave.3 | Sprintmaster Runner | Behaviour 3c46f8f2df | Race host: offer, quest, start, result | sure | Moves the hero to the line, swaps silk, writes race and quest data, and runs against one hero. | Race accepted, started, and result. | One racer at a time: block the partner, or two separate races | scenes/Sprintmaster_Cave.fsm.txt:1176 |
| Sprintmaster_Cave.4 | Sprintmaster Runner | Hero Start Line 9e75928860 | Crouch start pose until dash is released | no | Local input animation. | | | scenes/Sprintmaster_Cave.fsm.txt:1706 |
| Sprintmaster_Cave.5 | Sprintmaster Runner | SplineRunner (C#) | AI runner along spline, speed tied to hero | sure | Follows and rubber-bands to one hero only. | Only the racer's progress. | | scenes/Sprintmaster_Cave.objects.txt:26 |
| Sprintmaster_Cave.6 | Finish Trigger / sprint_ring_marker ×23 | SprintRaceLapMarker (C#) | Lap markers; wrong order disqualifies | sure | Only accept the local hero collider, so a partner racing alongside would not count. | Racer's lap progress. | | scenes/Sprintmaster_Cave.objects.txt:65 |
| Sprintmaster_Cave.7 | Track 1-3 Race Control ×4 | SprintRaceController (C#) | Counts laps, announces winner, holds reward | sure | Single hero versus runner; reward is a saved item. | Race result. | Reward per player or shared | scenes/Sprintmaster_Cave.objects.txt:258 |

## Tube_Hub

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Tube_Hub.1 | pneumatic_tube_hub_animated | Animation 822231576a | Tube capsule opens, closes, arrives, leaves | maybe | Capsule animation plays only in the game of the player using it, so the partner in the hub sees it unmoved. | Hero entering or leaving the tube. | | scenes/Tube_Hub.fsm.txt:1015 |

## Summary

1. Directional choice, memory scenes (Memory_*): are they private or shared. Entry is a scene change for one hero only (thread-memory burst in a field scene, or an ability shrine). Private means the partner stays put, no enemies need to be shared, and every `maybe` row in a Memory_* scene becomes `no`. Shared means both players in the same memory, so Hunter Queen, Coral King and all Coral Tower enemies become real boss and arena fights.
2. Events that should pull or lock the partner, if that is the choice:
   1. Thread-memory burst (all Memory_* entries): reason, one hero leaves the world.
   2. Organ_01 Phantom arena entry: gates and camera lock only affect the entering hero.
   3. Memory_Ant_Queen and Memory_Coral_Tower boss defeat (memory exit): reason, a white fade scene change for one hero.
   4. Memory_Ant_Queen corpse NPC interact: scripted grab that hides and moves the hero.
   5. Sprintmaster race start: the race tracks a single hero.
3. Save data written inside memories and rewards that need a per-player or shared decision: hasNeedolin and spinnerDefeated (Memory_Needolin), hasSilkBomb and defeatedFirstWeaver (Memory_First_Sinner), CompletedRedMemory, White Flower and White Palace Fly journal (Memory_Red), defeatedAntQueen, metAntQueenNPC, defeatedAntQueenAfterRedMemory (Memory_Ant_Queen), encounteredCoralKing, defeatedCoralKing and the Coral Heart memento (Memory_Coral_Tower), hasParry and defeatedPhantom (Organ_01).
4. Temporary memory state must not leak: entering a memory stores the hero's health, silk and tools and applies overrides, Memory_Red also forces a temporary Hunter crest and disables the inventory, and several memories replace the death respawn marker and save the game. If equipment or respawn data is mirrored to the partner, the memory-only values should be excluded.
5. Phantom (Organ_01) is a scripted duel: HP reaching zero does not kill it, only a parry from the hero ends it, then a timed parry prompt (about 1.5 s) decides a hit or the Parry skill. A choice is needed for who does the final parry and whether both get the skill.
6. Chapel_Wanderer trap: the tripwire is camera-gated and fires only in the game where it is cut; share the cut event.
7. Shellgrave: the scene variant is chosen from four save flags at load, and Plinney only stands up on the local battle-end event, so the flags and the battle end must arrive first.
8. Sprintmaster_Cave: single-racer design (one runner, one set of lap markers that only accept the local hero). Decide one racer at a time or independent races.
9. Abandoned_town memento: one hero's needolin hold triggers a saved item drop; choose shared versus per-player item.
10. Gaps: the end-of-scene templates in Silk Heart memories (BellBeast, LaceTower) are RunFSM templates not visible in the dump, so the exact Silk Heart save-data change is unconfirmed; Memory_Silk_Heart_WardBoss has no new elements.
