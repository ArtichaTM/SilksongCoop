# Audit 2.07: Bone_East_01 to Bone_East_Weavehome

Slice: catalog/Bone_East.md (whole file). Several scenes keep an Act 3 copy of their content under `Black Thread States` or `Black Thread World`; the same logic applies to both copies. Hero-targeting means the local hero of the game running the FSM, so every enemy, trap and arena listed as `sure` reacts only to the player whose game runs it.

## Bone_East_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_01.1 | Barrel Plat Lift ×2 | FSM Control 3bfd6084e2 | Lift platform; rises, falls or slams on OPEN | sure | A saved lift moves and its Crusher damages only in the game that received OPEN, so the other player sees a different platform position. | The OPEN trigger (the sender is wired in the inspector, not in this scene's FSMs); each game then plays the move itself. | | scenes/Bone_East_01.fsm.txt:4 |

## Bone_East_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_02.1 | Trapper Barb Trap Landmine ×2 | FSM Control 2d76fabba1 | Mine thrown out; arms, then explodes | maybe | Quest-scene mine goes off when the local hero enters its Trigger Range or hits it, so the partner can walk past a live mine in one game and a dead one in the other. | The mine's ATTACK trigger. | | scenes/Bone_East_02.fsm.txt:80 |
| Bone_East_02.2 | Bone Hunter, Bone Hunter Child ×2 | FSM Control 45fd357b06, d003582edc; FSM Detect Offscreen e537b5ffea | Act 3 hunters fight Garmond with random attacks | sure | The fight runs twice with different random attack picks, so hunters die at different times and Garmond's persistent result can differ between games. | Fight start (hero touching the Garmond Fight Scene trigger) and the outcome; or run it in one game only. | Mirror the fight live from one game, or sync only start and result. | scenes/Bone_East_02.fsm.txt:323; scenes/Bone_East_02.fsm.txt:1174; scenes/Bone_East_02.fsm.txt:314 |
| Bone_East_02.3 | Garmond Fighter | FSM Set Opponents bc9087d2a4 | Waits for hero, then assigns hunters as opponents | sure | Starts per game when the local hero is positioned, so the fight begins at different times. | Same as Bone_East_02.2. | | scenes/Bone_East_02.fsm.txt:2624 |
| Bone_East_02.4 | Fields Flock Flyer ×82 | C# FlockFlyer | Ambient birds flee hero or needolin | no | Cosmetic flee reaction to the local hero only. | | | scenes/Bone_East_02.objects.txt:1670 |

## Bone_East_02b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_02b.1 | Rhino | FSM Control 707fd1b782; FSM FSM 92d5d781bd (Alert Range) | Act 3 rhino rams hero, rock bursts | sure | Rams, turns and aggros only on the local hero (Alert Range sets Entered Alert Range), so the partner is ignored. | Enemy state, as for any enemy. | | scenes/Bone_East_02b.fsm.txt:3618; scenes/Bone_East_02b.fsm.txt:4057 |
| Bone_East_02b.2 | Bone Hunter Msg R | FSM FSM 7838224479 | Tells entering enemy to go right | no | Enemy path hint only. | | | scenes/Bone_East_02b.fsm.txt:926 |
| Bone_East_02b.3 | Detector ×2 (Message Controller) | FSM Control 458868e268, 1d3c961fba | Swaps enemy-guide colliders when something enters | no | Enemy navigation helper, reacts to any collider. | | | scenes/Bone_East_02b.fsm.txt:3555; scenes/Bone_East_02b.fsm.txt:3599 |
| Bone_East_02b.4 | Bone Hunter Child, Bone Hunter ×33 | C# EnemyEdgeControl | Slows enemy at patrol edges | no | Movement clamp only. | | | scenes/Bone_East_02b.objects.txt:736 |

## Bone_East_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_03.1 | Bone Hopper Simple ×10 | FSM Control 06f083d052, Startle Control f623630569 | Hops, idles, startles on damage | maybe | Random hop and idle rolls differ per game and it reacts only to the local needolin, so the herd looks different and a kill is not shared. | Enemy state, as for any enemy. | | scenes/Bone_East_03.fsm.txt:1346; scenes/Bone_East_03.fsm.txt:1504 |
| Bone_East_03.2 | Jump Trail ×15 | FSM Trail Control 9c3da8e147 | Particle trail while jumping | no | Pure visual. | | | scenes/Bone_East_03.fsm.txt:1510 |
| Bone_East_03.3 | Bone Hunter Child Alerter | FSM Alert and Run 9714ebcdc1 | Wakes near hero, faces it, digs away | maybe | Scripted staging wakes only when the local hero is in range, so the partner's game may never show it. | | | scenes/Bone_East_03.fsm.txt:2315 |
| Bone_East_03.4 | explode_wall_norck, explode rock | FSM Control cfbafbb843; FSM Detect Hit e6d8bf65b7 | Hero hit explodes wall, sets PlayerData bool | sure | The wall (saved as DockDashExplodeRock) opens only in the game that registered the hit; the other player still faces the wall. | The STRIKE from the valid hit; each game then explodes the wall itself. | | scenes/Bone_East_03.fsm.txt:5725; scenes/Bone_East_03.fsm.txt:5834 |
| Bone_East_03.5 | weaver_harp_sign_broken_bone_east | FSM Control a8979c7c42 | Glow text when needolin played nearby | no | Local UI hint. | | | scenes/Bone_East_03.fsm.txt:6160 |

## Bone_East_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_04.1 | Bone Hunter Buzzer ×3 | FSM Control ba1b93d0ab | Flying hunter chases hero and pokes | sure | Alerts, chases and pokes only the local hero. | Enemy state, as for any enemy. | | scenes/Bone_East_04.fsm.txt:4 |
| Bone_East_04.2 | Bone Hunter Tiny ×2 | FSM Control cdbc4ff186 | Burrowing walker, resurfaces near hero | sure | Chase Range wakes the run, needolin, X distance at battle spawn, face on emerge and the teleplane near the hero (40 percent, else random) read the local hero only, so the partner is ignored and each game places it differently. | Enemy state, as for any enemy. | | scenes/Bone_East_04.fsm.txt:1043 |
| Bone_East_04.3 | Chunk ×66 (Bone Hunter Tiny corpse) | C# QuickBurn | Corpse chunks burn in coal | no | Cosmetic. | | | scenes/Bone_East_04.objects.txt:1150 |

## Bone_East_04b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_04b.1 | Reminder Look Down | FSM Control 75d7c89177 | Hint when hero stands in trigger | no | Local tutorial hint. | | | scenes/Bone_East_04b.fsm.txt:1455 |

## Bone_East_04c

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_04c.1 | Gilly | FSM Behaviour ba0c74886d | NPC hides, talks, sets gilly PlayerData flags | sure | Dialogue writes gillyMet, gillyIntroduced and the convo flags, and hiding depends on enemies being defeated, so progress differs per player. | The talk completion (flags); the hide and appear state follows. | Per-player conversation progress, or shared once either talks. | scenes/Bone_East_04c.fsm.txt:1251 |
| Bone_East_04c.2 | Gilly | FSM Hiding Audio 405a793f44 | Random call sounds while hidden | no | Audio. | | | scenes/Bone_East_04c.fsm.txt:1523 |
| Bone_East_04c.3 | Slab Fly Large Cage | FSM Control 9b7f89c9a3 | Fly grabs, stomps and spits at hero | sure | Targets the local hero, grabs it and the hero escapes by button mashing; the partner is ignored. | Enemy state, as for any enemy. | If the partner is grabbed, free them or let the other player rescue. | scenes/Bone_East_04c.fsm.txt:1544 |
| Bone_East_04c.4 | Slab Fly Large Cage | FSM Take Silk 2ff247838d | Drains 1 silk from hero while gooped | sure | Writes to the local hero's silk only. | Covered by Bone_East_04c.3. | | scenes/Bone_East_04c.fsm.txt:1536 |
| Bone_East_04c.5 | Slab Fly Large Cage | FSM Taunt Detect 155dde5d06 | Marks challenge heard if taunt within 15 | maybe | The taunt is a local-hero event, so only the taunter's game gets the challenge. | | | scenes/Bone_East_04c.fsm.txt:2081 |
| Bone_East_04c.6 | fly corpse | C# BreakableGenerateCorpse | Persistent breakable flings corpse | maybe | Break and corpse placement follow the Breakable, which is per game. | The Breakable hit, as for any breakable. | | scenes/Bone_East_04c.objects.txt:164 |

## Bone_East_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_05.1 | Flea Rescue Barrel ×4 | FSM Control 7d6527cc70 | Barrel breaks, flea emerges (quest-scene bool) | sure | Rescued flag is saved per scene, so one game keeps showing the barrel after the partner freed the flea. | The final hit (RESCUE); each game then plays the flea release itself. | Flea credit for both players, or only for the player who freed it. | scenes/Bone_East_05.fsm.txt:6652 |
| Bone_East_05.2 | Flea Rescue Barrel ×4 | C# tk2dSpriteInitialiser | Pre-builds sprites | no | Setup only. | | | scenes/Bone_East_05.objects.txt:2245 |

## Bone_East_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_07.1 | Explode Wall Group | FSM Updraft Control 566150157a | Disables mask once wall exploded | sure | Updraft path exists only in the game where the wall was exploded. | The wall explode hit (see explode walls elsewhere in the area). | | scenes/Bone_East_07.fsm.txt:1687 |
| Bone_East_07.2 | Updraft After | FSM Control 469e76d8f2 | Unblocks updraft when boneEast07_openedMidRoof is set | sure | Reads a PlayerData bool and also reacts to MID ROOF EXPLODE live, so the roof stays blocked in the game that missed the explosion. | The MID ROOF EXPLODE trigger. | | scenes/Bone_East_07.fsm.txt:1845 |
| Bone_East_07.3 | Shard Flinger ×3 | FSM FSM a9d6a66f7b | Flings 15 shell shards on explosion | sure | Shell shards are currency flung with random velocity, so only the exploder's game spawns them. | Covered by the explode hit; the pickups follow. | Shards for the hitter only, or shared pickups. | scenes/Bone_East_07.fsm.txt:2687 |
| Bone_East_07.4 | Song Trapdoor ×3 | FSM Forced Open By Beastfly 9d29e4bc1d; FSM Control 584253c18d | Opens on song or when Beastfly Hunt quest is tracked | sure | Opens from the local quest state or an event only in one game, leaving the other player's trapdoor and updraft closed. | The OPEN / FORCE OPEN event. | | scenes/Bone_East_07.fsm.txt:3546; scenes/Bone_East_07.fsm.txt:3556 |

## Bone_East_08

Song Golem boss arena (additive scenes Bone_East_08_boss_golem and Bone_East_08_boss_golem_rest hold the boss itself).

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_08.1 | Boss Scene | FSM Control 8832ae48d4 | Start Range wakes golem, closes walls, sets encounteredSongGolem, saves | sure | Only the hero who walks into Start Range starts the fight and gets locked in; the partner sees a sleeping golem. | Fight start (WAKE / REMEET) and BATTLE END; each game then runs the boss. | Pull partner in at start, lock only the starter, or let the partner join by walking in. | scenes/Bone_East_08.fsm.txt:145 |
| Bone_East_08.2 | Boss Scene | FSM Camera Control 1c64b53eaf | Free-mode camera follows hero Y inside Custom Cam Range | maybe | Camera is local; matters only if the partner is expected in the arena. | | | scenes/Bone_East_08.fsm.txt:104 |
| Bone_East_08.3 | DropBomb Rock SG ×3 | FSM Control 9bc62502d0 | Rock falls when both walls exploded, hits golem | sure | Falling rock sets destroyedSongGolemRock and sends GOLEM EXPLOSION that deals 500 damage and stuns the golem, so it must happen in the fight's game. | The STRIKE (both walls exploded). | | scenes/Bone_East_08.fsm.txt:768 |
| Bone_East_08.4 | Hazard Saver ×3 | FSM Control 4c9f10abe9 | Collider on after a lava death | no | Hero lava respawn helper, local. | | | scenes/Bone_East_08.fsm.txt:1574 |
| Bone_East_08.5 | Rocks L, Rocks M, Rocks R; Dummy ×6; SG Lava Rock ×12 | FSM FSM 2fe2405dba, 742ba7802b; FSM Control f037b06960 | Random lava rocks drop and blow up | sure | Random drop point and rock are picked per game and the rocks hurt the hero, so hazards differ. | The DROP choice (point and rock), or the golem's Punch attack. | | scenes/Bone_East_08.fsm.txt:2371; scenes/Bone_East_08.fsm.txt:2402; scenes/Bone_East_08.fsm.txt:2426 |
| Bone_East_08.6 | Fade Boss Music on Exit | FSM Control df7d3cf109 | Music state | no | Audio. | | | scenes/Bone_East_08.fsm.txt:3940 |
| Bone_East_08.7 | Spatter White R | C# SpatterOrange, AreaEffectTint | Lava splatter and tint | no | Visual. | | | scenes/Bone_East_08.objects.txt:1343 |
| Bone_East_08.8 | Silkcatcher Plant ×48 | C# SilkcatcherPlant | Nail hit gives 2 silk, persistent | maybe | A hit gives silk to the hitter only and leaves the plant in the other game, which allows double harvest. | The plant break (hit). | Silk for the hitter only, or one harvest for both. | scenes/Bone_East_08.objects.txt:1935 |

## Bone_East_08_boss_beastfly

Savage Beastfly boss (Bone Flyer Giant), fought in the same arena as the golem; rematch variant exists.

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_08_boss_beastfly.1 | Bone Flyer Giant | FSM Control d42e9a00df | Boss: charge, stomp, roar summons, lava dive | sure | Chases, charges and stomps via the local hero (DistanceFly and Charge Range), picks random attacks and switches camlock and rage by its own HP. | Enemy and boss state, as for any boss. | Pull partner into the arena or lock only the player who wakes it (Wake Range). | scenes/Bone_East_08_boss_beastfly.fsm.txt:22 |
| Bone_East_08_boss_beastfly.2 | Bone Flyer Giant | FSM FSM 7adce7670f | Special death disabled outside killable X range | sure | Part of the boss HP logic, per game. | Covered by .1. | | scenes/Bone_East_08_boss_beastfly.fsm.txt:1 |
| Bone_East_08_boss_beastfly.3 | Bone Flyer Giant | FSM Notify Boss Scene of Damage cc57534ef5 | Forwards damage to scene as BOSS DAMAGED | sure | Drives the fight state from local hits. | Covered by .1. | | scenes/Bone_East_08_boss_beastfly.fsm.txt:15 |
| Bone_East_08_boss_beastfly.4 | Bone Flyer Giant | FSM Summon Cooldown f2ebda6c10 | Cooldown flag between summons | sure | Gates summons per game. | Covered by .1. | | scenes/Bone_East_08_boss_beastfly.fsm.txt:806 |
| Bone_East_08_boss_beastfly.5 | Bone Flyer Giant | FSM Explosion Stun a68bdb3580 | Gas explosion force-stuns boss | sure | Stun happens only where the explosion occurred. | The GAS EXPLOSION event. | | scenes/Bone_East_08_boss_beastfly.fsm.txt:815 |
| Bone_East_08_boss_beastfly.6 | Summon Enemies, Bone Spitter ×13 | FSM Control 844cd2344c, 3f86928909 | Random spitter summoned, fires at hero | sure | Which spitter appears and its fire angle differ per game and it shoots only the local hero. | The SUMMON choice, then enemy state. | | scenes/Bone_East_08_boss_beastfly.fsm.txt:2415; scenes/Bone_East_08_boss_beastfly.fsm.txt:2426 |
| Bone_East_08_boss_beastfly.7 | Item Chunk After (Defeated) | C# DeparentAfterDelay | Detaches reward chunk; collectable pickup is persistent | sure | The persistent Collectable Item Pickup next to it goes to whoever stands near it in each game. | The pickup collection. | Both players get the item, or only the first. | scenes/Bone_East_08_boss_beastfly.objects.txt:303 |

## Bone_East_08_boss_golem

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_08_boss_golem.1 | Boss Scene | FSM Pre build e2837d0240 | Pre-builds sprites | no | Setup only. | | | scenes/Bone_East_08_boss_golem.fsm.txt:1 |
| Bone_East_08_boss_golem.2 | song_golem | FSM Control 868252a718 | Boss: swipes, aimed slams, coal barrage, stunning roar | sure | Slams aim at the local hero's X and the roar stuns and clamps only that hero; defeat sets a PlayerData bool and kill record. | Boss state, as for any boss; defeat (ZERO HP / BATTLE END). | Pull partner in, or stun and clamp only the triggerer. | scenes/Bone_East_08_boss_golem.fsm.txt:6 |
| Bone_East_08_boss_golem.3 | SG_head | FSM Receive Explosion Dmg 987cf34da5 | Rock explosion deals 500 damage and stuns | sure | Only way to damage the golem; the other game's golem takes no damage. | The GOLEM EXPLOSION event. | | scenes/Bone_East_08_boss_golem.fsm.txt:577 |
| Bone_East_08_boss_golem.4 | SG_head | FSM Phase Control 1c261b414e | Stuns and phase change at HP 451, 326, 201 | sure | Phase follows the local boss HP. | Covered by Bone_East_08_boss_golem.2. | | scenes/Bone_East_08_boss_golem.fsm.txt:591 |

## Bone_East_08_boss_golem_rest

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_08_boss_golem_rest.1 | Golem_rest | FSM Wake be0e64311e | Dormant golem wakes, breaks apart, ACTIVATE GOLEM | sure | Cutscene-like wake only in the starter's game. | Covered by Bone_East_08.1 (WAKE). | | scenes/Bone_East_08_boss_golem_rest.fsm.txt:1 |

## Bone_East_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_09.1 | Wind Rock Double | FSM Updraft Control 9177556856 | Switches updraft once wall exploded | sure | The updraft depends on a persistent rock broken by a hit in one game only. | The rock break hit. | | scenes/Bone_East_09.fsm.txt:2006 |
| Bone_East_09.2 | Seamstress Umbrella Platform, umb_plat_small ×9 | C# FloatingPlatform | Platform bobs and dips under hero | no | Visual. | | | scenes/Bone_East_09.objects.txt:163 |
| Bone_East_09.3 | umb_large ×34 | C# FloatingObject | Sine bob | no | Visual. | | | scenes/Bone_East_09.objects.txt:699 |
| Bone_East_09.4 | umb_plat_small ×3 | C# HeroPlatformCollisionSplit | Hero collides with one collider, others with another | no | Physics detail; assumes one hero collider. | | | scenes/Bone_East_09.objects.txt:907 |

## Bone_East_09b

no new elements

## Bone_East_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_10.1 | Area Title Controller | FSM Avoid player entering from room 38ca301685 | Skip title if entered by door1 | no | Local UI. | | | scenes/Bone_East_10.fsm.txt:118 |
| Bone_East_10.2 | Pilgrim NPC Scene | FSM Control f4e616a1c0 | Picks pilgrim set from PlayerData (brolly, regions, rhino) | maybe | Reads each player's PlayerData, so the two games can show different NPC sets. | | | scenes/Bone_East_10.fsm.txt:338 |
| Bone_East_10.3 | Sherma Docks NPC | FSM Conversation Control a337874a1a | Sherma talks, sets metSherma | sure | Dialogue writes PlayerData and gates later talk lines per player. | The conversation completion. | Per-player or shared Sherma progress. | scenes/Bone_East_10.fsm.txt:425 |
| Bone_East_10.4 | Jitter Bits ×13 (sc_cart_plat) | FSM FSM 06c0eb6c9b | Cart plat shake broadcast | no | Visual. | | | scenes/Bone_East_10.fsm.txt:1207 |
| Bone_East_10.5 | toll door interactible | FSM Unlock Behaviour 56ac323e98 | Pay toll to drop and open Pilgrim's Rest door | sure | Spends currency and opens the door in one game only; PilgrimsRestDoorBroken is read from PlayerData. | The toll paid (door open) event. | Toll paid by one player opens for both. | scenes/Bone_East_10.fsm.txt:2073 |
| Bone_East_10.6 | Gate Smashing (Rhino Scene) | FSM Control fc9e4b19cc; FSM Taunt Check 6f13510fd6 | Offstage rhino rages at gate, taunt riles it | maybe | Dormant by PlayerData flags and riled only by the local taunt or needolin. | | | scenes/Bone_East_10.fsm.txt:2275; scenes/Bone_East_10.fsm.txt:2378 |
| Bone_East_10.7 | Ruckus | FSM Control b4b3f13fa9 | Building shakes during rhinoRuckus | no | Visual. | | | scenes/Bone_East_10.fsm.txt:2389 |
| Bone_East_10.8 | Rosary Thief Group | FSM Deactivate by Gate bcecfcf7e7 | Thief group off when entering via door1 | maybe | Whether the thief exists depends on which door the local player entered by. | | | scenes/Bone_East_10.fsm.txt:2441 |

## Bone_East_10_Church

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_10_Church.1 | Rhino Gate Notify | FSM Control d743cb0ba6 | On OPEN lets rhino escape through gate | sure | Rhino chase and escape depend on a gate state in one game. | The gate OPEN event. | | scenes/Bone_East_10_Church.fsm.txt:801 |
| Bone_East_10_Church.2 | Rhino Unalerter | FSM Unalert Rhino 7a68bc2233 | Hero enters trigger: rhino starts ramming | sure | Rhino aggro starts when the local hero crosses the trigger and keys off hero X. | Enemy state, as for any enemy. | | scenes/Bone_East_10_Church.fsm.txt:810 |

## Bone_East_10_Room

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_10_Room.1 | Bonetown Resident PilgrimsRest Variant | FSM Leave If Ruckus 657abfa273 | Sets pilbyLeftPilgrimsRest if rhinoRuckus | maybe | PlayerData write from the local flag. | | | scenes/Bone_East_10_Room.fsm.txt:688 |
| Bone_East_10_Room.2 | Generic Pilgrim Control | FSM Control bfcc322f52 | Picks crowd from pilgrimRestCrowd | maybe | Reads per-player PlayerData. | | | scenes/Bone_East_10_Room.fsm.txt:885 |
| Bone_East_10_Room.3 | Music Control | FSM Control b87a8211d1 | Shrine music | no | Audio. | | | scenes/Bone_East_10_Room.fsm.txt:925 |
| Bone_East_10_Room.4 | Pilgrims Rest Shop | FSM Dialogue bc25b4b57d | Shop and talk NPC, MetPilgrimsRestShop | sure | Shop stock, purchases and meet flag are per player. | Purchase of limited stock, if shared stock is wanted. | Shared or separate stock. | scenes/Bone_East_10_Room.fsm.txt:948 |
| Bone_East_10_Room.5 | Pilgrims Rest Shop, RestBench | FSM Ruckus Control efd2036cf1, 116740d9ca | Shop and bench disabled during rhinoRuckus, restored on RUCKUS END | sure | Shop and bench are off until the local game gets RUCKUS END, so killing the rhino in one game leaves them off in the other. | The RUCKUS END event. | | scenes/Bone_East_10_Room.fsm.txt:1122; scenes/Bone_East_10_Room.fsm.txt:2275 |
| Bone_East_10_Room.6 | Rhino | FSM Broadcast Death 4bc6055208 | On death sends RUCKUS END, rhinoRuckus off | sure | The rhino and its kill exist per game. | The rhino death. | | scenes/Bone_East_10_Room.fsm.txt:1152 |

## Bone_East_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_11.1 | Bone East 11 Cross Over Group | FSM Masks 34bb1ba2ef | Hides extra foreground masks | no | Visual. | | | scenes/Bone_East_11.fsm.txt:2575 |
| Bone_East_11.2 | DropBomb Rock ×6 | FSM Control 3660994070 | Hit-activated rock falls, breaks floor; persistent | sure | Floor breaks only where the hit landed; persistent flag is saved per game. | The STRIKE. | | scenes/Bone_East_11.fsm.txt:2717 |
| Bone_East_11.3 | Greymoor Entry/crashExit | FSM Control 11431aa629 | Hero enters: control lost, forced transition to Greymoor_01 | sure | Forced scene change takes only the hero who touches the trigger; sets crashingIntoGreymoor. | The transition trigger. | Partner follows to Greymoor_01, or stays. | scenes/Bone_East_11.fsm.txt:2867 |
| Bone_East_11.4 | Multihit Updraft | FSM Updraft Control be669724bd | Stages updrafts after wall and trapdoor | sure | Needs the wall and trapdoor events from Bone_East_11.2 and Bone_East_07.4 in this game. | The wall and trapdoor events. | | scenes/Bone_East_11.fsm.txt:2889 |

## Bone_East_12

Lace boss (first fight, Lace Boss1).

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_12.1 | Battle Gate Left DEMO | FSM BG Control 3fa9d78394 | Arena gate closes and opens | sure | Closes only on the starter's side. | BG CLOSE and BG OPEN. | | scenes/Bone_East_12.fsm.txt:146 |
| Bone_East_12.2 | Battle Scene | FSM FSM d08d5ca637 | Deactivates on BG OPEN | sure | Follows battle end per game. | Covered by the battle end. | | scenes/Bone_East_12.fsm.txt:180 |
| Bone_East_12.3 | Lace Boss1 | FSM Control c0f6ee7a64 | Meeting dialogue then fight; sets encounteredLace1, disablePause, isInvincible | sure | Takes control of the local hero for the meeting dialogue, then attacks only that hero; counters, charges and cross slash aim at it. | Fight start and result; the pre-fight cutscene is a start-of-fight event. | Pull partner in and freeze them during the cutscene, or leave them free. | scenes/Bone_East_12.fsm.txt:186 |
| Bone_East_12.4 | Lace Boss1 | FSM Check Death 2563d9285a | Hero death makes Lace pose | maybe | Reacts to the local hero's death only. | | | scenes/Bone_East_12.fsm.txt:1149 |
| Bone_East_12.5 | Lace Boss1 | FSM Escape CamLock Control 1efb675ac2 | Side camlocks by Lace X | maybe | Camera is local. | | | scenes/Bone_East_12.fsm.txt:1248 |
| Bone_East_12.6 | Lace Boss1 | FSM Detect Lava Damage 17cb321418 | Lace takes lava damage below Y | maybe | Boss HP differs if she falls in lava in one game only. | Covered by .3. | | scenes/Bone_East_12.fsm.txt:1273 |
| Bone_East_12.7 | Corpse Lace1(Clone) | FSM Control 6202216db1 | Lace leaps away, BG OPEN, LACE DEPARTED | sure | Departure and gate opening play where she died. | The BG OPEN and LACE DEPARTED events. | | scenes/Bone_East_12.fsm.txt:1282 |
| Bone_East_12.8 | Cross Slash, hero damager, MultiHit | FSM Multihit 4194983202; FSM Multihitter 91e8bc3029, 437d9d671a | Grabs hero, teleports it, multi-hit damage; parry cancels | sure | Moves and damages the local hero (Hero position set), and the partner's mirror is not affected. | The grab (MULTI HIT CONNECT) so the partner sees it. | Only the grabbed player, or also freeze the partner. | scenes/Bone_East_12.fsm.txt:1357; scenes/Bone_East_12.fsm.txt:1387; scenes/Bone_East_12.fsm.txt:1417 |
| Bone_East_12.9 | Silkfly ×6 (Silkflies w/o Sprint) | FSM Control 76c9dffb15 | Ambient flies leave on LEAVE | no | Visual. | | | scenes/Bone_East_12.fsm.txt:1453 |
| Bone_East_12.10 | Battle Range, Battle Range2, NoThwhip Range ×9 | C# SendFSMEventOnEntry | Any collider entering sends an event to Lace | sure | Starts the fight when any trigger collider enters, without a tag check. | The fight start. | | scenes/Bone_East_12.objects.txt:284 |
| Bone_East_12.11 | Silkflies w/o Sprint | C# DeactivateSavedItemCondition | Group active only if saved item still obtainable | maybe | Depends on per-player saved item state. | | | scenes/Bone_East_12.objects.txt:338 |

## Bone_East_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_13.1 | Dock Flyer Spawner | FSM FSM 7cc13a7400 | OPEN or ACTIVATE enables Dock Flyer enemies | sure | Flyers appear only where the event arrived. | The OPEN event. | | scenes/Bone_East_13.fsm.txt:1797 |
| Bone_East_13.2 | DF Lava Rock ×3 | FSM Control 88bfb5d602 | 85 percent chance rock is disabled at load | maybe | Random roll at load, so the hazard exists in one game and not the other. | | | scenes/Bone_East_13.fsm.txt:2086 |

## Bone_East_14

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_14.1 | Spine Floater ×9 | FSM Control ad7296dc5d; FSM Death Effect efb3a8a6b7 | Floating spine shooter, random patterns, explodes on death | sure | Random spine patterns differ and it reacts to the local needolin and hits. | Enemy state, as for any enemy. | | scenes/Bone_East_14.fsm.txt:4233; scenes/Bone_East_14.fsm.txt:4398 |
| Bone_East_14.2 | Explode Floor | FSM Explode 1601487152 | Bone bridge dust on EXPLODE | maybe | Bridge break must happen in both games. | The EXPLODE trigger. | | scenes/Bone_East_14.fsm.txt:4412 |
| Bone_East_14.3 | Vassal_Rest_Spot ×6 | FSM Dialogue bbf6b07e97; C# SteelSoulQuestSpot | Quest spot dialogue, marks spot seen in PlayerData | sure | SteelQuestSpots IsSeen and dialogue order are per player. | The inspection (spot seen). | Spot seen for one or both. | scenes/Bone_East_14.fsm.txt:6126; scenes/Bone_East_14.objects.txt:3327 |

## Bone_East_14b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_14b.1 | Spine Floater Spine P ×2 | FSM Control 0b59722985 | Smash flings Spine Pickup while Brolly Get quest tracked | sure | Pickup depends on the local quest state and a persistent Activated flag. | The SMASH. | Pickup for the smasher only, or both. | scenes/Bone_East_14b.fsm.txt:4502 |
| Bone_East_14b.2 | AudioEvent Manager ×2 | C# AudioEventManager | Audio pooling | no | Audio. | | | scenes/Bone_East_14b.objects.txt:74 |

## Bone_East_15

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_15.1 | Enemy Break Cage ×3 | FSM Control b6106766d6; FSM Control 150b570216 (Sprite) | Cage with enemy inside; hits break it and release it | sure | Breaking is persistent per cage and frees an enemy in one game only. | The final break hit. | | scenes/Bone_East_15.fsm.txt:1106; scenes/Bone_East_15.fsm.txt:1234 |

## Bone_East_16

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_16.1 | Music Transition ×2 | FSM FSM ea9708fa44 | Music state | no | Audio. | | | scenes/Bone_East_16.fsm.txt:1525 |

## Bone_East_17

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_17.1 | Bone Hunter | FSM Scene Out Dig Return f4ba433a89 | Sends OFFSCREEN when below Y 0 | no | Enemy reposition helper. | | | scenes/Bone_East_17.fsm.txt:3348 |

## Bone_East_17b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_17b.1 | Battle Music | FSM Control 976eb9127e | Counts alerted ants for music | no | Audio. | | | scenes/Bone_East_17b.fsm.txt:611 |
| Bone_East_17b.2 | Bone Hunter Child | FSM Cage Reaction cdc073931d | Becomes a battler when Hornet caged or cage broken | sure | Triggered by the cage events of the local hero. | The cage events. | | scenes/Bone_East_17b.fsm.txt:1874 |
| Bone_East_17b.3 | Flea Rescue Cage ×2 | FSM Control c019d83847 | Hits break cage, rescue flea (quest-scene bool) | sure | Rescued flag saved per scene and the break happens in one game. | The final break hit. | Flea credit for both or the breaker. | scenes/Bone_East_17b.fsm.txt:2608 |
| Bone_East_17b.4 | Hunter Trap Cage ×6 | FSM Control 5cc820dc81 | Cage springs, captures hero, hits break it | sure | If the local hero is inside Capture Range, the hero is moved to the capture point and caged; the partner is not caged. | The cage spring and capture. | Cage only the triggerer, or pull and cage both. | scenes/Bone_East_17b.fsm.txt:2823 |
| Bone_East_17b.5 | Hunter Trap Plate ×13 | FSM Control 96b83d6dfb | Pressure plate springs cages when stepped on | sure | Springs when a hero or enemy collider touches it, so the partner's step triggers nothing in the other game. | The plate STEP. | | scenes/Bone_East_17b.fsm.txt:2914 |

## Bone_East_18

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_18.1 | Bone Hopper Giant ×3 | FSM Control c86226b86b | Roams, kicks, stabs, reacts to needolin | sure | Random jump and idle rolls and attacks on nearby targets differ per game. | Enemy state, as for any enemy. | | scenes/Bone_East_18.fsm.txt:504 |
| Bone_East_18.2 | wild_long_grass ×760 | C# GrassSprite | Grass sprites | no | Visual. | | | scenes/Bone_East_18.objects.txt:90 |
| Bone_East_18.3 | Drift Fly Cloud ×15 | C# DriftflyCloud | Ambient fly cloud | no | Visual. | | | scenes/Bone_East_18.objects.txt:3092 |

## Bone_East_18b

Bone Hunter Trapper boss (Ant Trapper), with a fake trap bench.

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_18b.1 | Boss Scene | FSM Control d7f00c6ef5 | Start Range closes gates, sets encounteredAntTrapper, starts battle | sure | The fight starts and locks only for the hero who enters Start Range. | Fight start and BATTLE END. | Pull partner in or lock only the starter. | scenes/Bone_East_18b.fsm.txt:308 |
| Bone_East_18b.2 | Intro Barb LA ×17 | FSM Control 05f031bb94 | Thrown barb mines explode | sure | Random barb picks and positions differ and they damage the local hero. | The THROW choice. | | scenes/Bone_East_18b.fsm.txt:370 |
| Bone_East_18b.3 | Bone Hunter Trapper | FSM Control 2371e9f32d | Boss: charge, dig, hop, spikes, rage | sure | Targets the local hero with random attacks. | Enemy and boss state. | | scenes/Bone_East_18b.fsm.txt:2758 |
| Bone_East_18b.4 | Bone Hunter Trapper ×3 | FSM hero_binding_check c1c8d29a12 | Tracks hero bind so boss can react | maybe | Reacts to the local hero's bind only. | | | scenes/Bone_East_18b.fsm.txt:3329 |
| Bone_East_18b.5 | Corpse Bone Hunter Trapper(Clone) | FSM Death 920dbdde51 | Sets defeatedAntTrapper, BATTLE END, fanfare | sure | Defeat is a PlayerData write in the killer's game. | The BATTLE END. | | scenes/Bone_East_18b.fsm.txt:3347 |
| Bone_East_18b.6 | Battle Gate Ant Straight ×4 | FSM BG Control 9333bf771d | Arena gates | sure | Close only for the starter. | BG CLOSE and BG OPEN. | | scenes/Bone_East_18b.fsm.txt:3415 |
| Bone_East_18b.7 | Trapper Boss Spike ×3 | FSM Control cafa18d722 | Spikes pop at random L, M or R | sure | Random column per game. | The TRAP choice. | | scenes/Bone_East_18b.fsm.txt:3598 |
| Bone_East_18b.8 | TrapBench, TrapBench (Group 4) | FSM Control 86f5750fa0, 4ff9d55c1c | Fake bench: sitting cages the hero | sure | Hero control is taken and the hero is clamped in the cage, and the partner is unaffected. | The FAKE BENCH SIT. | Cage only the sitter or both. | scenes/Bone_East_18b.fsm.txt:3760; scenes/Bone_East_18b.fsm.txt:5810 |
| Bone_East_18b.9 | TrapBench Cage ×3 | FSM Control aa0293a373 | Cage holds hero, hits break it | sure | Breaking is per game. | The final break hit. | | scenes/Bone_East_18b.fsm.txt:3834 |
| Bone_East_18b.10 | TrapBench Detect Range ×3 | FSM Detect Hero 7ed242c4cd | Bench light when hero near | no | Visual. | | | scenes/Bone_East_18b.fsm.txt:3940 |
| Bone_East_18b.11 | Fields Harpoon Ring Pole ×19 | FSM Control 486ad6fee5; C# HarpoonRingPoleConfigure | Pole bends when hero harpoons the ring | no | Visual reaction to the local harpoon. | | | scenes/Bone_East_18b.fsm.txt:4226; scenes/Bone_East_18b.objects.txt:1996 |
| Bone_East_18b.12 | Hunter Sickle Trap (3) ×8 | FSM Control 24fbb3ca67 | Sickle swings on ACTIVATE, slams, tinks | maybe | Timed swing can hurt the local hero only. | The ACTIVATE. | | scenes/Bone_East_18b.fsm.txt:5018 |

## Bone_East_18c

no new elements

## Bone_East_20

no new elements

## Bone_East_21

no new elements

## Bone_East_22

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_22.1 | Hit Reaction (rope/target) | FSM Untarget On Ant Alert 27adc92a33 | Makes target untargetable once ants alert | maybe | Depends on ants alerting in the local game. | | | scenes/Bone_East_22.fsm.txt:4 |
| Bone_East_22.2 | Target Sequence | C# HitSequence | Ordered target hits complete a puzzle | sure | Counts only hits made in this game, so the puzzle completes only for the hitter. | The sequence completion. | | scenes/Bone_East_22.objects.txt:282 |
| Bone_East_22.3 | Mottled Ant Child NPC ×2 | FSM Dialogue 3726289e6a | Appears after sequence; takes a tool, sets MottledChildGivenTool | sure | Locks a tool and writes PlayerData for one player. | The sequence completion; tool gift per player. | Reward for the hitter only. | scenes/Bone_East_22.fsm.txt:1982 |
| Bone_East_22.4 | Retract Audio | FSM FSM 1280c4dc75 | Audio | no | Audio. | | | scenes/Bone_East_22.fsm.txt:2246 |

## Bone_East_24

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_24.1 | Bone Hopper BG Group, Bone Hopper BG ×13 | FSM Control 277e5dd939, b5305643e1 | Background hoppers spawn and hop | no | Pure background. | | | scenes/Bone_East_24.fsm.txt:612; scenes/Bone_East_24.fsm.txt:635 |

## Bone_East_25

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_25.1 | ant_gate_animated | FSM FSM b88b090a4b | Opens Gate on BG OPEN | sure | Opens when the local battle ends. | BG OPEN. | | scenes/Bone_East_25.fsm.txt:4 |
| Bone_East_25.2 | Bone Hunter Throw ×6 | FSM Control 3e5ed1fdc6 | Wave 1 hunter: slash, dive, throw at hero | sure | Targets the local hero. | Enemy state, as for any enemy. | | scenes/Bone_East_25.fsm.txt:10 |

## Bone_East_26

no new elements

## Bone_East_27

no new elements

## Bone_East_LavaChallenge

Lava climb challenge: floor explodes, entrance door seals, lava rises until the top.

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_LavaChallenge.1 | Battle Scene | FSM Start Sequence d7f2e2b619 | Floor explode starts battle, checks completed | sure | Starts after the floor explode hit in one game. | BATTLE FLOOR EXPLODE. | Pull partner in or let the starter run alone. | scenes/Bone_East_LavaChallenge.fsm.txt:43 |
| Bone_East_LavaChallenge.2 | Battle Scene | FSM End Sequence eaff1f0a76 | Battle end starts rising lava, ceilings, camera | sure | Lava rise starts per game. | BATTLE END. | | scenes/Bone_East_LavaChallenge.fsm.txt:4 |
| Bone_East_LavaChallenge.3 | Rising Lava | FSM Rise Control 871186187d | Lava rises, catches up to hero, sets completedLavaChallenge | sure | Speed and catch-up use the local hero's Y; completion is a PlayerData write. | LAVA START and completion. | Lava follows whoever is lower, or each player alone. | scenes/Bone_East_LavaChallenge.fsm.txt:1835 |
| Bone_East_LavaChallenge.4 | Cam Controller | FSM Camera Control 7dbbf14bbf | Free camera follows hero Y | maybe | Camera is local. | | | scenes/Bone_East_LavaChallenge.fsm.txt:1954 |
| Bone_East_LavaChallenge.5 | Ceiling | FSM Control 85590de133 | Ceilings jitter and break | maybe | Follows the lava start. | Covered by .2. | | scenes/Bone_East_LavaChallenge.fsm.txt:1615 |
| Bone_East_LavaChallenge.6 | lava_crumble_plat respawn | FSM Control 48ed8fe172 | Crumble platform drops and respawns | maybe | Platform state follows the local hero's collision and hazard respawn. | | | scenes/Bone_East_LavaChallenge.fsm.txt:1784 |
| Bone_East_LavaChallenge.7 | entrance_door | FSM FSM 594bce3ff2 | Door closes when hero inside, opens on complete | sure | Seals only the player who entered, so the partner can still leave. | LAVA CHALLENGE COMPLETE and the seal. | Seal both. | scenes/Bone_East_LavaChallenge.fsm.txt:6882 |
| Bone_East_LavaChallenge.8 | explode_floor_battle | FSM Control 4ae868f43a | Hit explodes floor, starts battle | sure | Opens in one game only. | The STRIKE. | | scenes/Bone_East_LavaChallenge.fsm.txt:6904 |
| Bone_East_LavaChallenge.9 | lava_challenge_big_crumble_wall ×4 | FSM Lava Break af7aa4eab5 | Wall breaks when lava touches it | maybe | Follows each game's lava position. | | | scenes/Bone_East_LavaChallenge.fsm.txt:8084 |
| Bone_East_LavaChallenge.10 | Music Control | FSM Control 3bc7c595ef | Music | no | Audio. | | | scenes/Bone_East_LavaChallenge.fsm.txt:8188 |
| Bone_East_LavaChallenge.11 | lava_set splines ×8 | C# FloatEventSpeed | Lava spline animation speed | no | Visual. | | | scenes/Bone_East_LavaChallenge.objects.txt:697 |

## Bone_East_Umbrella

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_Umbrella.1 | Seamstress | FSM Dialogue cfc236afc9 | Meet, Brolly quest offer, gives hasBrolly, dress cinematic | sure | Quest and item progress is per player, and the Brolly dress cinematic takes the talker's control. | The dialogue completion (quest, hasBrolly). | Pull partner into the cinematic, freeze them, or leave them free. | scenes/Bone_East_Umbrella.fsm.txt:4 |

## Bone_East_Weavehome

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_East_Weavehome.1 | Weaver Servitor Broken ×10 | FSM Control 5328ece334 | Looks at hero when needolin played | no | Ambient reaction. | | | scenes/Bone_East_Weavehome.fsm.txt:4 |
| Bone_East_Weavehome.2 | Corpse Weaver Servitor(Clone) ×22 | FSM Control c64b3a5b2f | Corpse flung by hits, breaks | no | Cosmetic debris. | | | scenes/Bone_East_Weavehome.fsm.txt:51 |
| Bone_East_Weavehome.3 | Head Swiveller ×23, Head ×10 | FSM Control ee6e6569ba, 0a6b4a6e30 | Head swivel and beam jitter | no | Visual. | | | scenes/Bone_East_Weavehome.fsm.txt:130; scenes/Bone_East_Weavehome.fsm.txt:148 |
| Bone_East_Weavehome.4 | Weavehome Music Controller ×3 | FSM Control 44f5c4a1fe | Music on WEAVER GATE CLOSED | no | Audio. | | | scenes/Bone_East_Weavehome.fsm.txt:1108 |
| Bone_East_Weavehome.5 | Weaver Servitor ×2 | FSM Control 235ac80326; Head FSM Control 7b301ae3bb | Servitor wakes, walks at 8 from hero, beams | maybe | Follows the local hero only. | | | scenes/Bone_East_Weavehome.fsm.txt:1130; scenes/Bone_East_Weavehome.fsm.txt:1471 |
| Bone_East_Weavehome.6 | Weaver Speed Challenge | FSM Completion 730640cc0f; C# WeaverSpeedChallenge | Sets CompletedWeaveSprintChallenge and Max, opens reward | sure | Records the local hero's speed and writes the PlayerData completion flags. | The completion event. | Reward for the runner only, or both. | scenes/Bone_East_Weavehome.fsm.txt:2427; scenes/Bone_East_Weavehome.objects.txt:620 |
| Bone_East_Weavehome.7 | weaver_speed_panel ×11 | C# WeaverSpeedPanel | Lights by local hero speed | sure | Local hero's speed only. | Covered by .6. | | scenes/Bone_East_Weavehome.objects.txt:622 |
| Bone_East_Weavehome.8 | weaver_lift_power_chamber (1) ×3 | FSM States 7b34415f23 | Two lever hits unlock the lift, saved | sure | Persistent unlock happens where the lever was hit. | The lever hit (LEVER HIT). | | scenes/Bone_East_Weavehome.fsm.txt:2504 |
| Bone_East_Weavehome.9 | Beam Source ×13 | C# TK2DSpriteFadePulse | Sprite fade pulse | no | Visual. | | | scenes/Bone_East_Weavehome.objects.txt:1887 |

## Summary

Events that should pull the partner to the player who triggered them (each is a boss, arena or forced sequence that otherwise locks or changes only one player's game).

1. Song Golem fight start (Bone_East_08.1, Bone_East_08_boss_golem.2): walking into Start Range wakes the golem, closes walls and stuns and clamps the hero during the roar.
2. Savage Beastfly wake (Bone_East_08_boss_beastfly.1): wakes on Wake Range, then camlocks the arena.
3. Lace first fight (Bone_East_12.3, .10): any collider entering the Battle Range starts a hero-control cutscene, gates close; Cross Slash multihit grabs and moves the hero (Bone_East_12.8).
4. Bone Hunter Trapper arena (Bone_East_18b.1, .8): Start Range closes gates; the fake TrapBench cages the sitter.
5. Hunter Trap Plate and Cage (Bone_East_17b.4, .5): stepping on the plate springs cages that move the hero into the cage.
6. Lava challenge (Bone_East_LavaChallenge.1, .7): floor explode hit starts it and the entrance door seals only the player inside.
7. Crash exit to Greymoor_01 (Bone_East_11.3): forced scene transition with lost control; decide whether the partner follows.
8. Seamstress cinematic (Bone_East_Umbrella.1) and Lace meeting: dialogue or cinematic that takes control of one hero; freeze or leave the partner free.
9. Garmond fight (Bone_East_02.2, .3): Act 3 fight simulated twice with random attacks; decide between mirroring one game or syncing start and outcome.

Other points that need a decision:

- Hit-opened walls, rocks, floors and cages (Bone_East_03.4, 07.1 to 07.3, 11.2, LavaChallenge.8, Bone_East_15.1, 17b.3): sync only the final hit event; each game then plays the break itself. The shell shards of Bone_East_07.3 are currency, so decide the sharing rule.
- Rewards and flags that live in PlayerData per player (Gilly, Sherma, Seamstress, Mottled Ant Child, Vassal rest spot, Flea rescue, Beastfly reward pickup): decide whether progress is shared or per player.
- Rhino ruckus (Bone_East_10_Room.5, .6): a kill in one game must also end the ruckus in the other, or the shop and bench stay off.
- Random layouts rolled in each game (DF Lava Rock, Lava Rocks, spike columns, barb mines, Bone Hunter Tiny teleports): sync the chosen outcome from one game if both players should see the same hazard.
- Silkcatcher plants (Bone_East_08.8): a hit gives silk to the hitter only; decide whether one harvest per plant is shared.
