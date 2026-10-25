# Audit 2.06: Dock_01 .. Dock_16 (catalog/Dock.md lines 1-135)

Verdict key: sure = coop must handle it, maybe = depends on a design decision or only a mild desync, no = local or cosmetic.
Unless stated otherwise, "the other player" means the player whose game did not run the trigger (each player runs a full game; only the local hero collides with triggers, hazards and alert ranges).
Enemies in this area (Dock Bomber, Shield Dockworker, Tar Slug, Dock Charger, Pilgrim and the Dock Foremen) all read the local hero for aggro, facing, chase and attack direction, so two copies of one enemy fight different players unless one hero is chosen per enemy.

## Dock_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_01.1 | Mines Lift ×2 (Mines Lift, Mines Lift (1)) | FSM [Lift Control] 77f3be960a | Pressure plate sends lift to the other end | sure | A lift called by one player stays at the old end in the other game, and its start end is picked from the local hero height at scene load. | The call (plate touched, direction); the move, chain and cog animations then play in both games. | Who may call it while the partner rides it: any press wins, or only the rider. | scenes/Dock_01.fsm.txt:1148 |
| Dock_01.2 | Head Checker ×2 | FSM [Check Hero Head] a27f5dd628 | Reports hero head touching lift, flips direction | maybe | Only the local hero head triggers the reversal, so a lift blocked in one game keeps going in the other. | The reverse event together with the lift call. | | scenes/Dock_01.fsm.txt:1454 |
| Dock_01.3 | cog1/cog2 ×8 | FSM [Rotate] 313417afdd | Spins cogs while the lift moves | no | Cosmetic, driven by the lift's own moving events. | | | scenes/Dock_01.fsm.txt:1460 |
| Dock_01.4 | BG Chain L/R ×8 | C# LiftChain | Stretches background chain with the lift | no | Visual only. | | | scenes/Dock_01.objects.txt:589 |

## Dock_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_02.1 | Forge Conversation Manager | FSM [FSM] 41a41b8809 | Forge worker and daughter trade idle chatter | no | Random ambient talk (reads blackThreadWorld once); each game may chat differently. | | | scenes/Dock_02.fsm.txt:4 |
| Dock_02.2 | Lift Dropper L | FSM [Move] 20820192ff | Whole coal-bucket lift raises or lowers 36 units | sure | A carrying platform that moves in one game only leaves the other player riding nothing; the RAISE/LOWER sender is not visible in the dump (likely inspector-wired, possibly to the trap bridge below). | The RAISE / LOWER trigger; the 3 s move and jitter then play in both games. | | scenes/Dock_02.fsm.txt:1572 |
| Dock_02.3 | Coal Bucket Plat (7) ×N | FSM [Control] bae3400a6a | Persistent bucket platform rises or lowers on OPEN | sure | Another moving platform with a persistent bool; one game moving it strands the other player. | The OPEN / ACTIVATE trigger. | | scenes/Dock_02.fsm.txt:1609 |
| Dock_02.4 | Coal Region ×3 (95 uses in game) | FSM [Damage Hero] e1c8bc388e; C# EnemyCoalBurn; C# ParticleDamageHero | Burns hero standing on coal, sets enemies alight | no | Each game damages only its own local hero and burns its own enemies, which is what we want. | | | scenes/Dock_02.fsm.txt:1645, scenes/Dock_02.objects.txt:1846, scenes/Dock_02.objects.txt:1521 |
| Dock_02.5 | Tar Slug ×19 | FSM [Behaviour] a74082172e; C# Climber | Wall crawler, spits goop at hero in range | sure | Attack range and needolin reaction read the local hero, and a slug killed in one game stays alive in the other. | Which hero it targets, its attack start and its death; crawling follows. | Target the nearest hero, the first one in range, or let each game keep its own copy. | scenes/Dock_02.fsm.txt:1751, scenes/Dock_02.objects.txt:1514 |
| Dock_02.6 | Corpse Tar Slug Expl ×18 | FSM [Control] 8ec09ddef7 | Slug corpse swells and bursts | no | Replays from the slug death in each game. | | | scenes/Dock_02.fsm.txt:1838 |
| Dock_02.7 | Charger Turn L/R Msg ×2 | FSM [Message] a9d30ebda2 | Zone tells charger to turn around | no | Only passes a bool to the charger entering it, so it follows the charger sync. | | | scenes/Dock_02.fsm.txt:2176 |
| Dock_02.8 | Dock Bomber | FSM [Bomb Blacken (DISABLED)] 821f464ee3 | Disabled blackening tint | no | Disabled and cosmetic. | | | scenes/Dock_02.fsm.txt:2414 |
| Dock_02.9 | Dock Bomber ×11 | FSM [Behaviour] 9ea29700bb | Flies at hero, throws bomb rocks, ambushes | sure | Throw aim, facing, flight distance and aggro read the local hero, so each game's bomber aims at a different player. | Target hero and throw event; rock flight and explosion follow. | Same targeting choice as Dock_02.5. | scenes/Dock_02.fsm.txt:2424 |
| Dock_02.10 | Corpse Dock Bomber ×9 | FSM [Control] f2f056acc4 | Corpse falls, then bursts in gas or lava | no | Follows the bomber death in each game. | | | scenes/Dock_02.fsm.txt:2693 |
| Dock_02.11 | lava_crumble_plat ×N (82 uses in game) | FSM [Control] 2a519f2fcc | Platform drops when stepped on, then returns | maybe | Dropped by one player's landing only; the other game keeps the platform and shows the partner standing on air. | The first collision (Drop Antic start); drop and return then play in both games. | | scenes/Dock_02.fsm.txt:3060 |
| Dock_02.12 | Lift Dropper R | FSM [Control] e252eb1b8c | Persistent lift lowers 46 units after OPEN | sure | A one-way persistent lift (also shakes camera); lowered in one game only, the other player's route differs. | The OPEN trigger; the 3 s lowering follows. | | scenes/Dock_02.fsm.txt:3356 |
| Dock_02.13 | Shield Dockworker ×2 | FSM [Behaviour] 3db048018a | Shield, charge and jump at hero | sure | Shield side, charge direction and jump choice depend on the local hero, and blocking makes it invincible in one game only. | Target hero, attack choice and death. | Same targeting choice as Dock_02.5. | scenes/Dock_02.fsm.txt:3522 |
| Dock_02.14 | Innerworks Trap Bridge Large Long | C# TrapBridgeExtend | Lever extends a bridge, unlocks hidden levers | sure | Either player hits its levers; the hold-open check sees only bodies in this game, so the bridge can retract under the partner. | The lever hit (Open); extension, hold and persistent lever unlock follow. | Whether a remote rider must hold the bridge open. | scenes/Dock_02.objects.txt:1964 |
| Dock_02.15 | lava_rocks_top_glower ×N (1476 uses in game) | C# GlowResponseCoal | Rock glow reacts to coal | no | Pure visual. | | | scenes/Dock_02.objects.txt:4652 |

## Dock_02b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_02b.1 | Dock Charger ×3 | FSM [Control] e765b7cebc | Sleeping or idle charger runs at hero | sure | Alert range, charge direction, pass-through and clamber checks all use the local hero; ambush variant wakes by range. | Target hero, wake and charge start; death. | Same targeting choice as Dock_02.5. | scenes/Dock_02b.fsm.txt:528 |
| Dock_02b.2 | Dock Charger ×5 | FSM [Bomb Blacken] c6fdba49fe | Tint tween after bomb | no | Cosmetic. | | | scenes/Dock_02b.fsm.txt:863 |
| Dock_02b.3 | Dock Guard Thrower Enc1 | FSM [Control] c6299387f5 | Kiting thrower, jumps along points, throws spears | sure | Pursues and aims at the local hero and goes dormant by a range that only the local hero fills. | Target hero, jump and throw events; death. | Same targeting choice as Dock_02.5. | scenes/Dock_02b.fsm.txt:925 |
| Dock_02b.4 | Lift Dropper R | FSM [Control] 0c56f8568b | Persistent lift lowers 46 units after OPEN | sure | Same one-way lift as Dock_02.12. | The OPEN trigger. | | scenes/Dock_02b.fsm.txt:2828 |
| Dock_02b.5 | Song Golem Floor ×2 | FSM [Control] 661d1988aa | Floor bobs when stepped, breaks and reforms | maybe | Breaks on global break or bomb events and on landing, which differ between games; the partner may stand on a missing floor. | The break trigger; break and reform timers follow. | | scenes/Dock_02b.fsm.txt:3184 |
| Dock_02b.6 | thread_memory | FSM [FSM] a4e1776fa6; FSM [Deep Memory Pre Enter Effect] 7046c37dcc | Needolin spot, can pull hero into a memory | sure | Needolin by one player blocks input and plays the burst (and a collapse cutscene if it is a deep memory); activated bool is persistent. | The needolin burst (THREAD MEMORY BURST) and memory entry. | When one player enters a memory: freeze the partner, pull them along, or leave them free. | scenes/Dock_02b.fsm.txt:3664, scenes/Dock_02b.fsm.txt:3783 |
| Dock_02b.7 | Enemy Bool Setter Control ×3 | C# ActivationRange | Turns enemy children on when something enters | maybe | Each game only enables its group when its own hero enters, so a group can be off for one player. | Range entered by either player. | | scenes/Dock_02b.objects.txt:907 |
| Dock_02b.8 | Shell Shard 02/03 Persistent | C# ShellShard | Persistent shard pickup | sure | Only the toucher gets shards and the other game keeps it until reload. | The pickup; the shard disappears in both games. | Both players get shards, or only the picker. | scenes/Dock_02b.objects.txt:3397 |
| Dock_02b.9 | docks_giant_gate | C# TimelineGate | Big gate opens with timeline, persistent | sure | Opened once by a lever or plate; the other game keeps it shut. | The Open call (the lever hit, probably Song_lever_side); timeline follows. | | scenes/Dock_02b.objects.txt:3766 |
| Dock_02b.10 | Wall Bump Particle System ×N | C# ParticleSystemAutoDeactivate | Hides charger bump particles | no | Visual only. | | | scenes/Dock_02b.objects.txt:4185 |

## Dock_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_03.1 | Dock Bomber Ambush | FSM [Control] 8bab25527b | Hero entering blows roof, spawns bombers | sure | Triggered by the local hero entering; sets docksBomberAmbush so the ambush happens once, and the other game has no ambush. | The trigger enter; roof burst and ambushers then replay. | Pull the partner in, or spawn the ambush where the trigger player is. | scenes/Dock_03.fsm.txt:1365 |
| Dock_03.2 | Harpoon Ring Chain ×3 | FSM [Proxy] 68a06a905f; C# HarpoonRingChain | Relays ring connect and disconnect | no | Plumbing and chain visuals; hero state is handled by the ring below. | | | scenes/Dock_03.fsm.txt:3378, scenes/Dock_03.objects.txt:3755 |
| Dock_03.3 | Harpoon Ring Citadel (130 uses in game) | FSM [Control] 938138bf16 | Hero grabs or hangs from ring | maybe | A hero hanging is local hero state; the other game sees no swing, and two heroes cannot both hold one ring. | Grab and drop of the ring. | Show the hanging partner on the ring or ignore it. | scenes/Dock_03.fsm.txt:3389 |
| Dock_03.4 | Harpoon Ring Pull Switch ×3 | FSM [Pull Control] 2d66ae0869 | Pulled down by hanging hero, opens gate | sure | Pulled by one hanging hero; the persistent gate opens only in that game. | Pull Complete (OPEN); pull motion needs only the hanging player. | | scenes/Dock_03.fsm.txt:3522 |
| Dock_03.5 | dock_pipe_trapdoor (2) (107 uses in game) | C# Trapdoor | Lever-driven door, closes unless blocked | sure | Either player hits a lever and the close check sees only this game's bodies, so the door can shut on the partner. | The lever hit (open direction); close timing follows. | | scenes/Dock_03.objects.txt:2968 |
| Dock_03.6 | Platform | C# ActivateColliderWhenNotInside; C# CollisionDepress | Door platform collider and bob | no | Local physics and visuals of the trapdoor. | | | scenes/Dock_03.objects.txt:2987 |

## Dock_03b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_03b.1 | Fields Goomba ×N | FSM [Behaviour] 7b30af7663 | Passive crawler flees hero, sings with needolin | maybe | Flees only from the local hero and a goomba killed in one game stays in the other. | Damage and death. | | scenes/Dock_03b.fsm.txt:332 |

## Dock_03c

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_03c.1 | Docks Alarm | FSM [Control] 3404648ff9 | Alarm bell rings on ALARM RING, hit animation | maybe | Sits inside the Battle Scene and rings when the arena calls it; a nail hit only plays locally. | Nothing extra, it follows the arena start. | | scenes/Dock_03c.fsm.txt:4 |
| Dock_03c.2 | Enviro Region (2) | FSM [Enviro Region] a383271c64 | Sets environmentType while hero inside | no | Local ambience and footsteps from the local hero position. | | | scenes/Dock_03c.fsm.txt:5284 |
| Dock_03c.3 | explode_wall (5) | FSM [Control] 76eda56dcb; FSM [Detect Hit] 15b3a4d79e (explode rock) | Nail hit rumbles then blows wall open | sure | Persistent wall broken by one nail strike (hero must stand in the detector), writes a PlayerData bool and removes updraft blocking only in that game. | The valid STRIKE; rumble, flash and explosion follow. | | scenes/Dock_03c.fsm.txt:5887, scenes/Dock_03c.fsm.txt:6001 |

## Dock_03d

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_03d.1 | Dock Bomber Ambush | FSM [Control] 0558da5876 | Hero entering blows roof, spawns bombers | sure | Same as Dock_03.1. | The trigger enter. | Same as Dock_03.1. | scenes/Dock_03d.fsm.txt:210 |
| Dock_03d.2 | Fields Flyer ×18 | FSM [Control] 311d955a91 | Patrols between points, calls out, sings | maybe | Not hero-aware besides needolin, but killable and its patrol is time-driven per game. | Damage and death. | | scenes/Dock_03d.fsm.txt:794 |
| Dock_03d.3 | Flea Rescue Sleeping ×9 | FSM [Call Out] d2509beacb | Sleeping flea freed by touch, nail or noise | sure | Whoever frees it gets the saved-flea item; the other game keeps the flea asleep and the quest count differs. | The rescue; flea departure replays. | Credit both players or only the rescuer. | scenes/Dock_03d.fsm.txt:1020 |

## Dock_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_04.1 | dock_plat_float_01 terrain collider ×N | FSM [depress_plat] 6b7eaec4f7 | Floating platform dips when landed on | no | Visual dip only. | | | scenes/Dock_04.fsm.txt:84 |
| Dock_04.2 | left1 | FSM [Advance Quest] 3d83c8844d | Begins Black Thread Pt4 Return quest on entry | maybe | Quest starts only for the player who walks through, if hasSuperJump is true. | Quest begin (or both reach it by themselves). | Share quest progress or keep it per player. | scenes/Dock_04.fsm.txt:3090 |
| Dock_04.3 | dock_steam_door ×5 | C# TempGate | Door opens when pressure plate is stepped on | sure | The plate sees only the local hero, so the door opens in one game; persistent id dock_pressure_plate_lock. | The plate Activated event; door open and timed close follow. | | scenes/Dock_04.objects.txt:956 |

## Dock_05

no new elements

## Dock_06_Church

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_06_Church.1 | RestBench (1) | FSM [Record Bench Sit] 80df97db4d | Sets SatAtBenchAfterAbyssEscape on sitting | maybe | Story flag set only for the sitter; the other save lacks it. | The PlayerData bool if both saves must match. | Share story flags or keep per player. | scenes/Dock_06_Church.fsm.txt:898 |

## Dock_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_08.1 | Pilgrim 02 ×10 | FSM [Attack] eec48d9d6d; C# AudioPlayWhenGrounded | Rolling and bouncing wave enemy, breaks things | sure | Wave 3 battle enemy; roll and jump direction use the local hero, and its breaker damages scenery only in that game. | Target hero, attack choice and death. | Same targeting choice as Dock_02.5. | scenes/Dock_08.fsm.txt:1824, scenes/Dock_08.objects.txt:128 |
| Dock_08.2 | Heart Piece | FSM [Heart Container Control] d2141019d2 | Heart piece pickup, +1 piece, freezes hero | sure | Persistent single pickup; picker gets heartPieces +1 and the other game still shows it and lets the partner take it again. | The pickup (piece disappears in both games); each game plays its own UI. | Both players get the piece, or only the picker. | scenes/Dock_08.fsm.txt:5456 |
| Dock_08.3 | Heart Container UI(Clone) ×12 | FSM [Heart Container UI] 9da3498693; FSM [destroy_after_anim] 94ab5e85d8; C# WorldRumblePreventWhileActive | Collect cutscene UI, saves the game | maybe | Local UI sequence that blocks hero control, pausing and rumble. | Nothing, it follows the pickup. | Freeze the partner during it or leave them free. | scenes/Dock_08.fsm.txt:5208, scenes/Dock_08.fsm.txt:5420, scenes/Dock_08.objects.txt:2008 |
| Dock_08.4 | Heart Piece Sound Control/Heart Piece ×15 | FSM [Generate Wave] b63b88b27f; FSM [FSM] 89808eb2aa (Plink) | Idle pulse wave and plink effect | no | Cosmetic. | | | scenes/Dock_08.fsm.txt:5430, scenes/Dock_08.fsm.txt:5653 |
| Dock_08.5 | arrow ×28 (Sign_post_hang_interactive_1) | C# RelativeJointAutoConfigure | Configures hanging sign joint | no | Physics decoration. | | | scenes/Dock_08.objects.txt:1634 |

## Dock_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_09.1 | Boss Scene | FSM [Control] 87ea553e1a | Dock Foremen arena: locks cam, phases, lava, defeat | sure | Starts when the local hero enters Battle Start Range or a minion is hit; sets encounteredDockForemen and defeatedDockForemen, locks camera and plays lava phases. | Fight start (BATTLE START), phase change (CALL THROWER, LAVA START) and defeat. | Pull the partner into the arena, lock them in, or let them stay outside. | scenes/Dock_09.fsm.txt:4 |
| Dock_09.2 | Boss Scene | FSM [Music End] f7ee626166 | Cuts music when both bosses fall | no | Audio only. | | | scenes/Dock_09.fsm.txt:162 |
| Dock_09.3 | Audio Loop Tension | FSM [Control] 6135a239d5 | Tension loop until fight starts | no | Audio only. | | | scenes/Dock_09.fsm.txt:198 |
| Dock_09.4 | Dock Guard Slasher | FSM [Control] 007bdffe46; Projectile FSM [Control] cf18ce6d5d | Boss: slash combos, evade, leap, thrown blade | sure | Boss with random attack picks, leap target and throw direction from the local hero; HP phases at 90, 70 and 50 percent and an HP boost when its brother dies. | Target hero, attack choice and phase; damage and death. | Which hero the boss targets, and whether boss HP scales with two players. | scenes/Dock_09.fsm.txt:210, scenes/Dock_09.fsm.txt:920 |
| Dock_09.5 | Dock Guard Thrower | FSM [Control] b0f23e2fdd | Boss: jumps, throws projectiles at hero | sure | Throw aim and jump point use the local hero; second phase after the Slasher dies. | Target hero, attack choice and phase. | Same as Dock_09.4. | scenes/Dock_09.fsm.txt:1037 |
| Dock_09.6 | Intro Minions: Dock Flyer (1), Dock Worker (2) | FSM [FSM] 741e3116f4; FSM [Control] 9c026012d8 | Scripted watchers leave when fight starts | maybe | A hit on a minion (MINION DAMAGED) starts the fight; otherwise they leave on LEAVE. | The leave event (comes with fight start). | | scenes/Dock_09.fsm.txt:1520, scenes/Dock_09.fsm.txt:1536 |
| Dock_09.7 | Lava_Waterfall L/R, M | FSM [Control] 85d799e7dc; FSM [Control] 44cd267f70; C# IgnoreCollision | Phase hazard: lava columns fall in sequence | sure | Timed lava hazards start with LAVA START and hurt whoever stands there in each game, so timing must match for both players. | LAVA START and LAVA END; the sequence then replays. | | scenes/Dock_09.fsm.txt:1566, scenes/Dock_09.fsm.txt:1621, scenes/Dock_09.objects.txt:812 |
| Dock_09.8 | Lava_Waterfall M/Heat Glow | FSM [Control] e2a79fd84a | Glow and sparks follow lava column | no | Visual. | | | scenes/Dock_09.fsm.txt:1684 |
| Dock_09.9 | Dock Slide Floor ×2 | FSM [Control] 30063a73e9 | Floor slides open (OPEN) and closes (CLOSE) | sure | Opened by the boss phase; a floor open in one game only drops or blocks the partner. | OPEN and CLOSE with the phase events. | | scenes/Dock_09.fsm.txt:2613 |

## Dock_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_10.1 | Sauna Control | FSM [FSM] 4fc317dea6 | Random drip and fog cycle | no | Ambience only. | | | scenes/Dock_10.fsm.txt:804 |

## Dock_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_11.1 | Memory Orb Challenge | FSM [memory_font] 2b660cdd2d | Persistent challenge, reveals needolin memory when done | sure | Completion is a persistent bool plus reward; only the finishing game unlocks it. | Challenge completion. | | scenes/Dock_11.fsm.txt:1589 |
| Dock_11.2 | Needolin Memory | FSM [Control] c2990c2c4f; FSM [Needolin Msg] 0793c94e36 | Plays needolin near it, grants Attunement Increase | sure | Reads the local hero needolin; the reward goes into the finishing player's inventory only. | The needolin start and the final reward event. | Reward both players or only the player who finished. | scenes/Dock_11.fsm.txt:1606, scenes/Dock_11.fsm.txt:1651 |
| Dock_11.3 | Silkfly Cloud and Memory Orb ×N | FSM [Control] 2371888254; C# SilkflyCloud; C# MemoryOrb | Timed orb trail the hero chases and touches | sure | Hero touches collect orbs and set a PlayerData bitmask and timer, so progress and failure differ per game. | Trail start, orb collected, timer fail. | One shared trail with one timer, or one trail per player. | scenes/Dock_11.fsm.txt:1664, scenes/Dock_11.objects.txt:567, scenes/Dock_11.objects.txt:584 |
| Dock_11.4 | Tar Slug Huge ×8 | FSM [Control] 31076cc58e | Large crawler, attacks hero in range, sings | sure | Same enemy class as Dock_02.5: attack range and needolin reaction read the local hero. | Target hero, attack and death. | Same targeting choice as Dock_02.5. | scenes/Dock_11.fsm.txt:1970 |
| Dock_11.5 | Corpse Tar Slug Huge Expl ×8 | FSM [Control] 1e86064cf3 | Corpse bursts into goop | no | Follows the slug death. | | | scenes/Dock_11.fsm.txt:2096 |
| Dock_11.6 | lava_rocks_spike ×N (2052 uses in game) | C# GraphicsTiersActivator | Enables decor by graphics tier | no | Graphics setting. | | | scenes/Dock_11.objects.txt:5955 |

## Dock_12

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_12.1 | Inspect Region | FSM [FSM] 103c7cf385 | Controls inspect dialogue, begins Diving Bell Pt2 quest | maybe | Only the inspecting player begins the quest. | Quest begin if quest state is shared. | Share quest progress or keep it per player. | scenes/Dock_12.fsm.txt:18 |
| Dock_12.2 | Ballow Diving Bell Repair NPC | FSM [Audio] 021c42a8f4 | Plays creak sfx on a sprite frame | no | Audio only. | | | scenes/Dock_12.fsm.txt:38 |
| Dock_12.3 | Ballow Diving Bell NPC ×2 | FSM [Dialogue] 854a44e103 | Dialogue, gives Dock Key once | sure | BallowGivenKey and the key item go only to the talking player, who alone can unlock the door. | The key given (item grant and BallowGivenKey). | Key to both players or only the talker. | scenes/Dock_12.fsm.txt:50 |
| Dock_12.4 | lock (Diving Bell Door) | FSM [FSM] 003e50145a; C# ItemReceptacle | Inspect lock, consumes Dock Key, opens door | sure | Unlock consumes the item and is a persistent door; one game stays locked. | The unlock; door opening follows. | Whether the key is shared and consumed once or per player. | scenes/Dock_12.fsm.txt:210, scenes/Dock_12.objects.txt:29 |

## Dock_13

no new elements

## Dock_14

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_14.1 | Silk Grub Large/Small Cocoon ×34 | C# SilkGrubCocoon | Nail hits give silk, break, drop item | sure | Persistent breakable; hitter gets silk and the drop, and the other game keeps the cocoon. | Each hit and the break; effects and drop replay. | Drop for both players or only the breaker. | scenes/Dock_14.objects.txt:385 |
| Dock_14.2 | Push React ×34 | C# HeroTouchForce | Cocoon sways when hero touches | no | Local physics nudge. | | | scenes/Dock_14.objects.txt:392 |
| Dock_14.3 | Audio Player Actor ×13 | C# StopAudioDuringTransition | Stops break audio on transition | no | Audio only. | | | scenes/Dock_14.objects.txt:397 |

## Dock_15

no new elements

## Dock_16

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dock_16.1 | Flea Rescue BlackSilk ×2 | FSM [Control] 4f5f45c2e0 | Black-silk cage freed by nail hits | sure | Cut by nail hits (HP counter), then a flea emerges from the hero and counts for the quest; other game keeps it caged. | Each cut and the release. | Credit the flea to both players or only the rescuer. | scenes/Dock_16.fsm.txt:402 |
| Dock_16.2 | Lava_Waterfall Set, Set (2) | FSM [Control] 907df8913a | Looping lava columns fall in order | maybe | Free-running timed hazard, so each game is in a different phase and a safe gap in one game burns in the other. | The cycle start time on scene entry. | | scenes/Dock_16.fsm.txt:746 |
| Dock_16.3 | Lava_Waterfall Set (1) | FSM [Control] ba41aefc6d | Looping lava columns, 6 steps | maybe | Same as Dock_16.2. | Same as Dock_16.2. | | scenes/Dock_16.fsm.txt:775 |

## Summary

Events that should pull the partner toward the player who triggered them (or at least hold them), with a one-line reason:

1. Dock_09.1 Dock Foremen boss fight: it locks the camera and the arena (two bosses, lava phases, slide floors), so a partner left outside gets no fight and no lock.
2. Dock_03c Battle Scene with Docks Alarm (Dock_03c.1) and Dock_08 Battle Scene with Pilgrims (Dock_08.1): arena gates close only on the entering player's side.
3. Dock_02b.6 thread_memory (needolin deep memory): blocks the player's input, collapses the hero and may enter a memory, so decide freeze, pull or free for the partner.
4. Dock_08.2 Heart Piece pickup: freezes and invincibilises the picker for the cutscene; pause or free the partner during it.
5. Dock_03.1 / Dock_03d.1 Dock Bomber Ambush: one-shot roof ambush set by the first player entering; decide whether the partner is brought along.

Items both players depend on, where one trigger must run in both games: Dock_01.1 lifts, Dock_02.2, 02.3, 02.12, 02b.4 lift droppers and bucket platforms (their RAISE/LOWER/OPEN senders are not visible in the dump, trace them before coding), Dock_02.14 trap bridge levers, Dock_02b.9 giant gate, Dock_03.4 pull switch, Dock_03.5 trapdoor, Dock_03c.3 exploding wall, Dock_04.3 steam door, Dock_09.9 slide floors, Dock_12.4 diving bell door.

Shared-reward questions (give both players the item, or only the one who acted): Dock_02b.8 shell shards, Dock_03d.3 and Dock_16.1 flea rescues, Dock_08.2 heart piece, Dock_11.2 Attunement Increase, Dock_12.3 Dock Key, Dock_14.1 cocoon drops. These write PlayerData or inventory for one player only.

Enemy targeting: Dock Bomber, Shield Dockworker, Tar Slug, Dock Charger, Dock Guard Thrower Enc1, Pilgrim 02 and both Dock Foremen all use the local hero for aggro and aim; one targeting rule (nearest, first aggro, or per-game copy) decides all of Dock_02.5, 02.9, 02.13, 02b.1, 02b.3, 08.1, 09.4, 09.5 and 11.4.

Timed hazards that run free: Dock_16.2, 16.3 lava columns and lava_crumble_plat (Dock_02.11); sync the start phase or accept that gaps differ between games.

New notes written: TrapBridgeExtend, TimelineGate, TempGate, Trapdoor, HarpoonRingChain, MemoryOrb, ItemReceptacle, SilkGrubCocoon, ActivationRange, ShellShard (all in audit/notes/).
