# Audit 2.05: Bellshrines, Bellways, Wisp Thicket, Wormways

Reading guide. Verdict: sure / maybe / no = does coop need to handle it. Triggers that read `Hero`, alert ranges, `Trigger2dEvent` and `CheckHeroPerformanceRegion` (needolin) only ever see the LOCAL hero, so everything below that "wakes", "aggroes" or "reacts to needolin" does so per game.
Notes written: StateChangeSequence, AdditiveLoreSceneController, LifebloodPustule, LifebloodState, HeroCorpseMarkerProxy. Notes used: RestBench, BellBench, Lever, PersistentBoolItem, Gate, AlertRange, CheckHeroPerformanceRegion, EnemySingControl, HealthManager, RosaryCache, FrostRegion.

## Bellshrine

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellshrine.1 | Hanging Bell | FSM Break Threads 40dc8cbf03 | Three hits snap threads, release bell | sure | Each hit gives the local hero 1 silk and advances a hit counter; the 3rd snaps threads and calls StateChangeSequence.SetState(1) (saved), so the partner's bell stays threaded and their shrine sequence never starts. | The thread-snap (final hit, or each of the 3 hits); release, physics bell and later states replay natively. | Count hits from both players together (recommended) or per game. | scenes/Bellshrine.fsm.txt:909 |
| Bellshrine.2 | Bellshrine Sequence (+ Large Bell ×2) | C# StateChangeSequence | Saved step machine: bell, lever, bench | sure | Holds the shrine progress in a PersistentIntItem and on the last step sets the shrine's PlayerData bool and an achievement; only the game that advanced it moves on. | The step index (SetState n); each game runs its own OnEnter events. | | scenes/Bellshrine.objects.txt:143 |
| Bellshrine.3 | Bell Shrine Lever | FSM Activate Delayed 994e9d0e73 | Lever appears after 3s, takes the hit | sure | The lever hit (C# Lever) starts the mechanism and bench rise; only the hitter's game proceeds. | The lever hit; mechanism, bench appear and gate replay natively. | | scenes/Bellshrine.fsm.txt:804 |
| Bellshrine.4 | Bench Appear Sequence | FSM Bench Appear 5c98c53cc4 | Bench rises, quest update, shrine title | sure | Quest update, achievements and BELLBENCH APPEARED (opens the shrine gate) run only in the game whose lever chain fired. | Nothing beyond the lever hit; the sequence then plays in both games. | Show the quest popup/title in both games (recommended) or only the activator's. | scenes/Bellshrine.fsm.txt:829 |
| Bellshrine.5 | Physics Bell, Physics Bell Return, Large Bell | FSM Bell Needolin Response fb2129a498, Sound Effects 2b6a94b9af, Tink Hit Force f93b643e9e, Tink Jitter 1aab1f8053; C# ForceRamp2D, DetectSwingPeak | Bell swings, rings, glows on needolin | no | Physics swing (random tink direction) and ring sounds are cosmetic. | | | scenes/Bellshrine.fsm.txt:1002 |
| Bellshrine.6 | Lit Candles, Candle Group ×N, Mechanism Effects | FSM Light Candles 7652ee7f30, Effects b30c7273b0; C# AnimatorGroup | Candles light, ambient light, dust and shakes | no | Visual follow-up of the sequence. | | | scenes/Bellshrine.fsm.txt:1127 |
| Bellshrine.7 | Bellshrine Lore Scene Controller | C# AdditiveLoreSceneController | Needolin at shrine shows a lore vision | no | Local hero needolin only; loads an additive scene for that player's presentation. | | | scenes/Bellshrine.objects.txt:375 |

## Bellshrine_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellshrine_02.1 | Music Control Act 3 | FSM FSM d763cf957d | Starts rest-area music | no | Audio. | | | scenes/Bellshrine_02.fsm.txt:10 |

## Bellshrine_03

no new elements

## Bellshrine_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellshrine_05.1 | Whole Scene | FSM Demo End f3a82fc803 | Demo build: bench sit ends demo | no | Only in demo mode (CheckIsDemoMode); inert in the release game. | | | scenes/Bellshrine_05.fsm.txt:24 |

## Bellshrine_Coral

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellshrine_Coral.1 | Coral Crust Wall Sphere ×3 (100 uses game-wide) | FSM Control bff02bd179; C# IsCoralCrustWall | Multi-hit coral wall, path blocker, saves PD | sure | Hits count only when the local hero is in Damageable Range; on break removes terrain collider and camera lock and sets a PD bool. The other player stays blocked until reload. | The final break (wall id); fade, debris and shake replay natively. | | scenes/Bellshrine_Coral.fsm.txt:804 |
| Bellshrine_Coral.2 | Terrain Collider ×3 | C# IgnoreHeroCollision | Lets hero pass through a collider | no | Ignores collisions of the local hero only; static level setup. | | | scenes/Bellshrine_Coral.objects.txt:113 |

## Bellshrine_Enclave

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellshrine_Enclave.1 | Custom Music Controller | FSM Control c8f1ee4da7 | Soft music when enclave level above 0 | no | Audio. | | | scenes/Bellshrine_Enclave.fsm.txt:4 |
| Bellshrine_Enclave.2 | Set Respawn Point | FSM Set Respawn Point 34529a56dc | Lever hit sets death respawn here | no | Respawn point is per player; the lever itself is Bellshrine.3. | | | scenes/Bellshrine_Enclave.fsm.txt:836 |

## Bellshrine_Lore_Additive

no new elements

## Bellway_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_01.1 | Bellway Pilgrim | FSM Behaviour e25f9ffbdd | NPC talk; scared by Bell Beast arrival | maybe | Dialogue writes PD BonebottomBellwayPilgrimState and a scene bitmask; when the bell beast arrives (BEAST APPEAR) sets PD BonebottomBellwayPilgrimScared, later Left. Each game only changes on its own beast call. | The bell beast arrival (bellway call) is the real event; dialogue flags stay per player. | | scenes/Bellway_01.fsm.txt:345 |
| Bellway_01.2 | Music Control, Skull King Ruckus | FSM Control ee6a55cc38, Control 086f28a7a1 | Music and distant Skull King thumps | no | Audio and camera shake keyed to PD skullKingInvaded. | | | scenes/Bellway_01.fsm.txt:462 |

## Bellway_01_boss

no new elements

## Bellway_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_02.1 | Bellway Toll Machine (+ same FSM in all Bellway stations, 11 uses) | FSM Unlock Behaviour f0365cb8dd; C# DemoModeBehaviour | Pay to unlock bellway station, floor opens | sure | Payment prompt (RunFSM template) takes the payer's currency and sets the station's PD bool, opens the Floor Gate, raises the map, sends BELLWAY UNLOCKED (allows calling the beast). Partner's station stays locked until reload. | The unlock (station PD bool); the partner's machine plays its External/Map Up branch. | Charge only the payer (recommended) or split the cost. | scenes/Bellway_02.fsm.txt:134 |
| Bellway_02.2 | Music Control Bellway | FSM Control 0d17e79693 | Rest music near station | no | Audio, local trigger. | | | scenes/Bellway_02.fsm.txt:206 |

## Bellway_02_boss

no new elements

## Bellway_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_03.1 | Music Control Bellbench | FSM Control ab64635ede | Rest music near bell bench | no | Audio. | | | scenes/Bellway_03.fsm.txt:292 |
| Bellway_03.2 | Bone Circler Vicious ×3 (31 uses) | FSM Control 636bbf3d72 | Ambush flyer, chases, fires at hero | sure | Wakes and ambushes from alert/ambush ranges, ChaseObject and FireAtTarget at the local hero, sings to needolin; two players see it chase different targets. | Enemy state and position from one game, plus hits and death. | | scenes/Bellway_03.fsm.txt:1271 |
| Bellway_03.3 | Wind Rock ×2 (+ explode rock) | FSM Control b945466455, Detect Hit f40d81e002 | Hit rock explodes, opens an updraft | sure | A valid hit sets a PD bool, removes rock and walls, enables the updraft and drops a camera lock; the other player still faces the rock and no updraft. | The strike (rock id); explosion and updraft swap replay natively. | | scenes/Bellway_03.fsm.txt:2553 |
| Bellway_03.4 | Updraft After ×2 (85 uses) | FSM Control 7561c0174e; C# PositionRelativeTo, ScaleToMatchCollider, EnemyUpdraftRegion | Updraft lifts hero inside region | no | Calls EnterUpdraft on the local hero only; each player gets lifted in their own game once the rock state matches. | | | scenes/Bellway_03.fsm.txt:2792 |

## Bellway_03_boss

no new elements

## Bellway_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_04.1 | Mite Trap (45 uses) | FSM Control 00f120d575 | Spring trap grabs hero or mite | maybe | Springs on hero, attacks, enemies; grabbing the hero runs a damage template; stays sprung (saved Activated). A trap sprung by the partner stays armed locally and can still snap the local player. | The spring (trap id); snap animation then replays natively. | | scenes/Bellway_04.fsm.txt:234 |
| Bellway_04.2 | Tip Over Object ×2 (135 uses) | C# TipOverObject | Random tip-over on camera shake | no | Decorative, random per game. | | | scenes/Bellway_04.objects.txt:850 |

## Bellway_04_boss

no new elements

## Bellway_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_08.1 | Sherma BellBench NPC | FSM Conversation Control eecaab9fd2 | Sherma wakes and talks at bell bench | maybe | Wakes on needolin or interact; writes PD metSherma, shermaWokeInSteps, shermaConvoCoralBench/Judges; reads the local toll machine. Partner sees her asleep or idle differently. | Nothing required; dialogue flags are per player. | Share Sherma's met/woke flags so both see the same NPC state, or leave per player (recommended). | scenes/Bellway_08.fsm.txt:1122 |
| Bellway_08.2 | Zaza Beckon NPC | FSM Control ca7ae4f381, Check Exit Leading b5dd4dbad8 | Beckons hero across rooms to Garmond's corpse | sure | Leads, waits and jumps based on the local hero position and entry gate across Bellway_08 and Coral_33/35/43; in coop the NPC paces in front of each player separately and may sit at different points. | The NPC's progress point (scene + landing) from the leading player. | Zaza follows the first player (recommended) or each player separately. | scenes/Bellway_08.fsm.txt:2193 |
| Bellway_08.3 | Memento Pickup | FSM Complete Quest 34a3ed1451 | Pickup ends quest Garmond Black Threaded | sure | Pickup gives the memento and EndQuest to the taker only; the other game keeps the item and the open quest. | The pickup taken (item id) and quest end. | Both players complete the quest (recommended) or taker only. | scenes/Bellway_08.fsm.txt:2512 |
| Bellway_08.4 | Camera Shake Region Judge Steps, Dust Storm Manager | C# CameraShakeRegion, VentRegion, WorldRumbleManager, TransitionAudioFader, SetWorldTransform | Local shakes, random dust-storm rumbles, audio | no | Camera and audio per player; rumble timing random per game but only cosmetic events. | | | scenes/Bellway_08.objects.txt:799 |

## Bellway_Aqueduct

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_Aqueduct.1 | Caravan Troupe Hunter Wild | FSM Dialogue 8492c65cad | Flea NPC; meeting counts a rescued flea | sure | Meet dialogue sets PD MetTroupeHunterWild and adds 1 to FleasCollected; only the talker gets the flea and the NPC then leaves in that game only. | The meet (flea rescued); both games set the flag and count. | Count the flea for both players (recommended) or talker only. | scenes/Bellway_Aqueduct.fsm.txt:715 |
| Bellway_Aqueduct.2 | waterways_particles (1) | FSM FSM 941f352b5b | Particles follow the camera | no | Visual. | | | scenes/Bellway_Aqueduct.fsm.txt:2156 |

## Bellway_Centipede_additive

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_Centipede_additive.1 | Bell Centipede Bellway Scene | FSM Control 3e08862b94 | Giant centipede ambushes the bellway call | sure | On CALL CENTIPEDE with PD BellCentipedeWaiting: input block, roar stun, sets PD bellCentipedeAppeared and starts MainQuest Bellbeast Rescue, opens the centipede door. Only the caller's game plays it. | The bellway call (CALL CENTIPEDE) in that station; the cutscene replays natively. | Pull the partner in for the ambush, freeze them, or leave free. | scenes/Bellway_Centipede_additive.fsm.txt:1 |
| Bellway_Centipede_additive.2 | Bell Centipede Door | FSM Control dc505caaa1 | Hornet jumps in, goes to arena | sure | Interact makes the local hero jump, sets disablePause and invulnerable, fades and transitions to Bellway_Centipede_Arena top1 (the boss). | The entry (door used); partner follows by choice. | Pull the partner into the arena (recommended for a boss) or let them choose. | scenes/Bellway_Centipede_additive.fsm.txt:107 |
| Bellway_Centipede_additive.3 | door_centipedeExit | FSM Teleport Arrive e3f56f1346 | Hero flung out on arrival here | no | Arrival animation for the local hero after the fight's travel. | | | scenes/Bellway_Centipede_additive.fsm.txt:169 |

## Bellway_Centipede_Arena

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_Centipede_Arena.1 | Centipede Control | FSM Control 40926916c7 | Boss director: intro, phases by HP, death | sure | Starts on local Start Range enter (roar stun, camera locks, title, music), phases on summed head+butt HP, death chain spawns Bell Beast NPC and journal kill. Two games run separate fights. | Fight start, total HP (hits) and phase changes from one game; death then replays natively. | One shared boss with combined HP (recommended) or per-player boss. | scenes/Bellway_Centipede_Arena.fsm.txt:3495 |
| Bellway_Centipede_Arena.2 | Giant Centipede Head, Giant Centipede Butt | FSM Control 2cd225fe2b, Control 953b0b74f4, Death Control 8dcc6558f8 | Boss ends: tele floor/roof, slash, spit bombs | sure | Attack choice and teleport X are RNG (SendRandomEventV4, RandomFloat), faces the local hero, needolin pauses them; both must look the same to both players. | Each chosen attack and teleport point from one game. | | scenes/Bellway_Centipede_Arena.fsm.txt:5773 |
| Bellway_Centipede_Arena.3 | Coil Charge Ground, Coil Charge Roof (+ Coils ×10, Drop Bells ×2) | FSM Control eea3c3fdf4, Control c55fdb081d, Control 4db1326f9e, Drop 8d3b5eecef, Drop d52cf2fadb | Coil sweep attack, falling bells | sure | Random flip and random X offset per attack; position must match or one player dodges a coil the other sees elsewhere. | The attack start with its flip and offset. | | scenes/Bellway_Centipede_Arena.fsm.txt:3809 |
| Bellway_Centipede_Arena.4 | BodyWall L, BodyWall R, Body Seg ×19 | FSM Shift 642f69c661, Control bbaef7987a, centipede_boss_seg b88afa677c | Arena body walls: appear, widen, explode | maybe | Driven by Centipede Control events (APPEAR, WIDE, WALL EXPLODE); Hero Blocker walls follow automatically once the director is shared. | Nothing beyond Bellway_Centipede_Arena.1. | | scenes/Bellway_Centipede_Arena.fsm.txt:386 |
| Bellway_Centipede_Arena.5 | Bell Beast DefeatedCentipede NPC | FSM Control c8d847e9df | Post-boss: Bell Beast sings, travel unlocked | sure | Interact runs a Hornet/beast exchange, needolin prompt, children sing, fade, gives Bellbeast Melody, sets PD UnlockedFastTravelTeleport and travelling, autosaves, then LoadSceneSingle Cinematic_Stag_travel. Hard scene change of the interacting player only. | The interact (start of the sing scene); both games then run it and travel. | Pull the partner into the cutscene and travel together (recommended), or travel alone and partner walks. | scenes/Bellway_Centipede_Arena.fsm.txt:181 |
| Bellway_Centipede_Arena.6 | BellBeast Child MeetSing A-D | FSM Control 353ae896de | Children pop out, sing | no | Part of the .05 cutscene, random launch speed is cosmetic. | | | scenes/Bellway_Centipede_Arena.fsm.txt:69 |
| Bellway_Centipede_Arena.7 | top1 | FSM Set Target e5229d9a79 | Exit leads to beast's current station | maybe | Target scene from PD FastTravelNPCLocation; players with different beast locations exit to different scenes. | Nothing if beast location is shared via PlayerData. | | scenes/Bellway_Centipede_Arena.fsm.txt:6202 |
| Bellway_Centipede_Arena.8 | Bell Entry Effect, Approach Voice, Strike Nail | FSM Control 39306a209b, Control a7ae8e5c62, Strike d5a370dd1e | Entry effect, beast calls, hit sprite | no | Local triggers, audio and visuals. | | | scenes/Bellway_Centipede_Arena.fsm.txt:26 |
| Bellway_Centipede_Arena.9 | HeroCorpseMarkerProxy | C# HeroCorpseMarkerProxy | Death cocoon goes to beast's station | no | Per-player death bookkeeping. | | | scenes/Bellway_Centipede_Arena.objects.txt:993 |

## Bellway_City

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_City.1 | City Travel Tube (+ pneumatic_tube_set) | FSM Tube Travel 6ca2de7d45, Animation df0c383c81 | Tube fast travel with map picker | sure | Locks the local hero, hides them in a fake Hornet, opens the tube map, saves level state, transitions to the picked station; also breaks all delivery-quest items on arrival. Partner is left behind. | The departure (target station). | Pull the partner along on departure (recommended, same as other fast travel) or travel alone. | scenes/Bellway_City.fsm.txt:793 |
| Bellway_City.2 | tube_toll_machine | FSM Unlock Behaviour 61ac6620c2 | Pay to unlock tube station | sure | Sets the station's PD bool after payment; the other game's tube stays locked until reload. | The unlock (PD bool); retract animation replays natively. | Charge only the payer (recommended) or split. | scenes/Bellway_City.fsm.txt:1078 |
| Bellway_City.3 | Map Machine | FSM Unlock Behaviour 087a70edc3 | Buy area map | maybe | Gives a map item to the buyer (SavedItemGet), currency from the buyer. | Nothing required. | Give the map to both players or buyer only (recommended). | scenes/Bellway_City.fsm.txt:2046 |
| Bellway_City.4 | CameraLockArea Arrive | FSM Disable On Land dfa1584fb8 | Removes camera lock after landing | no | Camera per player. | | | scenes/Bellway_City.fsm.txt:784 |

## Bellway_Peak

no new elements

## Bellway_Peak_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bellway_Peak_02.1 | One Way Wall Crystal (2) | FSM Control 4a71b48298 | One-side breakable crystal wall, sets PD | sure | Hits count only from the Can Break Range side; on break sets a PD bool, removes collider and camera lock; partner stays blocked. | The break (wall id). | | scenes/Bellway_Peak_02.fsm.txt:67 |
| Bellway_Peak_02.2 | Snowflake Chunk ×103 (+ Collider Outer) | FSM Control 9d7450eb4c, Superjump Passthrough 1fdf833956, Control 89c122c236; C# EnableFsmOnEnable, GrabPassBatcher | Ice chunk breaks on hit, reforms in ~2s | maybe | Temporary blocker/platform; a chunk the partner breaks stays solid locally, so the partner's Hornet appears to pass through ice for 2s. | The chunk break (id) if visual mismatch matters. | Share breaks (cheap) or ignore (recommended). | scenes/Bellway_Peak_02.fsm.txt:151 |
| Bellway_Peak_02.3 | rosary_dish_fling | FSM Fling Self c3b882db10 | Cache dish flies away from hero | no | Cosmetic follow-up of the rosary cache (see note RosaryCache). | | | scenes/Bellway_Peak_02.fsm.txt:142 |
| Bellway_Peak_02.4 | weaver lamp_roof heat large/Lamp | C# WeaverHeatLamp | Warmth zone against frost | no | Warms the local hero inside its trigger; each player handled in their own game. | | | scenes/Bellway_Peak_02.objects.txt:1653 |

## Bellway_Shadow

no new elements

## Wisp_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Wisp_02.1 | Farmer Wisp ×N (+ Immolater variants, 10 uses) | FSM Control 8dfb9f87fa, Fall Tele 0224294924 | Caster: teleports near hero, summons wisps, charges | sure | Teleport point picked by RandomFloat relative to the local hero X, attack choice by SendRandomEventV4, spawns Wisp Fireballs with random angle. | Enemy state, teleport point and summons from one game, plus hits and death. | | scenes/Wisp_02.fsm.txt:172 |
| Wisp_02.2 | Wisp Fireball Master | FSM Control ae02faf8e5 | Rate-limits wisp attacks | maybe | Shared attack token between wisps; correct automatically if the wisps are run by one game. | Nothing on its own. | | scenes/Wisp_02.fsm.txt:3913 |
| Wisp_02.3 | Wisp Flame Lantern ×2 | FSM Control 732c2cfdbb | Lantern spawns wisps while hero near | sure | Spawns hero-seeking Wisp Fireballs at random angles while the local hero is in range; breakable (journal kill), friendly if the Wisp Lantern tool is equipped (per player). | Spawns and break from one game. | | scenes/Wisp_02.fsm.txt:3933 |
| Wisp_02.4 | Corpse Farmer Wisp | FSM Burnup 8fcc2a2eb3 | Corpse explodes after stopping | maybe | Explosion may hurt; follows the corpse each game spawns. | Nothing beyond enemy death. | | scenes/Wisp_02.fsm.txt:1383 |
| Wisp_02.5 | Farmer Wisp BG | FSM Control b14925ff3e | Background wisp drifts by | no | Visual. | | | scenes/Wisp_02.fsm.txt:4 |

## Wisp_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Wisp_03.1 | Doctor Door Inspect | FSM FSM aa40f51f2a | Doctor's door talk; opens for cure quest | maybe | Dialogue writes per-player PD (MetBelltownDoctorDoor and others); opens the door and activates the transition only when the local player can complete Doctor Curse Cure. | Nothing required; partner uses the door from their own game. | | scenes/Wisp_03.fsm.txt:4 |
| Wisp_03.2 | Farmer Catcher (13 uses) | FSM Control 5a159cc32d; C# StaffFallOver | Wakes, attacks, random evade, silk possession | sure | Ranges and GetXDistance on the local hero, SendRandomEventV4 evade choice. Staff physics on the corpse is cosmetic. | Enemy state from one game, hits and death. | | scenes/Wisp_03.fsm.txt:124 |
| Wisp_03.3 | Bell Goomba (9 uses) | FSM Control 6a048195f4 | Emerges from node, attacks hero | sure | Emerge and attack ranges on the local hero, random spawn point choice. | Enemy state from one game, hits and death. | | scenes/Wisp_03.fsm.txt:424 |

## Wisp_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Wisp_04.1 | Fall Bench Group | FSM Bench Fall Control 00fd3e54e3 | Breaking a sack drops the bench | sure | Bench only becomes usable after Breakable Hang Sack 3 breaks (persistent); if the partner breaks it, the local bench stays hidden. | The sack break (breakable id); the fall then replays natively. | | scenes/Wisp_04.fsm.txt:146 |

## Wisp_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Wisp_05.1 | Music Snapshot Marker | C# MusicSnapshotMarker | Music mix marker | no | Audio. | | | scenes/Wisp_05.objects.txt:1054 |

## Wisp_06

no new elements

## Wisp_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Wisp_07.1 | Hero Shover ×3 (48 uses) | FSM Control 57c29e4637 | Nudges hero up out of crumble plat | no | Moves the local hero only; collision helper. | | | scenes/Wisp_07.fsm.txt:875 |

## Wisp_08

no new elements

## Wisp_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Wisp_09.1 | Blade Spider (1) (24 uses) | FSM Behaviour 0bc717df45 | Crawler, alerts, ambush, attacks hero | sure | Alert/attack ranges and facing on the local hero, sings to needolin. | Enemy state from one game, hits and death. | | scenes/Wisp_09.fsm.txt:4 |
| Wisp_09.2 | Blade Spider Hang ×2 (12 uses) | FSM Behaviour b93e483e3e; C# BoxCollider2DConfigs | Hanging spider dives at hero angle | sure | Attack velocity aimed at GetAngleToTarget2D(local hero); reacts to needolin noise. | Enemy state and dive angle from one game. | | scenes/Wisp_09.fsm.txt:283 |

## Crawl_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Crawl_01.1 | Bone Worm Nest ×N (28 uses) | FSM FSM 4bfca3f54a | Spawns worms while hero in range | sure | Picks a random worm from the pool on a timer while the local hero is in Spawn Range; two games spawn different worms at different times; keeps spawning after Hornet death. | Spawn events (which worm, when) from one game. | Nests driven by the nearer player, or by the host only. | scenes/Crawl_01.fsm.txt:4 |
| Crawl_01.2 | Bone Worm ×N (61 uses) | FSM Control 0b18ce3e4e, Detect Silk Dash 731562bb54, Ground Bouncer Control 02da52a010 | Worm drops at random X, digs, chomps | sure | Random spawn X, faces the local hero, silk-dash hit kills it. | Worm state and spawn X, hits and death. | | scenes/Crawl_01.fsm.txt:219 |
| Crawl_01.3 | Crypt Worm ×N (52 uses) | FSM Control 3479123f9f | Ambush worm, launches at hero, chomps | sure | Ambush range and launch aimed at the local hero; chomp holds the hero. | Enemy state, hits and death. | | scenes/Crawl_01.fsm.txt:3943 |
| Crawl_01.4 | Crypt Worm Dig Effect ×3 | FSM Control 54cc80c8a9 | Dig dust travelling to home | no | Visual tell, local range. | | | scenes/Crawl_01.fsm.txt:3823 |

## Crawl_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Crawl_02.1 | Heart Piece | FSM Behind Wall Control 5dcd59ce2d | Mask shard sound/glow behind breakable wall | maybe | Only audio and glow; the shard pickup and the wall are separate shared-item and breakable cases. | The wall break and the pickup (handled elsewhere). | Shard for both players or the taker only. | scenes/Crawl_02.fsm.txt:938 |
| Crawl_02.2 | PUSTULE (16 uses) | C# LifebloodPustule | Lifeblood blob; extract withers vines | sure | Extracting pins the local hero, gives lifeblood, breaks it for good (persistent) and withers linked vines; partner's vines stay. | The extracted break (pustule id); withering replays natively. | | scenes/Crawl_02.objects.txt:1405 |
| Crawl_02.3 | Tiny Dragonfly (2) | FSM Control bf14382dba | Ambient dragonfly darting | no | Cosmetic. | | | scenes/Crawl_02.fsm.txt:2283 |

## Crawl_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Crawl_03.1 | Bone Worm BlueBlood ×N (11 uses) | FSM Control 63197461e6 | Lifeblood worm digs to hero X, spits | sure | Dig position from the local hero X, random SPIT/CHARGE and direction, lifeblood projectiles, moss-extract kill. | Enemy state and attack choice from one game, hits and death. | | scenes/Crawl_03.fsm.txt:4 |
| Crawl_03.2 | Bone Worm BlueTurret ×2 (5 uses) | FSM Control f5dc0a307a | Turret worm shoots; extract holds hero | sure | Shoots at the local hero; EXTRACT holds the hero still beside it. | Enemy state, hits and death. | | scenes/Crawl_03.fsm.txt:649 |
| Crawl_03.3 | infected enemies | C# LifebloodState, EventRelayResponder | Regen while unhurt, permanent kill | maybe | Healing runs per game, permanent kill saved only by the killer. | Covered by the enemy death above. | | scenes/Crawl_03.objects.txt:1606 |
| Crawl_03.4 | Wither Folder vines ×59 | C# LifebloodPustuleSubWither | Vines wither when pustule extracted | no | Follows Crawl_02.2. | | | scenes/Crawl_03.objects.txt:578 |
| Crawl_03.5 | Song Gate Needolin Responder | FSM Control b1944fa6de | Glows while hero plays needolin | no | Visual only, local needolin. | | | scenes/Crawl_03.fsm.txt:11396 |
| Crawl_03.6 | Sand Region | C# SandRegion | Sand movement zone for hero | no | Affects the local hero only. | | | scenes/Crawl_03.objects.txt:2704 |

## Crawl_03b

no new elements

## Crawl_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Crawl_04.1 | Roof Crab | FSM Control 6904d59873, Death 7040edd08c | Wakes, ground-chases hero, shoots; big death | sure | ChaseObjectGround on the local hero, alert/angry ranges; corpse plays a boss-style explosion. | Enemy state, hits and death. | | scenes/Crawl_04.fsm.txt:106 |
| Crawl_04.2 | Small Crab ×3 | FSM Behaviour 90e144804e | Harmless crab walks, sings | no | Ambient critter. | | | scenes/Crawl_04.fsm.txt:4 |

## Crawl_05

no new elements

## Crawl_06

no new elements

## Crawl_07

no new elements

## Crawl_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Crawl_08.1 | Blue Scientist Sit | FSM Dialogue c89855feaf | Quest NPC: hand gland, get reward | sure | Moves the local hero, takes the collectable and gives the Extractor Blue Worms reward to the talker only. | Quest turn-in (quest id). | Reward both players or the talker only (recommended). | scenes/Crawl_08.fsm.txt:4 |
| Crawl_08.2 | Blue Scientist | FSM Behaviour 50f9a2a45e, Sing Detect 24271040cc | Walking NPC, dialogue, corpse talk | maybe | Dialogue template and PD BlueScientistMet/TalkedCorpse per player; area state (Infected/Uninfected) can differ between players. | Nothing required. | | scenes/Crawl_08.fsm.txt:71 |

## Crawl_09

no new elements

## Crawl_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Crawl_10.1 | Blue Assistant | FSM Control 03d9c977b1, Phase Control 4e97831a51 | Lifeblood mini-boss, extraction caps HP | sure | HP 1000 enemy, random charge direction, lifeblood projectile barrages, roar stun; extracting pins the local hero and saves PD BlueAssistantBloodCount, which caps max HP; journal and encounter PD on death. | Enemy state and attack choice, hits, extract result (blood count) and death. | Extraction count shared (recommended) or per player. | scenes/Crawl_10.fsm.txt:69 |

## Summary

1. Pull-partner candidates (forced travel or boss)
- 1.1. Bell Centipede door (Bellway_Centipede_additive.2): one-way entry into the Giant Centipede boss, transition to the arena.
- 1.2. Bell Beast post-fight (Bellway_Centipede_Arena.5): sets UnlockedFastTravelTeleport and hard-loads Cinematic_Stag_travel; the other player would be stranded in a dead arena.
- 1.3. City Travel Tube (Bellway_City.1): fast travel; same choice as bellway/beast travel, which is the open "bellbeast travel desync" bug and should get one rule for all fast travel.
- 1.4. Centipede ambush on bellway call (Bellway_Centipede_additive.1): input block + roar stun cutscene and quest start; decide pull, freeze or free.

2. Progress events that leave the partner blocked or behind (share one event each)
- 2.1. Bell Shrine sequence: thread snap and lever hit (Bellshrine.1, .3), stored in StateChangeSequence; 6 shrines use it.
- 2.2. Pay-to-unlock machines: Bellway Toll Machine (11 stations), tube toll, both write a station PD bool and open floor gates.
- 2.3. Path blockers: Coral Crust walls (100 uses), Wind Rock (11), One Way Wall Crystal, Fall Bench sack, Lifeblood pustule extraction.
- 2.4. Quest/collect: Garmond memento quest end, Troupe Hunter flea, Blue Scientist turn-in, Blue Assistant blood count.

3. Enemies with hero targeting or RNG that need one authoritative game: Giant Centipede (head, butt, coils, director), Bone Circler Vicious, Farmer Wisp and wisp lanterns, Farmer Catcher, Bell Goomba, Blade Spiders, Bone Worm nests/worms, Crypt Worm, Bone Worm BlueBlood/BlueTurret, Roof Crab, Blue Assistant, Mite Trap.

4. Gaps and open choices
- 4.1. Zaza Beckon NPC leads one hero across four rooms using the local hero only; needs a "who is followed" rule.
- 4.2. Bone Worm nests spawn by timer + RNG while the local hero is near; two players near different nests produce different worm sets unless one game drives all nests.
- 4.3. Moss/lifeblood extraction (pustules, BlueTurret, Blue Assistant) pins the extracting hero; the partner should see the pin on the other Hornet but the enemy result must be shared.
- 4.4. Bellway Pilgrim, Sherma and Doctor door write many per-player dialogue flags; leaving them per player is fine but the NPCs will look different in each game.
