# Audit 2.17: Hang, Cradle, Clover (Hang_01 .. Hang_17b, Cradle_01 .. Cradle_Destroyed_Challenge_Bench, Clover_01 .. Clover_21)

Slice: catalog/Hang.md (High Halls), catalog/Cradle.md (The Cradle), catalog/Clover.md (Verdania). Verdict means: does coop need to handle it. Enemy rows are `sure` when alert, chase, target or attack choice reads only the local hero or uses a random roll; followers (hands, hitboxes, particles, audio) are `no` or `maybe` because they replay from the parent attack. Scripted scenes (take hero control, set PlayerData, change scene) are `sure` and carry a design choice. "One game decides" means the other game should replay enemy state, HP and death. Random picks (RandomBool, GetRandomChild, SendRandomEvent) differ between games unless the result is shared. New shared notes in `audit/notes/`: CrankPlat, MemoryOrbSceneMap, MemoryOrbSource, InspectDoor, DoorTargetCondition, ParentAngleEvent, TransferVelocityOnCollide. Existing notes used: CameraLockArea, BattleScene, BattleSceneEnemy, MemoryOrb, HarpoonRingChain, HarpoonRingSlider, TestGameObjectActivator, PersistentBoolItem, NPCEncounterStateController, QuestRewardHolder.

## Hang_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_01.1 | CameraLockArea (2) | FSM Control be43702010 | Disables camera lock while ring held | no | Camera box is per player. | | | scenes/Hang_01.fsm.txt:28 |
| Hang_01.2 | Harpoon Ring Citadel (1)/CameraLockArea, (2)/CameraLockArea (2) | FSM FSM f76f5de931 | Enables camera lock while ring held | no | Camera box is per player. | | | scenes/Hang_01.fsm.txt:405 |
| Hang_01.3 | Harpoon Ring Group | FSM Timer 08eaa673d1 | Sends a TICK event on a loop | no | No listener found in this scene, pure timer. | | | scenes/Hang_01.fsm.txt:554 |
| Hang_01.4 | Harpoon Ring VerticalRide ×6 (incl. (1), (2)) | FSM ride f798dfab0a | Ring rises while hero hangs, falls on release | maybe | Ring height follows only the local hero's grab, so a hanging partner appears mid-air with the ring still at the bottom. | The ring GRAB and DETACH of each ring. | Let both players share one ring, or only the first rider moves it. | scenes/Hang_01.fsm.txt:560 |

## Hang_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_02.1 | Camera Diagonal Control | FSM Control 840f57a951 | Locks camera Y along a slope | no | Camera is per player. | | | scenes/Hang_02.fsm.txt:132 |
| Hang_02.2 | Song Pilgrim Maestro | FSM Shift Pos 44725baf92 | Moves spawn x if hero enters from right | maybe | The spawn side depends on which side the local hero entered from, so the two games can start the mini-boss at different x. | The Maestro start position (or let the first entrant decide). | | scenes/Hang_02.fsm.txt:740 |
| Hang_02.3 | Song Pilgrim Maestro (5 uses) | FSM Control 84c2602286 | Mini-boss: chases, slashes, summons tiny flies | sure | Alert, chase, slash and summon rolls (10 to 13 summons, random events) read the local hero and local RNG. | One game decides target and attack; share state, HP, death and the summoned flies. | Target nearest player, or only the one who woke it. | scenes/Hang_02.fsm.txt:781 |
| Hang_02.4 | Song Knight Control/Song Knight Present/Clear Out Group | FSM FSM b584074905 | Marks Maestro persistently gone when Knight present | maybe | The Present or Absent variant is picked from each player's own PlayerData, so one game may fight the Maestro while the other has the resting Knight. | The variant choice (one PlayerData decides the room). | Host PlayerData decides, or the first entrant. | scenes/Hang_02.fsm.txt:1206 |

## Hang_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_03.1 | Song Administrator, Song Administrator (1) (12 uses) | FSM Control 72aa5dea9e | Wall or ground flier: wakes, chases, dives | sure | Wake, chase distance (random 6 to 7.5), attack and evade rolls read the local hero; ADMIN WAKE wakes all administrators of the local game only. | One game decides target and attack; share ADMIN WAKE, state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Hang_03.fsm.txt:567 |
| Hang_03.2 | bucket ×34 (waterwheel_main) | C# ParentAngleEvent | Fires event when bucket tips past angle | no | Local visual splash only (note ParentAngleEvent). | | | scenes/Hang_03.objects.txt:1280 |

## Hang_03_top

no new elements

## Hang_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_04.1 | Song Heavy Sentry ×N (Battle Scene Act3/Wave 1) | FSM Bomb Cast 27614fda59 | Spawns rune bombs singly then in pairs | sure | Bombs are spawned by the casting game only; the other game has no bombs. | The BOMB CAST from the game that decides the attack. | | scenes/Hang_04.fsm.txt:536 |
| Hang_04.2 | Song Heavy Sentry ×N (4 uses) | FSM Control 79a5b279ce | Heavy sentry: walks, slams, evades, casts, jump-slams | sure | Close or far choice, slam, evade and cast are random and read the local hero; it also reads blackThreadWorld and its buddy sentry. | One game decides target and attack; share state, HP and death. | Target nearest player, or only the one who started the battle. | scenes/Hang_04.fsm.txt:564 |
| Hang_04.3 | Garmond Heavy Sleeper NPC | FSM Conversation a6259d8551 | Post-battle dialogue | no | Dialogue only, writes no state. | | | scenes/Hang_04.fsm.txt:1067 |
| Hang_04.4 | Hang Battle Drop Lamp | FSM Drop Control 5368736ace | Lamp drops each heavy wave, ends arena lighting | sure | Reads hang04Battle and blackThreadWorld and follows forum battle events; the partner's lamp never drops unless the battle runs there. | FORUM BATTLE START, HEAVY WAVE and FORUM BATTLE END of the battle owner. | | scenes/Hang_04.fsm.txt:1084 |

## Hang_04_boss

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_04_boss.1 | Battle Scene/Call Ally; Battle Scene/Wave 2 - Call Ally | FSM Control c1c9c55c22; FSM Call Ally 46d1492ba1 | Wave 2 summons Garmond or Shakra ally | sure | Which ally arrives comes from each player's garmondAidForumBattle and shakraAidForumBattle; the ally fights only in the game running the battle. | The ally ENTER FORUM event with which ally. | Ally fights for both players or only the host's game. | scenes/Hang_04_boss.fsm.txt:1, scenes/Hang_04_boss.fsm.txt:10495 |
| Hang_04_boss.2 | Battle Scene/Start Range | FSM Control 12212c8a75 | Boss arena trigger: roar, lock camera, yank hero, start | sure | Only the hero who enters is stunned, pulled and locked in; the gates and fight start only in that game. | The arena START (woken or unwoken path) and the battle END. | Pull the partner into the arena, freeze them, or leave them free. | scenes/Hang_04_boss.fsm.txt:1917 |

## Hang_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_06.1 | Aid NPC Control | FSM Control c0cd1e8d6e | Picks Garmond or Shakra aid NPC from state | maybe | Reads hang04Battle, Shakra Final Quest and garmondWillAidInForumBattle per player, so the two games may show different NPCs. | The aid choice (PlayerData bools). | | scenes/Hang_06.fsm.txt:4 |
| Hang_06.2 | Shakra Aid NPC | FSM Behaviour 3b221923b5 | Asks to join forum battle, sets aid bool | sure | Accepting sets shakraAidForumBattle in one game only and locks only the talker; the partner still gets the first ask. | The aid bool (accepted). | Either player accepting commits the ally for both. | scenes/Hang_06.fsm.txt:211 |

## Hang_06_bank

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_06_bank.1 | Rosary Cannon Object | FSM Control 761e98df46 | Breakable machine, drops rosary pickup | sure | Hits break it and set destroyedRosaryCannonMachine locally; the other game shows an intact machine and no pickup. | The BREAK. | Pickup goes to the first taker, or each player gets one. | scenes/Hang_06_bank.fsm.txt:2153 |
| Hang_06_bank.2 | Chest ×3 (Thieves Here) | FSM Set Opened 854f837396 | Marks thief-scene chests already opened | no | Static flag set at scene load. | | | scenes/Hang_06_bank.fsm.txt:2752 |
| Hang_06_bank.3 | Vent REgion Exit Event | FSM FSM 119bc8599d | Broadcasts VENT REGION EXIT next frame | no | Local steam vent signal. | | | scenes/Hang_06_bank.fsm.txt:4288 |

## Hang_06b

no new elements

## Hang_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_07.1 | Set Grand Reeds Dead | FSM Control 89e89bffc4 | After 1s marks four Grand Reeds dead | maybe | It writes saved bools only; reeds alive in the other game stay alive until reload. | The Chevalier scene start (or just the four saved bools). | | scenes/Hang_07.fsm.txt:738 |
| Hang_07.2 | Song Knight (Chevalier) | FSM Cross Slash Block f3a33301be | Plays block animation on blocked hit | no | Visual response. | | | scenes/Hang_07.fsm.txt:1816 |

## Hang_08

no new elements

## Hang_09

no new elements

## Hang_10

no new elements

## Hang_12

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_12.1 | Last Conductor NPC | FSM Dialogue 9fcd02b8da | Melody quest NPC: meet, needolin prompt, give melody | sure | Talk sets HeardMelodyConductorNoQuest, ConductorWeaverDlgHeard and quest progress for the talker only, and locks the inventory while the prompted melody plays. | The quest and dialogue flags (HasMelodyConductor and the Heard bools). | Quest progress shared by both players, or per player. | scenes/Hang_12.fsm.txt:4 |

## Hang_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_13.1 | Camera Diagonal Control (1) | FSM Control a3dc0bade1 | Locks camera Y along a slope | no | Camera is per player. | | | scenes/Hang_13.fsm.txt:349 |

## Hang_14

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_14.1 | Handmaiden Scene | FSM Control bd912b3b4a | On BREAK kills hanging handmaidens, completes journal | sure | Reads marionettesBurned; the break shakes the camera and drops handmaidens only in the game where the thread snapped. | The BREAK of the thread core. | | scenes/Hang_14.fsm.txt:481 |
| Hang_14.2 | break_thread_core | FSM Break Threads 1ec16f29d3 | Four hits snap the thread, +1 silk per hit | sure | Hit counter, silk and the saved snap live in the hitter's game; the partner's thread stays intact. | Each THREAD HIT (silk goes to the hitter only). | | scenes/Hang_14.fsm.txt:532 |
| Hang_14.3 | laundry_basket, laundry_basket (1) | FSM Behaviour 6baa9ab267 | Basket bonked by hero, tilts, wakes handmaidens | maybe | The bonk comes from the local hero's landing or sprint and sends LAUNDRY BONK to wake enemies in that game. | The LAUNDRY BONK. | | scenes/Hang_14.fsm.txt:9049 |
| Hang_14.4 | Handmaiden Scene/Unvent Range, Vent Range | FSM Control 3af7067133 | Sends VENT EXIT when hero enters | no | Local steam vent signal. | | | scenes/Hang_14.fsm.txt:9243 |
| Hang_14.5 | Handmaiden Scene/Wake Range | FSM Control 103f3b9be8 | Hero entering or bonking wakes handmaidens | sure | Trigger reacts to the local hero only, so handmaidens (HP 48, contact damage) wake in one game. | WAKE and WAKE AWAY from the game that triggered. | Wake for both players, or only the one who entered. | scenes/Hang_14.fsm.txt:9263 |
| Hang_14.6 | Song Handmaiden ×10 | C# CharacterTint | Tints sprite from scene appearance | no | Visual only. | | | scenes/Hang_14.objects.txt:97 |
| Hang_14.7 | laundry_basket terrain detector ×N | C# TransferVelocityOnCollide | Basket bounces off walls | no | Decorative physics. | | | scenes/Hang_14.objects.txt:568 |
| Hang_14.8 | terrain collider ×2 | C# Rb2dFollowWithVelocity | Collider follows target body | no | Decorative physics. | | | scenes/Hang_14.objects.txt:2144 |

## Hang_15

no new elements

## Hang_16

no new elements

## Hang_17b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Hang_17b.1 | Boss Scene - To Additive Load | FSM Control 0b102f181b | Arena camera lock, gates, music | sure | ARENA START and BATTLE START run only for the hero who entered; gates close for them alone. | ARENA START and ARENA END. | Pull the partner into the arena, freeze them, or leave them free. | scenes/Hang_17b.fsm.txt:10 |
| Hang_17b.2 | Song Knight | FSM FSM 98541870db | Multi-hit grab: carries hero to catch point | sure | The grab moves the local hero only. | CATCH HERO and the multi-hit start. | Grab the nearest player only. | scenes/Hang_17b.fsm.txt:478 |
| Hang_17b.3 | Song Knight | FSM Save Hero f6b882164a | After grab, clamps hero x into arena | sure | Same local-hero reposition as the grab. | Covered by Hang_17b.2. | | scenes/Hang_17b.fsm.txt:502 |
| Hang_17b.4 | Corpse Song Knight(Clone) | FSM Death f2530c0d32 | Death: ends Song Knight quest, item appears | sure | Quest end and the collectable pickup belong to the killer's game; the partner's arena stays closed. | BOSS DEATH and ARENA END. | Item and quest completion for both, or only the killer. | scenes/Hang_17b.fsm.txt:1588 |

## Cradle_01

no new elements

## Cradle_01_Destroyed

no new elements

## Cradle_02

no new elements

## Cradle_02b

no new elements

## Cradle_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cradle_03.01 | beast_slash_animation (+ fast, unused) | FSM Deal Quick Damage 12e14816f7, d441473fd1 | Cutscene slashes deal 2 damage to hero | sure | Calls TakeQuickDamage on the local hero only. | The BEAST SLASH attack event (one game decides). | Damage the hero it targets, or both. | scenes/Cradle_03.fsm.txt:4, scenes/Cradle_03.fsm.txt:19 |
| Cradle_03.02 | Bell_Hub_Attack_Effect | FSM Control 20a5fb631b | Bell pulse visual on stun, needolin, death | no | Visual only. | | | scenes/Cradle_03.fsm.txt:31 |
| Cradle_03.03 | Boss Title | FSM Title Control 8809ad8773 | Shows boss title, starts boss music | no | Cosmetic. | | | scenes/Cradle_03.fsm.txt:98 |
| Cradle_03.04 | Death Sequence | FSM Control a75766a8d2 | Final bind, picks ending, loads cinematic | sure | Locks and hides the local hero, sets UnlockSilkFinalCutscene, ending completed, achievement and loads Cinematic_Ending; the ending depends on soulSnareReady and bind choices of that player. | The boss DEATH and the chosen ending. | Pull the partner into the same ending cinematic, let them keep playing, or end the session for both. | scenes/Cradle_03.fsm.txt:123 |
| Cradle_03.05 | Death Sequence Cursed | FSM Contorl 9f35135e99 | Cursed-variant death sequence | sure | Same ending flow with the cursed finish, again only for the local hero. | Same as Cradle_03.04. | Same as Cradle_03.04. | scenes/Cradle_03.fsm.txt:910 |
| Cradle_03.06 | Death Arm; Fling Hornet | FSM fade e535d4c7d4; FSM FSM eca2f00566 | Death cutscene arm fade and empty fling | no | Visual part of the death sequence. | | | scenes/Cradle_03.fsm.txt:997, scenes/Cradle_03.fsm.txt:1004 |
| Cradle_03.07 | Intro Sequence | FSM First Challenge 24782a86f3 | Challenge start: cocoon burst, camera lock, face boss | sure | Faces and locks the entering hero and closes the arena gate in that game only; reads encounteredSilk. | CHALLENGE START and boss BEGIN. | Pull the partner in, freeze them, or leave them free. | scenes/Cradle_03.fsm.txt:1008 |
| Cradle_03.08 | Intro Sequence/Challenge Region | FSM Challenge 791e891700 | Taunt zone: takes hero control, starts challenge | sure | Only the hero in the region loses control and sets disablePause; the other player keeps playing outside. | The SPECIAL CHALLENGE start. | Pull the partner in, freeze them, or leave them free. | scenes/Cradle_03.fsm.txt:1085 |
| Cradle_03.09 | Rubble Fields; Rubble Field L, M, R | FSM Rubble Timer 07a98ea719; FSM FSM 3cdf02d6db | Boulder fields fall and sweep on ATTACK | sure | Boulder x spread is random and the sweep damager hits the local hero. | The ATTACK or ATTACK LONG event (boulder spread seed). | | scenes/Cradle_03.fsm.txt:1152, scenes/Cradle_03.fsm.txt:1163 |
| Cradle_03.10 | Silk Boss | FSM Attack Control ebf450bba8 | Picks hand, dash or web attack at random | sure | Weighted random pattern choice with hero x ranges (33 to 48) and hero catch. | The chosen attack event; hands and strands replay it. | One game decides, or target the nearest player. | scenes/Cradle_03.fsm.txt:1343 |
| Cradle_03.11 | Silk Boss | FSM Phase Control 04bcf42ed5 | HP phases P1 to P6, stagger, death hit | sure | HP thresholds on TOOK DAMAGE; the death hit sets disableInventory, records journal and breaks delivery quest items for the local hero. | Phase changes, stagger and death. | | scenes/Cradle_03.fsm.txt:1914 |
| Cradle_03.12 | Silk Boss | FSM Control 45969a63ee | Boss movement: drift, dash, juke, stun, beast slash | sure | Drift, dash direction and teleport (Beast Shift) read the local hero's x and RNG. | One game decides movement; share state, HP, stun and death. | Target nearest player, or the one who started it. | scenes/Cradle_03.fsm.txt:2088 |
| Cradle_03.13 | Silk Boss Hair; Silk_Hair; Preview/MG/BG Hair | FSM Hair Events 0396d78f34; FSM Animation Control 940aaea264; C# DisableAtRuntime, FollowRotation, ChildUpdateOrdered | Hair animation states and follow | no | Visual only. | | | scenes/Cradle_03.fsm.txt:2454, scenes/Cradle_03.fsm.txt:5781, scenes/Cradle_03.objects.txt:114, scenes/Cradle_03.objects.txt:117, scenes/Cradle_03.objects.txt:134 |
| Cradle_03.14 | DashSlash Effect; DashSlash End Effect | FSM Hitbox Control 97935846eb, 9539e37601 | Toggles dash-slash hitboxes with animation | no | Follows the dash attack, replays with it. | | | scenes/Cradle_03.fsm.txt:2475, scenes/Cradle_03.fsm.txt:2500 |
| Cradle_03.15 | Hand L, Hand R | FSM Hand Control 9ea408f5a9 | Hands stomp, swipe, claw, pincer at hero x | sure | Aim x is the local hero x (clamped 22 to 55); ready flags feed the attack loop. | Driven by the attack event (Cradle_03.10). | | scenes/Cradle_03.fsm.txt:2524 |
| Cradle_03.16 | Finger Blade L, M, R ×14 | FSM Tink 2e255d252b; FSM Control 54ab59af65 | Blades follow hand attacks, tink recoil on hit | maybe | Tink recoils the local hero only; blade control replays from hand events. | Nothing extra beyond Cradle_03.10. | | scenes/Cradle_03.fsm.txt:2818, scenes/Cradle_03.fsm.txt:2856 |
| Cradle_03.17 | silk_cocoon_core ×4; Audio Heartbeat ×5 | FSM Loop Beat Anim 0011e2f9da; FSM Control 50d20f74e1 | Heartbeat animation and audio | no | Ambience. | | | scenes/Cradle_03.fsm.txt:5734, scenes/Cradle_03.fsm.txt:5753 |
| Cradle_03.18 | Enclave Caretaker SoulSnare | FSM Control 1aef9471b5 | Caretaker NPC: hero control, dialogue, sets caretaker flag | sure | Trigger takes control of the local hero and sets caretakerSoulSnareConvo; the soul snare readiness feeds the ending choice. | The convo done bool and soulSnareReady. | Both players hear it, or only the one who entered. | scenes/Cradle_03.fsm.txt:5829 |
| Cradle_03.19 | Rune Particle Follow Hornet; Rune_Glow NeedolinResponse | FSM Control de5b616b3b, 0b93ed58c5 | Rune particles follow hero, glow on needolin | no | Visual only. | | | scenes/Cradle_03.fsm.txt:5941, scenes/Cradle_03.fsm.txt:5962 |
| Cradle_03.20 | Spike Floor 1 to 6 | FSM Control e2da284497 | Floor spikes slam on ATTACK, retract when hero spiked | sure | Spikes damage the hero of the game that runs them and retract on that hero's HERO SPIKED. | The attack event; spikes replay locally. | | scenes/Cradle_03.fsm.txt:5976 |
| Cradle_03.21 | Pattern 1, Pattern 2 | FSM silk_boss_pattern_control 873afd5dcc | Web strand pattern with random shift and flips | sure | Random translate and flip decide where strands appear. | The ATTACK event with the pattern shift. | | scenes/Cradle_03.fsm.txt:6480 |
| Cradle_03.22 | Silk Boss WebStrand ×21 | FSM Hornet Catch 7c56798096; FSM Control 29bb5673f3 | Strand catches hero and stuns | sure | Catches only the local hero (CanHeroTakeDamage) and broadcasts HORNET CAUGHT to the boss locally. | The CATCH HERO. | Catch only the caught player, boss reacts to either. | scenes/Cradle_03.fsm.txt:6518, scenes/Cradle_03.fsm.txt:6527 |
| Cradle_03.23 | cradle_spike_plat ×25 | C# CrankPlat | Nail-hit crank lifts a spike platform | sure | Platform speed integrates over hit timing and carries the local hero; hits in one game leave the other platform low (note CrankPlat). | Each crank HIT. | | scenes/Cradle_03.objects.txt:3149 |

## Cradle_03_Destroyed

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cradle_03_Destroyed.1 | Silk Needle Spell Get | FSM Control 889617423e | Interact: bind cutscene, unlock Silk Boss Needle | sure | Hides and moves the interacting hero, adds silk, sets hasSilkBossNeedle and auto-equips the tool; the partner gets nothing. | The pickup (needle unlocked flag). | Each player must do it, or one interaction unlocks it for both. | scenes/Cradle_03_Destroyed.fsm.txt:40 |

## Cradle_Destroyed_Challenge_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cradle_Destroyed_Challenge_01.1 | Hand L old, Hand R old | FSM Control 3654ca5800 | Idle hands follow a boss point | no | Decorative followers. | | | scenes/Cradle_Destroyed_Challenge_01.fsm.txt:22 |
| Cradle_Destroyed_Challenge_01.2 | Centipede Trap ×3 | FSM Hazard Cooldown 29bdd59039; FSM Control 7f37b9db67; FSM Damage Effects dbff4fd3a5 | Trap bursts near hero, fires corpse | sure | Spawn range, node nearest the hero, random waits and the corpse aimed at the local hero all differ per game. | One game decides spawn and burst; share TOOK DAMAGE. | Trap targets the nearest player. | scenes/Cradle_Destroyed_Challenge_01.fsm.txt:3566, scenes/Cradle_Destroyed_Challenge_01.fsm.txt:3926, scenes/Cradle_Destroyed_Challenge_01.fsm.txt:4166 |
| Cradle_Destroyed_Challenge_01.3 | Spike Lazy Flyer ×11 | FSM Blocked Hit Recoil e80dcac679 | Recoil animation on blocked hit | no | Visual response. | | | scenes/Cradle_Destroyed_Challenge_01.fsm.txt:4708 |
| Cradle_Destroyed_Challenge_01.4 | Spike Lazy Flyer ×11 | FSM Control d0e2ddd38f | Patrolling spiked flyer, respawns after hazard reset | sure | HP 50 with contact damage; death and respawn follow the local hero's HAZARD RESPAWNED. | State, HP and death from one game. | | scenes/Cradle_Destroyed_Challenge_01.fsm.txt:4727 |

## Cradle_Destroyed_Challenge_02

no new elements

## Cradle_Destroyed_Challenge_Bench

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cradle_Destroyed_Challenge_Bench.1 | Inspect Door ×2 | C# InspectDoor | Yes or No prompt, then enters door | maybe | Only the interacting hero is sent through; hasEnteredBefore is saved per game. | | Partner follows into the challenge, or stays at the bench. | scenes/Cradle_Destroyed_Challenge_Bench.objects.txt:598 |

## Clover_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_01.1 | door_wakeOnGround | FSM Update Map 9e77f8b646 | Applies queued game map update | no | Map is per player. | | | scenes/Clover_01.fsm.txt:210 |
| Clover_01.2 | Memory Scene/Green Prince Stand (2 uses) | FSM Dialogue 47f9d5aea5 | Memory cutscene: trigger takes hero, two dialogues | sure | The enter trigger locks only the entering hero for the full scene; variant picked by each player's PlayerData. | The cutscene START and END. | Pull the partner in, freeze them, or leave them free. | scenes/Clover_01.fsm.txt:340 |

## Clover_01b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_01b.1 | Grass Goomba (9 uses) | FSM Control 66947b5409 | Hides, wakes, charges at hero and rolls | sure | Wake, attack range and charge read the local hero. | One game decides target and attack; share state, HP and death. | Target nearest player. | scenes/Clover_01b.fsm.txt:59 |
| Clover_01b.2 | Hornet Dragonfly, Hornet Dragonfly (1) (12 uses) | FSM Control b602edfb1f | Flying random patrol, contact damage | maybe | Path is random and ignores the hero, but HP 22 to 35 and kills happen in one game only. | State, HP and death. | | scenes/Clover_01b.fsm.txt:387 |
| Clover_01b.3 | Clover_Zone_Orb_Chart | C# MemoryOrbSceneMap | Needolin arrows to uncollected orbs elsewhere | no | Reads local PlayerData, visual only. | | | scenes/Clover_01b.objects.txt:1407 |
| Clover_01b.4 | Clover_Zone_Orb_Chart children ×24 | C# MemoryOrbSceneMapConditional | Condition for an orb arrow | no | Same visual hint. | | | scenes/Clover_01b.objects.txt:1417 |

## Clover_02c

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_02c.1 | Grasshopper Fly, Slasher (1), Slasher (2) ×14 | FSM Enter Water 78f96b5d62 | Water surface touch sends SPLASH | no | Local splash reaction. | | | scenes/Clover_02c.fsm.txt:481 |
| Clover_02c.2 | Grasshopper Fly ×5 | FSM Control 2a9dba105d | Flier: wakes, teleports near hero, throws | sure | Wake, teleport marker on the hero, random angles and throw target the local hero. | One game decides target and attack; share state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Clover_02c.fsm.txt:489 |
| Clover_02c.3 | Grasshopper Fly, Slasher (1), (2) ×20 | FSM grasshopper_block_control 7731c2f716 | Tracks damaged versus blocked hit | no | Per-hit bookkeeping of the same enemy. | | | scenes/Clover_02c.fsm.txt:868 |
| Clover_02c.4 | Grasshopper Slasher (1), (2) ×9 | FSM Control f7df611db0 | Slasher: listens, alerts, teleports to hero, slashes | sure | Alert, listen range and teleport target read the local hero; battle waves use it. | One game decides target and attack; share state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Clover_02c.fsm.txt:990 |
| Clover_02c.5 | Grasshopper Slasher Crowd ×2 | FSM Control c40f61fe48 | Crowd slasher pops up on alert, wakes slasher | sure | Alert range of the local hero; WAKE goes to the local slasher. | The WAKE. | | scenes/Clover_02c.fsm.txt:1779 |
| Clover_02c.6 | Lilypad Fly ×17 | FSM Control 789032d16f | Critter flees hero or enemy, returns on needolin | no | HP 1 ambient critter. | | | scenes/Clover_02c.fsm.txt:3556 |
| Clover_02c.7 | Lilypad Plat Trap 1 | FSM Control 94963487c9 | Picks random child platform to be a trap | sure | Each game rolls a different child, so a safe lilypad in one game is a trap in the other. | The chosen child index. | | scenes/Clover_02c.fsm.txt:5607 |
| Clover_02c.8 | Memory Orb Group, (1) ×14 | FSM FSM 53900dec5b | Last orb sends ALL ORBS COLLECTED | sure | Completion fires from the collector's bitmask only; the partner's orbs stay uncollected. | The collected orb and completion. | Orbs shared by both players, or per player. | scenes/Clover_02c.fsm.txt:7505 |
| Clover_02c.9 | White Cloverbush Orb ×8; Clover_Statue_Break Orb ×4 | FSM Control fcee9c1a1b, bbc381cfa9 | Hitting bush or statue starts orb chain | sure | The hit starts the orb chain only in the hitter's game. | The START hit. | | scenes/Clover_02c.fsm.txt:7533, scenes/Clover_02c.fsm.txt:7575 |
| Clover_02c.10 | Memory Orb Group, (1) | C# MemoryOrbGroup | Stores collected orbs as PlayerData bitmask | sure | Bitmask written per collector; completion grants a collectable (see note MemoryOrb). | The collected orb bit. | Orbs shared by both players, or per player. | scenes/Clover_02c.objects.txt:2080 |

## Clover_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_03.1 | Lilypad Plat | FSM Randomise Enemy d62c2afb83 | Coin flip: lilypad is a trap | sure | RandomBool per game, so the trap lilypads differ and one player is ambushed alone. | The Is Enemy result. | | scenes/Clover_03.fsm.txt:203 |

## Clover_04b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_04b.1 | Battle Gate Clover (1), (2) ×3 | FSM BG Control e4003a33c8 | Arena gates close and open | sure | Close and open follow the battle in one game only; the partner can walk through. | BG CLOSE and BG OPEN (battle start and end). | Pull the partner into the arena, freeze them, or leave them free. | scenes/Clover_04b.fsm.txt:4 |
| Clover_04b.2 | Grasshopper Slasher Intro (1) to (3) ×6 | FSM Control 4db621126a | Wave 1 slashers rise and leave at battle start | sure | Waits for BATTLE START and rolls random leave angle. | Battle START. | | scenes/Clover_04b.fsm.txt:1351 |
| Clover_04b.3 | Start Next Wave | FSM FSM b0037128d4 | After 3s calls WaveEnd to start next wave | sure | Drives wave progress in the game that runs the battle. | WAVE END (wave index). | | scenes/Clover_04b.fsm.txt:1603 |
| Clover_04b.4 | Grasshopper Child ×6 | FSM Control 9b9843fd4d | Fast flitting child, aims at hero | sure | Flit target is the local hero with random offsets. | One game decides target and attack; share state, HP and death. | Target nearest player. | scenes/Clover_04b.fsm.txt:4084 |
| Clover_04b.5 | Lilypad Trap Setter, (1) | FSM FSM 75a8aaa3f6 | Picks random child lilypad trap | sure | Each game rolls a different trap. | The chosen child index. | | scenes/Clover_04b.fsm.txt:7604 |
| Clover_04b.6 | palace_control | FSM Control 81a998c894 | Shows missing palace unless orb target collected | maybe | Reads CloverMemoryOrbsCollectedTarget, so players with different orb progress see different rooms. | The orb target bool. | | scenes/Clover_04b.fsm.txt:9594 |
| Clover_04b.7 | Green Prince Sit ×2 | FSM Dialogue a4c66779b1 | Green Prince dialogue, needolin sing reaction | no | Dialogue only, no state written. | | | scenes/Clover_04b.fsm.txt:9609 |
| Clover_04b.8 | Orb Source ×12 | C# MemoryOrbSource | Needolin glow toward uncollected orbs | no | Visual hint (note MemoryOrbSource). | | | scenes/Clover_04b.objects.txt:355 |
| Clover_04b.9 | door1 | C# DoorTargetCondition | Sets door destination from PlayerData | maybe | Players with different PlayerData take the same door to different scenes (note DoorTargetCondition). | The condition result. | | scenes/Clover_04b.objects.txt:1041 |

## Clover_05c

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_05c.1 | Hornet Dragonfly BG, BG (1) ×4 | FSM Control 1a558de616 | Background dragonfly patrol | no | No health or damage, decoration. | | | scenes/Clover_05c.fsm.txt:362 |

## Clover_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_06.1 | Cloverstag (2) ×5 | FSM Control 644b5d5870 | Stag enemy: hops, alerts, jumps with random picks | sure | HP 62; alert range and jump choice read the local hero. | One game decides target and attack; share state, HP and death. | Target nearest player. | scenes/Clover_06.fsm.txt:4 |
| Clover_06.2 | White Cloverstag Encounter ×2 | FSM Control 987d9152c4 | Stag calls, then flees; sets encountered bool | maybe | Flees when the local hero is near or hits it and sets encounteredWhiteCloverstagMid; it stays in the other game. | The LEAVE. | | scenes/Clover_06.fsm.txt:2354 |

## Clover_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_10.1 | Boss Scene | FSM Scene End 6942a5c083 | On BOSS DEFEAT returns hero to Clover_01 | sure | Preloads and transitions the local hero only. | BOSS DEFEAT. | Partner follows to Clover_01, or stays. | scenes/Clover_10.fsm.txt:41 |
| Clover_10.2 | Clover Dancer Sickle ×4 | FSM Control 201c1d90ae | Thrown sickle, multi-slash on hit connect | sure | Projectile aimed at the local hero, spawned by the boss game. | Covered by the dancer attack events. | | scenes/Clover_10.fsm.txt:219 |
| Clover_10.3 | Dancer Control | FSM Control 4a43e67830 | Beat-driven boss pattern director | sure | Random sequence and combo choice; one game's AIM and GO beats. | The pattern event (PTN or COMBO) and beat. | One game decides attack; target nearest player. | scenes/Clover_10.fsm.txt:660 |
| Clover_10.4 | Corpse Green Prince(Clone) | FSM Death b6090787a5 | Death: sets defeatedCloverDancers, map update | sure | Result bool and map update belong to one game. | Boss death. | | scenes/Clover_10.fsm.txt:3541 |
| Clover_10.5 | Corpse Green Prince(Clone)/NPC | FSM Control 922a4762b4 | Interact: hero is carried off and hidden | sure | Hides and moves the interacting hero only. | The interaction start. | Interactor only, or both players. | scenes/Clover_10.fsm.txt:3601 |
| Clover_10.6 | Dancer B | FSM Control feae4ee428 | Second dancer: aims, dashes, eye beams, staggers | sure | Aim targets and random look beams read the local hero; ZERO HP stagger local. | One game decides target and attack; share state, HP and death. | Target nearest player. | scenes/Clover_10.fsm.txt:3711 |
| Clover_10.7 | Tornado_Spin Centre Damager, Hero Damager | FSM Slash Effect 526b5ffb67 | Cut effect when tornado is hit | no | Visual. | | | scenes/Clover_10.fsm.txt:14124 |
| Clover_10.8 | Tornado_Spin/Spinner/disperse | FSM FSM d9e802fb72 | Flips tornado by scale | no | Visual orientation. | | | scenes/Clover_10.fsm.txt:14133 |
| Clover_10.9 | Tornado_Spin/Spinner/Hero Damager | FSM Hero Stick a45dda04e3 | Tornado drags hero around the arena | sure | Moves the local hero only, clamped to arena bounds. | The grab on MULTI HIT CONNECT. | Drag the nearest player only. | scenes/Clover_10.fsm.txt:14256 |
| Clover_10.10 | Green Prince Boss NPC | FSM Dialogue 07449c6bc5 | Intro dialogue, closes gates, starts battle | sure | Enter trigger takes the entering hero's control, sets encounteredCloverDancers and fires BATTLE START there only. | Battle START and gates. | Pull the partner into the arena, freeze them, or leave them free. | scenes/Clover_10.fsm.txt:14279 |
| Clover_10.11 | Lilypad Trap Setter | FSM FSM c02bfa458c | Picks random child lilypad trap | sure | Each game rolls a different trap. | The chosen child index. | | scenes/Clover_10.fsm.txt:14492 |

## Clover_10_web

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_10_web.1 | Return Ambush/Ambush/Barrel Breaker | FSM Control 88cba8347b | Trigger breaks barrel to release ambush | sure | Triggered by the local hero entering; ambushers release in that game only. | The ambush trigger. | | scenes/Clover_10_web.fsm.txt:1329 |

## Clover_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_11.1 | Grove_barrel_side spawner/Waker | FSM Wake 38fcd1b840 | Hero standing in trigger wakes spawner | maybe | Trigger reads the local hero, so barrel spawns start in one game. | The WAKE. | | scenes/Clover_11.fsm.txt:1036 |
| Clover_11.2 | Memory Orb Group/Grove_barrel_white (1) | FSM Control 391f336b03 | Breaking barrel starts orb chain | sure | The break starts the chain only in the hitter's game. | The START hit. | | scenes/Clover_11.fsm.txt:1072 |

## Clover_16

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_16.1 | Cloverstag (6) | FSM FSM c09d6082c6 | Sends ALERT after 2s to start stag | maybe | Fires on load, so the stag acts per game (stag behavior in Clover_06.1). | | | scenes/Clover_16.fsm.txt:633 |
| Clover_16.2 | Lilypad plat Setter (1) | FSM Set 544bab07c5 | Picks random child lilypads | sure | Each game rolls different children. | The chosen child index. | | scenes/Clover_16.fsm.txt:3729 |

## Clover_18

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_18.1 | Dragonfly Large ×8 | FSM Control 30c488c521 | Passive dragonfly patrols random nodes | maybe | HP 70 and killable, no contact damage; random path differs per game. | State, HP and death. | | scenes/Clover_18.fsm.txt:4 |
| Clover_18.2 | Dragonfly Large sprite ×8 | FSM Control 26f6d0e03e | Refresh event on sprite | no | Visual. | | | scenes/Clover_18.fsm.txt:82 |
| Clover_18.3 | Fountains Group | FSM FSM e700fc5656 | Checks three shrine plates, completes fountains | sure | Plate bools are saved only when a plate drops locally. | SHRINE PLATE DROPPED. | | scenes/Clover_18.fsm.txt:356 |
| Clover_18.4 | thread_memory_orb_source ×7 | FSM FSM 9012360082 | Needolin blooms orbs, can start boss | sure | Blooms for the player who plays the needolin; BOSS START fires only there. | The bloom or BOSS START. | Boss for the player who summoned, or both. | scenes/Clover_18.fsm.txt:490 |
| Clover_18.5 | Plinth Inspect | FSM Sequence f9a1130c18 | Lectern: dialogue, needolin prompt, summons lake orbs | sure | Locks the interacting hero and sets summonedLakeOrbs once. | The summoned bool. | Interactor only, or both players. | scenes/Clover_18.fsm.txt:906 |
| Clover_18.6 | Orb Source | C# MemoryOrbSourceGroup | Glow hint toward uncollected orb groups | no | Visual hint (note MemoryOrbSource). | | | scenes/Clover_18.objects.txt:1601 |

## Clover_19

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_19.1 | Cloverstag White Boss | FSM Control c472e9430f | Boss: roar, teleport, jump away, cast sickles | sure | Waits for the local hero position, stuns and locks it, teleport points and choices random. | One game decides target and attack; share boss state, HP, death and phase. | Target nearest player; pull the partner in or let them stay. | scenes/Clover_19.fsm.txt:1 |
| Clover_19.2 | Cloverstag White Boss | FSM Protect from underneath nail 9a31163998 | Reacts when hero is below it | maybe | Reads the local hero's position under the boss. | Covered by Clover_19.1. | | scenes/Clover_19.fsm.txt:409 |
| Clover_19.3 | Cloverstag White Boss | FSM Cast Timer 5c154c30b7 | Cast cooldown by phase | no | Timer for the same attack loop. | | | scenes/Clover_19.fsm.txt:429 |
| Clover_19.4 | Cloverstag White Boss | FSM Cast Control 07835a02a8 | Spawns sickles at random height each side | sure | Random side and height, spawn x from the hero x. | The cast event (side and height). | | scenes/Clover_19.fsm.txt:451 |
| Clover_19.5 | Cloverstag White Boss ×9 | FSM Harpoon Evade 595bc5aeca | Dodges when hero throws harpoon | sure | Reads the local hero facing and distance, only on its harpoon throw. | The THROWING HARPOON. | | scenes/Clover_19.fsm.txt:490 |
| Clover_19.6 | Corpse White Cloverstag(Clone) | FSM Disappear 6bcdea211b | Death: BOSS DEFEATED, camera end, orb pop | sure | Fires BOSS DEFEATED and FIGHT END in one game only. | Boss death. | | scenes/Clover_19.fsm.txt:508 |
| Clover_19.7 | Boss Scene/Memory Orb Group (1) | FSM Pop on boss death and set defeated a77c71e157 | Sets defeatedWhiteCloverstag, opens gates, orb appears | sure | Defeated bool and open gates are in the killer's game; the other arena stays closed. | Boss death and BOSS DEFEATED. | | scenes/Clover_19.fsm.txt:899 |

## Clover_20

no new elements

## Clover_21

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Clover_21.1 | Attack Alerter, Attack Alerter (1) | FSM FSM 81cdce9b7c | Hitting it sends ATTACK ALERT to nearby foes | maybe | Hit by the local hero; only that game's grasshoppers are alerted. | The ATTACK ALERT. | | scenes/Clover_21.fsm.txt:4 |

## Summary

### Events that should pull the partner to the player who triggered them

1. Cradle_03 intro (Challenge Region, Intro Sequence): takes control of the entering hero, locks the camera and closes the gate; the partner outside would miss the whole boss start.
2. Cradle_03 death sequence and ending (Death Sequence, Death Sequence Cursed): hides the hero and loads Cinematic_Ending A, B or C; the ending depends on the local player's soulSnareReady and bind choices. Decide whether both go to one ending, each their own, or the partner keeps playing.
3. Hang_04_boss arena (Start Range): roar stun, pulls the hero, closes gates, calls Garmond or Shakra as ally.
4. Hang_17b Song Knight arena (Control) and Clover_10 Green Prince arena (Green Prince Boss NPC) and Clover_19 Cloverstag arena (Control): gates and camera locks close only around the entering hero.
5. Clover_04b battle (Battle Gate Clover): gates close only for the hero who starts the wave fight; the partner can walk through.
6. Cutscenes that lock one hero: Clover_01 Green Prince Stand memory, Cradle_03 Soul Snare caretaker, Hang_12 Last Conductor melody prompt, Clover_18 Plinth Inspect, Cradle_03_Destroyed Silk Needle get.

### Events to share regardless of who is nearby

1. Boss attack decisions: Silk Boss Attack Control, Silk Boss Control, Dancer Control, Cloverstag White Boss Cast Control, Heavy Sentry Control, Song Pilgrim Maestro. One game decides, the other replays attack, HP and death. Hands, finger blades, spike floors, strands, sickles and hitboxes follow from the attack event.
2. Randomised lilypad traps (Clover_02c.7, Clover_03.1, Clover_04b.5, Clover_10.11, Clover_16.2): each game rolls its own, so the chosen child or Is Enemy flag must be shared or one player lands on a trap the other does not see.
3. Memory orb chain (Clover_02c, Clover_11, Clover_18): the hit or needolin that starts the chain and each collected bit; otherwise the partner's orbs and completion collectable never happen.
4. Persistent switches that show only after reload: Hang_06_bank rosary cannon break, Hang_14 thread core snap (kills handmaidens), Clover_18 shrine plate drops, Hang_07 grand reeds dead.
5. Aid ally accept (Hang_06 Shakra Aid NPC) and the ally call at the forum battle (Hang_04_boss).

### Design choices for the user

1. Boss reward ownership: Song Knight item and quest end (Hang_17b.4), Silk Needle (Cradle_03_Destroyed.1), Clover quests and orbs. First taker, both, or per player.
2. Target rule for enemies and bosses: nearest player, the one who woke it, or host only.
3. Variant picks from PlayerData (Hang_02.4 Maestro or Knight, Hang_06.1 aid NPC, Clover_04b.9 door, Clover_04b.6 palace): one PlayerData must decide the room, otherwise the two players get different enemies or scenes.
4. Hero hazards that target only the local hero (web strand catch, tornado drag, Song Knight grab, beast slash damage): confirm that only the caught player is affected while the boss reacts to either.

### Gaps and unknowns

1. Hang_01.3 Harpoon Ring Group Timer sends TICK; no listener was found in the scene dump, so its purpose is unknown.
2. Hang_17b Collectable Item Pickup Song Knight and Hang_06_bank Rosary Cannon Pickup are pickups; the pickup logic was not audited in this slice (not in these catalogs).
3. Clover_10 runs as a memory-style fight (Exit Memory returns to Clover_01 door_wakeOnGround); how a player enters Clover_10 was not traced.
