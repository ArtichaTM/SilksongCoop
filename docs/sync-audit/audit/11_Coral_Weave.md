# Audit 2.11: Coral and Weave (Coral_02 .. Coral_Tower_01, Weave_02 .. Weave_14)

Slice: catalog/Coral.md and catalog/Weave.md. Verdict means: does coop need to handle it. Rows with `maybe` are mostly personal state (dialogue, quests, pickups) where the user decides between per-player and shared.

## Coral_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_02.1 | Pilgrim Hiker ×6 | FSM Control 266271f100 | Patrols, blocks, evades, attacks hero | sure | Block/evade/attack are chosen from the range of one hero, so two games would fight two different heroes. | One game decides target and attack choice; share enemy state, HP and death. | Target nearest player or only the player who woke it. | scenes/Coral_02.fsm.txt:2435 |
| Coral_02.2 | Pilgrim Hiker / Coral Judge ×13 | FSM Edge Catch 30913fab3c | Slows enemy at ledge edges | no | Pure movement helper that follows the enemy state. | | | scenes/Coral_02.fsm.txt:2742 |
| Coral_02.3 | Coral Judge ×3 | FSM Control 164d9641e1 | Patrols, charges, jumps, swipes at hero | sure | Jump aim, charge and swipe use the local hero position and weighted random picks, so games diverge. | One game decides; share enemy state, HP, death. | Target nearest player or only the one who alerted it. | scenes/Coral_02.fsm.txt:2756 |
| Coral_02.4 | Coral Judge ×3 | FSM Escape Patrol Range 12bdee01b7 | Swaps patrol range when pushed outside | no | Follows the enemy position only. | | | scenes/Coral_02.fsm.txt:3208 |
| Coral_02.5 | Coral Judge ×3 | FSM Shield Collider 6291067278 | Shield on only while facing hero | maybe | Facing is computed against the local hero, so the shield side differs if the enemy targets the other player. | Follows from sharing the enemy target and facing. | | scenes/Coral_02.fsm.txt:3226 |
| Coral_02.6 | Sand Centipede Hero Damager | FSM Control 43b5a1723d | Drags and damages hero sinking in sand | maybe | Moves and damages only the hero in the local game; the other player just sees a position slide. | Nothing extra if hero position and damage are mirrored. | | scenes/Coral_02.fsm.txt:7457 |
| Coral_02.7 | shell_plat shortcut | FSM Animate Cogs 43e0716c86 | Spins cogs while platform moves | no | Visual follower of the platform. | | | scenes/Coral_02.fsm.txt:7496 |
| Coral_02.8 | shell_plat_hang_adjustable shortcut ×5 | FSM FSM 303adfd7df | Platform moves to destination Y when opened | sure | Both players ride it, and it moves only in the game where OPEN fired. | The OPEN trigger (lever or switch that opens it); each game then moves the platform. | | scenes/Coral_02.fsm.txt:7510 |
| Coral_02.9 | sand_centipede_scuffle ×N | C# SandCentipede | Decor centipedes pop out at random times | no | Ambient visuals with Random.Range timing. | | | scenes/Coral_02.objects.txt:3001 |
| Coral_02.10 | Sand Centipede Group ×N / Sand Centipede Attacker ×N | C# RangeAttackGroup, RangeAttacker | Centipedes lunge at hero near sand | maybe | Group tracks only the local hero, so the partner's hero is never attacked in the other game; the attacker is also shreddable by damagers. | | | scenes/Coral_02.objects.txt:3667, scenes/Coral_02.objects.txt:3672 |
| Coral_02.11 | Sand Centipede Group Damager ×N | C# CogCorpseReaction, BreakItemsOnContact, ForceThunker | Grinds corpses, breaks items, blocks pogo | no | Local physics reactions on contact. | | | scenes/Coral_02.objects.txt:3669, scenes/Coral_02.objects.txt:3670 |

## Coral_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_03.1 | Coral Conch Shooter ×N | FSM Control a3ac1a455f | Hovers at range, shoots hero | sure | Flies relative to the local hero and picks shot direction from its position, so shots diverge. | One game decides; share enemy state, HP, death, shots. | Target nearest player. | scenes/Coral_03.fsm.txt:4 |
| Coral_03.2 | Coral Conch Shooter ×N | FSM flyer_go_up_response 9ee2adc273 | Pushes flyer when forced by regions | no | Movement helper triggered by local zones. | | | scenes/Coral_03.fsm.txt:198 |
| Coral_03.3 | lift mechanism cogs | FSM Control Effects 8fec4ebc5e | Spins cogs when lift opens | no | Visual follower of the lift. | | | scenes/Coral_03.fsm.txt:5293 |

## Coral_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_10.1 | Citadel Ambient Music Control ×4 | FSM Control e9668f1791 | Picks ambience by citadelWoken | no | Audio only. | | | scenes/Coral_10.fsm.txt:849 |
| Coral_10.2 | Citadel Quest Starter | FSM Start Citadel Investigate Quest 9233c9f172 | First entry starts Act 2 quest and autosave | sure | Entering by the first gate sets visitedCitadel, act2Started, begins the quest and unlocks ENDING_B only for the entering player; the other player keeps Act 1 state. | The start (visitedCitadel, act2Started, quest begin) once either player triggers it. | Both players start Act 2 when either enters, or each player starts it on their own entry. | scenes/Coral_10.fsm.txt:863 |
| Coral_10.3 | Gate Quest Ender ×2 | FSM FSM 8aeff3d7d3 | Ends Grand Gate Bellshrines quest | maybe | Completes the quest from SeenLastJudgeGateOpen, so the partner's quest stays open unless that flag is shared. | SeenLastJudgeGateOpen (comes with the gate-open sync in Coral_Judge_Arena). | | scenes/Coral_10.fsm.txt:907 |
| Coral_10.4 | left_firstEntrance | FSM Entry 322364ffb7 | Fades in, walks hero in from the left | no | Controls only the local hero for a few seconds. | | | scenes/Coral_10.fsm.txt:924 |
| Coral_10.5 | Sherma Citadel NPC | FSM Conversation Control 71709fd0d0 | Sherma chime and dialogue, sets metSherma | maybe | Seen/met flags are per player, so the partner would repeat the first meeting dialogue. | metSherma and the shermaCitadel Seen flags. | | scenes/Coral_10.fsm.txt:1058 |
| Coral_10.6 | Song Gate Entrance Right | FSM Set Z 3ca98d3906 | Shifts layer depth by hero side | no | Visual depth only. | | | scenes/Coral_10.fsm.txt:1225 |

## Coral_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_11.1 | Coral Conch Shooter Heavy ×N | FSM Control ac130222ea | Hovers at random distance, shoots hero | sure | Random distance and aim at the local hero diverge between games. | One game decides; share enemy state, HP, death. | Target nearest player. | scenes/Coral_11.fsm.txt:301 |
| Coral_11.2 | Boss Scene (Coral Conch Drillers) | FSM Boss AI 1c3f4d835f | Picks random double-drill attack combos | sure | Weighted random attack choice and positions from the hero X; two games would play different patterns. | The attack choice events (H/V/shoot/stomp) from the deciding game. | | scenes/Coral_11.fsm.txt:782 |
| Coral_11.3 | Boss Scene | FSM Control c90e604915 | Arena start, gates close, defeat flags | sure | Only the player who touches the Start Region closes gates and starts the fight; defeatedCoralDrillers and encountered flags are per save. | Battle start (with gate close) and battle end (defeatedCoralDrillers, gate open). | Pull the partner into the arena at start, lock them out, or let them join freely. | scenes/Coral_11.fsm.txt:876 |
| Coral_11.4 | Boss Scene | FSM needolin_capture_timer afe9b0d775 | Times hero needolin to stall drillers | maybe | Counts only the local hero's needolin time, so the partner's song cannot make the drillers sing. | Whether any player is playing the needolin in the arena. | | scenes/Coral_11.fsm.txt:984 |
| Coral_11.5 | Coral Conch Driller Giant Mourn | FSM Control 35ec2480ea | Mourning drill corpse roars then leaves | maybe | Cutscene tied to the dead boss position; it sends BATTLE END which opens the gates. | Boss defeat (covered by 11.3). | | scenes/Coral_11.fsm.txt:1013 |
| Coral_11.6 | Driller A, Driller B | FSM Death Checker 06c723b346 | Last driller to be hit triggers kill | sure | The two drillers share one kill: whoever takes the last hit decides the death, so both games need the same final hit. | The final kill hit (KILL HIT) once. | | scenes/Coral_11.fsm.txt:1065 |
| Coral_11.7 | Driller A, Driller B | FSM Control fa4343fd0b | Intro roar, drills at hero, stomps, shoots | sure | Targets the local hero's X for every attack and the roar stuns the hero in whichever game runs it. | Boss state and attack events from the deciding game. | Stun the partner too, or only the closer player. | scenes/Coral_11.fsm.txt:1088 |
| Coral_11.8 | Battle Gate Conch ×9 | FSM BG Control 46b2a867ef | Closes or opens arena gates | sure | BG CLOSE fires only for the starter; the partner walks through an open gate or stays locked out. | Battle start and end (same events as 11.3). | | scenes/Coral_11.fsm.txt:2285 |
| Coral_11.9 | Pt Slam, Pt Slam Floor, Pt Slam L, Pt Slam R | FSM FSM 7cbd3ed152, e317a35016, 203495ae5d, b6ebc43a06 | Roof slam dust particles | no | Visual. | | | scenes/Coral_11.fsm.txt:2445, scenes/Coral_11.fsm.txt:2454, scenes/Coral_11.fsm.txt:2464, scenes/Coral_11.fsm.txt:2473 |

## Coral_11b

no new elements

## Coral_12

no new elements

## Coral_19

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_19.1 | rubble_fall_generic_follow_right ×2 | FSM FSM 37d728ddd1 | Rubble follows hero X in trigger | no | Visual follower of the local hero. | | | scenes/Coral_19.fsm.txt:1115 |
| Coral_19.2 | Encounter Scene Control | FSM Control 3ef4aecdb9 | Picks Lace meet or workers from flags | maybe | Choice depends on per-save flags (encounteredLace1, defeatedLastJudge, blackThreadWorld), so games show different casts if flags differ. | Make those flags match (shared progress flags). | | scenes/Coral_19.fsm.txt:1298 |
| Coral_19.3 | Lace NPC Blasted Bridge | FSM Control 4556a0f40b | Forced Lace cutscene with dialogue | sure | Takes hero control, disables pause, sets laceLeftDocks and encounteredLaceBlastedBridge and saves; the other player sees Lace vanish or still stands there. | The cutscene start and the flags (encountered, laceLeftDocks). | Pull the partner in and freeze them, leave them free, or play it once per player. | scenes/Coral_19.fsm.txt:1324 |
| Coral_19.4 | Pharloom World Edge ×2 | FSM Control 4e5be1f160 | Hero-triggered world-edge dialogue, hero walked back | maybe | Takes control of and moves only the local hero; flags encounteredPharloomEdge are per player. | encounteredPharloomEdge / Act3 flags. | Freeze partner during the dialogue or leave them free. | scenes/Coral_19.fsm.txt:1769 |

## Coral_19b

no new elements

## Coral_23

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_23.1 | Coral Spike Goomba ×9 | FSM Control 485b6da27a | Crawler charges at hero, reads wall-broken bool | sure | Charge triggers from the local hero range and the crust-wall persistent decides its start state. | One game decides; share enemy state, HP, death, wall-broken flag. | Target nearest player. | scenes/Coral_23.fsm.txt:722 |
| Coral_23.2 | Coral Crust Tree Spike Hero Damager ×50 | FSM Control 50c9446682 | Spike damage off while hero hooked | no | Switches damage only for the hooking local hero. | | | scenes/Coral_23.fsm.txt:1024 |
| Coral_23.3 | Coral Crust Tree Branch ×N | C# CoralCrustTreeBranchStates | Branch grow and break effects | no | Visual follower of the activator. | | | scenes/Coral_23.objects.txt:1835 |
| Coral_23.4 | Coral Crust Tree Plat ×N | C# ActivatorPlatform | Tree platform, breaks after hits | maybe | Hit counts that break a platform are local, so one game can lose a platform the other still has. | The platform break event. | | scenes/Coral_23.objects.txt:3798 |
| Coral_23.5 | Coral Crust Tree Spike ×50 | C# RandomRotationDelay | Random spike rotation | no | Cosmetic. | | | scenes/Coral_23.objects.txt:4087 |
| Coral_23.6 | Coral Crust Tree Activator ×44 | C# TimedActivator | Hit grows platforms and spikes for a time | sure | One game's hit grows the tree there only, so the other player cannot use or avoid it. | The hit or harpoon hook on the activator; both games run the grow, warn and retract timers. | | scenes/Coral_23.objects.txt:4666 |

## Coral_24

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_24.1 | Coral Conch Stabber ×2 | FSM Control a1f9eef60b | Flyer stabs hero when near | sure | Attack direction and trigger come from the local hero range. | One game decides; share enemy state, HP, death. | Target nearest player. | scenes/Coral_24.fsm.txt:260 |
| Coral_24.2 | Coral Crust Mask ×2 (one unused) | FSM Control f70583de0d | Persistent mask reveals on BREAK | maybe | A break reveals the cover in one game only and is saved. | The break event. | | scenes/Coral_24.fsm.txt:492 |
| Coral_24.3 | Coral_plat_float_green_medium ×2 | FSM plat_dust 815279cf5b | Dust on landing | no | Visual. | | | scenes/Coral_24.fsm.txt:1758 |

## Coral_25

no new elements

## Coral_26

no new elements

## Coral_27

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_27.1 | Charge Drillers | FSM Control 1feba3e925 | Random order and side of drill charges | sure | GetRandomChild order and L/R pick differ per game. | The CHARGE L/R event with the driller order. | | scenes/Coral_27.fsm.txt:4 |
| Coral_27.2 | Driller A, B, C | FSM Control f5733d783e | Emerge, then drill across arena at hero | sure | Drill charge height and hero drag depend on the local hero, so hits land in one game only. | Charge events from the deciding game. | | scenes/Coral_27.fsm.txt:42 |
| Coral_27.3 | Darkness Region (1) | FSM FSM 5aa3ec4738 | Enables darkness after the roar | no | Visual region fed by the roar event. | | | scenes/Coral_27.fsm.txt:1080 |
| Coral_27.4 | Roar Drillers | FSM Audio Control 6e6d4b38ec | Drill emerge/impact audio | no | Audio. | | | scenes/Coral_27.fsm.txt:1323 |
| Coral_27.5 | Coral Conch Driller BG Intro ×4 | FSM Control 31c68514db | Background drillers fly across at intro | no | Intro decoration with no hitbox. | | | scenes/Coral_27.fsm.txt:1336 |
| Coral_27.6 | Coral Conch Driller Roar ×5 | FSM Control 4c1beb1a11 | Drill up or down at random positions | sure | Real enemies (own HP) with random direction and X offsets. | One game decides; share state, HP, death. | | scenes/Coral_27.fsm.txt:1504 |
| Coral_27.7 | Coral Conch Driller Giant Solo | FSM Control f820b455b3 | Boss: roars, drills at hero, phases | sure | Boss roar stuns the hero, targets hero X/Y each attack, battle scene locks only the triggering player. | Battle start, boss state and attack choice, phase change, battle end. | Pull partner into arena and stun them, or let partner stay free. | scenes/Coral_27.fsm.txt:2849 |
| Coral_27.8 | Corpse Coral Conch Driller Giant Solo(Clone) | FSM Death c226baea08 | Boss blast, fanfare, DRILLER KILLED | sure | The DRILLER KILLED event ends the battle and opens gates, and sets the solo-defeated flags. | Boss death. | | scenes/Coral_27.fsm.txt:3564 |

## Coral_28

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_28.1 | Tension Tone Single ×9 | C# TriggerActivateGameObject | Enables tension audio when entered | no | Audio trigger. | | | scenes/Coral_28.objects.txt:2251 |

## Coral_29

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_29.1 | Audio Tension | FSM Fade Out b57b82f996 | Fades tension audio on events | no | Audio. | | | scenes/Coral_29.fsm.txt:4 |
| Coral_29.2 | Audio Zap Boss Distant Call | FSM Control 36377cc64c | Distant boss call audio | no | Audio. | | | scenes/Coral_29.fsm.txt:12 |
| Coral_29.3 | Bolt 1..N ×17 | FSM Control 050b89d531 | Lightning bolts zap near hero | sure | Boss ATTACK events, random shift and rotation, and the hero in Attack Range start each strike. | The boss bolt pattern event (INWARD/OUTWARD/CHASER); each game replays the strikes. | | scenes/Coral_29.fsm.txt:32 |
| Coral_29.4 | Zap Core Enemy | FSM Control d4bb45432b | Boss: teleports, zaps clusters and bolts | sure | Wakes on one hero's range, roar stuns, picks random positions, clusters and bolts, sets defeatedZapCoreEnemy, opens the arena. | Wake/battle start, position choice, attack choice, phase change, defeat. | Pull partner into arena and stun them, or leave partner free. | scenes/Coral_29.fsm.txt:5297 |
| Coral_29.5 | Zap Hazard GrabTendril ×3 | FSM Control 602ad1edb8 | Pulls and damages hero when grabbed | maybe | Moves only the damaged hero in the local game; the other player sees a position slide. | Nothing extra if hero position is mirrored. | | scenes/Coral_29.fsm.txt:5781 |
| Coral_29.6 | Collectable Item Pickup | C# FeatherPhysics | Floaty fall for the boss drop | no | Visual physics of a pickup. | | | scenes/Coral_29.objects.txt:606 |
| Coral_29.7 | Mega Jelly Zap ×38 | C# MegaJellyZap | Zap burst with random offset and rotation | maybe | Boss cluster attack that hits the local hero; random placement differs unless the cluster choice is shared. | Follows the Zap Core attack choice (29.4). | | scenes/Coral_29.objects.txt:770 |
| Coral_29.8 | Zap Grass Cluster ×62 | C# ZapGrassCluster | Grass zaps while something stands inside | no | Reacts to the local tracked hero only and showed no damage logic. | | | scenes/Coral_29.objects.txt:3862 |

## Coral_32

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_32.1 | Coral Crust Wall Tall | FSM Direction Control 26efe14e4d | Disables collider unless entered from top | no | Depends on the local entry gate, and only the local hero collides with it. | | | scenes/Coral_32.fsm.txt:2481 |

## Coral_33

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_33.1 | Garmond Black Threaded Fighter | FSM Control 31bf573311 | Boss: roar, wildslash, stabs, stomp globs | sure | Roar stuns and clamps hero X to 53, closes battle gate, targets hero each attack; Act 3 variant only. | Battle start, boss state and attack choice, stun, battle end. | Pull partner into arena and clamp them, or let partner stay free. | scenes/Coral_33.fsm.txt:53 |
| Coral_33.2 | Dice Game Control | FSM Control 9fcafe12e8 | Picks dice pilgrim state from dicePilgrimState | maybe | State (active/dead/departed) is saved per player, so one game can have no pilgrim. | dicePilgrimState. | | scenes/Coral_33.fsm.txt:1562 |
| Coral_33.3 | Dice Pilgrim | FSM Dialogue 1e4a0db2d3 | Dice NPC talk, met/explained flags, defeated idle | maybe | Dialogue flags and bank-empty state are per save. | metDicePilgrim, dicePilgrimGameExplained, dicePilgrimBank. | Shared pilgrim bank or one bank per player. | scenes/Coral_33.fsm.txt:1584 |
| Coral_33.4 | Die Diamond/Octagon/Triangle ×3 | FSM floating_die_mid_drift ee3580e60b, floating_die 6de72f9306 | Dice float and follow master die | no | Visual dice physics. | | | scenes/Coral_33.fsm.txt:1720, scenes/Coral_33.fsm.txt:1746 |
| Coral_33.5 | Play Point | FSM Play Control f745ebbb0d | Dice wager: rosaries, random roll, bank | sure | Win or loss moves rosaries and dicePilgrimBank; empty bank sets dicePilgrimDefeated and spawns a reward, which would be given twice or not at all. | The round result (bank change, defeated flag, reward taken). | One shared bank/reward, or independent game per player. | scenes/Coral_33.fsm.txt:2128 |

## Coral_34

no new elements

## Coral_35

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_35.1 | Zaza Beckon NPC | FSM Control 569ba2dcf9 | Zaza runs ahead of or behind hero | sure | Leads or follows one hero via the static Hero Is Leading Zaza; with two heroes Zaza has two targets and hides when Garmond wakes. | Zaza state (leader/follower, hide, wake) and who she follows. | Zaza follows the nearest player, one fixed player, or each game has its own Zaza. | scenes/Coral_35.fsm.txt:896 |

## Coral_35b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_35b.1 | Stalactite Group Top, Stalactite Group Bottom | C# DropFloorBreaker | Tower stalactite falls and breaks floors | sure | One-time persistent collapse that changes the route and adds a camera lock. | The Break trigger. | | scenes/Coral_35b.objects.txt:236 |

## Coral_36

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_36.1 | Judgling Ambient Voice | C# AudioSourceFadeControl, TrackTriggerEvent | Fades ambient voice by trigger | no | Audio. | | | scenes/Coral_36.objects.txt:267 |

## Coral_37

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_37.1 | Audio Player Seer Sonar ×2 | FSM Fade Audio In 2175dd1230 | Fades sonar audio in | no | Audio. | | | scenes/Coral_37.fsm.txt:4 |
| Coral_37.2 | Steel Sentinel | FSM Control 7b03236df7 | NPC wakes by hit/song, offers quest, gives reward | maybe | Quest offered, accepted and rewarded through per-player quest/PD state; also updates the map. | Quest accept and reward flags if shared. | Per-player quest or shared quest. | scenes/Coral_37.fsm.txt:12 |

## Coral_38

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_38.1 | Coral Conch Driller ×11 | FSM Control ce7ed1dd50 | Ambush flyer drills down at hero | sure | Chases the local hero; the parent Conch Driller Scene picks Pattern A or B at random, so games spawn different drillers. | The pattern choice, then one game decides; share enemy state, HP, death. | Target nearest player. | scenes/Coral_38.fsm.txt:4 |
| Coral_38.2 | Coral Conch Driller ×61 | FSM FSM 0428895f14 | Forced flyer direction response | no | Movement helper. | | | scenes/Coral_38.fsm.txt:656 |

## Coral_39

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_39.1 | Coral Warrior Grey | FSM Control 160e896b05 | Mini-boss slashes, evades, digs into pit | sure | Random attack picks aimed at the local hero, bound hero blocks it, and Hornet-dead events stop it. | One game decides; share boss state, HP, death. | Target nearest player. | scenes/Coral_39.fsm.txt:97 |
| Coral_39.2 | Coral Warrior Grey | FSM Battle Music 56d847fd6b | Starts battle music | no | Audio. | | | scenes/Coral_39.fsm.txt:749 |
| Coral_39.3 | Corpse Coral Warrior Grey(Clone) | FSM Control 8ab4fabc46 | Death roar, drops sword or sinks in pit | sure | Sets defeatedGreyWarrior and greyWarriorDeathX, and the pit eating it feeds the item spitter. | Warrior death with death position. | | scenes/Coral_39.fsm.txt:759 |
| Coral_39.4 | Coral Warrior Item Spitter, Pit Pickup | FSM Control 238f0837b8, Fling Out 0306cecb0e | Pit spits item after warrior eaten | sure | A collectable appears in one game only unless the eaten event is shared; it is a one-time pickup. | The WARRIOR EATEN event. | | scenes/Coral_39.fsm.txt:829, scenes/Coral_39.fsm.txt:848 |
| Coral_39.5 | Music Region (1) | FSM Activate da6a4adc4c | Music region on hero in position | no | Audio. | | | scenes/Coral_39.fsm.txt:863 |

## Coral_40

no new elements

## Coral_41

no new elements

## Coral_42

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_42.1 | Audio Loop CrowdVoice | FSM Control 598eab1a57 | Starts and stops crowd audio | no | Audio. | | | scenes/Coral_42.fsm.txt:53 |
| Coral_42.2 | Rosary Thief Crowd | FSM Randomise Crowd dd839420b3 | Activates 2 to 4 random thieves | no | Random cosmetic crowd. | | | scenes/Coral_42.fsm.txt:715 |
| Coral_42.3 | Rosary Thief Crowd ×7 | FSM Control 2da5f1c318 | Thief hides or emerges by hero range | no | Local reaction to hero range, cosmetic. | | | scenes/Coral_42.fsm.txt:731 |
| Coral_42.4 | Thief NPC Shop | FSM Conversation d0dae8b01a | Grindle shop NPC, intro dialogue | maybe | Met flags (metGrindleAct3) and shop stock purchases are per save. | metGrindleAct3 and limited stock bought. | Shared stock or per-player stock. | scenes/Coral_42.fsm.txt:3042 |

## Coral_43

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_43.1 | Music Control - Garmond Tension | FSM FSM 9c8e412789 | Tension music while Garmond quest runs | no | Audio. | | | scenes/Coral_43.fsm.txt:4 |

## Coral_44

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_44.1 | Secret Mask New Sharp Ultra | FSM FSM 90e1f6ee74 | Secret wall mask fades when broken | maybe | A persistent break reveals the room only in the game that broke it. | The break event. | | scenes/Coral_44.fsm.txt:703 |

## Coral_Judge_Arena

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_Judge_Arena.1 | Big Battle Gate | FSM Open 70f211d9b2 | Black-thread gate opens on event | maybe | Act 3 variant only; opens on BLACK THREAD GATE OPEN. | That event if Act 3 is played together. | | scenes/Coral_Judge_Arena.fsm.txt:4 |
| Coral_Judge_Arena.2 | Boss Scene | FSM Control a2db30ebb9 | Last Judge fight, gate opens, ending cinematic | sure | Arena start sets encounteredLastJudge, defeat sets defeatedLastJudge and SeenLastJudgeGateOpen, and the gate cinematic takes control and changes scene for one player only. | Battle start, boss defeat, gate-open event, cinematic start and end. | Pull the partner into the arena, or the cinematic, freeze them, or leave them free. | scenes/Coral_Judge_Arena.fsm.txt:24 |
| Coral_Judge_Arena.3 | Boss Scene | FSM Needolin Scene b376a7117c | Locks hero into needolin gate scene | maybe | Locks only the local hero and disables inventory while the gate pilgrims sing. | Result only (see 20 on bells). | | scenes/Coral_Judge_Arena.fsm.txt:277 |
| Coral_Judge_Arena.4 | Gate | FSM FSM e396a95914 | Snaps gate open on cinematic skip | no | Follows the Boss Scene gate-open state. | | | scenes/Coral_Judge_Arena.fsm.txt:321 |
| Coral_Judge_Arena.5 | Gate Wallslide Shadow | FSM FSM ae6a3e63da | Shadow toggles by hero X | no | Visual from local hero position. | | | scenes/Coral_Judge_Arena.fsm.txt:329 |
| Coral_Judge_Arena.6 | Last Judge | FSM Control 4c6db3b5fe | Boss: spins, dashes, stomps, flame phases | sure | HP phases at 75 and 30 percent, weighted random attacks aimed at the hero, censer slam kills pilgrims, intro roar. | One game decides; share boss state, attack choice, phase, HP, death. | Target nearest player. | scenes/Coral_Judge_Arena.fsm.txt:342 |
| Coral_Judge_Arena.7 | Censer Throw | FSM Control 5edc72431f | Censer aims at judge, chain swings | maybe | Damaging weapon that must match the boss state. | Follows Last Judge state. | | scenes/Coral_Judge_Arena.fsm.txt:1277 |
| Coral_Judge_Arena.8 | Corpse Last Judge(Clone) | FSM Control 7b4709ee3d | Death burn, JUDGE DEFEATED, break apart | sure | JUDGE DEFEATED drives defeat flags and the gate-open sequence. | Boss death. | | scenes/Coral_Judge_Arena.fsm.txt:1286 |
| Coral_Judge_Arena.9 | Swing Attack, Swing Attack Flame | FSM Control cd1928398e | Swing hitbox and flame follow judge | maybe | Attack hitboxes must match the boss state. | Follows Last Judge state. | | scenes/Coral_Judge_Arena.fsm.txt:1434 |
| Coral_Judge_Arena.10 | Gate BG Pilgrim | FSM Control 0cf334379d | Background pilgrim sings, dies on slam | no | Cosmetic NPC. | | | scenes/Coral_Judge_Arena.fsm.txt:1530 |
| Coral_Judge_Arena.11 | Gate NPC | FSM Die 4f66bb9a06 | Gate pilgrim dies on KILL PILGRIMS | no | Cosmetic death event from the boss slam. | | | scenes/Coral_Judge_Arena.fsm.txt:1620 |
| Coral_Judge_Arena.12 | Gate NPC | FSM Dialogue 6461359388 | Gate pilgrim explains shrines, starts quest | maybe | Sets metGatePilgrim, starts the Grand Gate Bellshrines quest and queues the needolin scene per player. | metGatePilgrim and quest start if shared. | Per-player quest or shared quest. | scenes/Coral_Judge_Arena.fsm.txt:1632 |
| Coral_Judge_Arena.13 | Caravan | FSM Set Respawn 62ce6fad05 | Sets death respawn after caravan travel | no | Local hero respawn only. | | | scenes/Coral_Judge_Arena.fsm.txt:1789 |
| Coral_Judge_Arena.14 | Caravan Troupe Leader Judge NPC | FSM Dialogue 5ab00590dd | Flea caravan dialogue, offers travel | sure | Accepting travel relocates the caravan, and the leave flags (MetCaravanTroupeLeaderJudge, CaravanTroupeLeaderCanLeaveJudge) are per save. | The accepted travel and met/can-leave flags. | Caravan moves when either player accepts, or per player. | scenes/Coral_Judge_Arena.fsm.txt:2518 |
| Coral_Judge_Arena.15 | door2 | FSM FSM 74c06c3568 | Caravan door open and close on entry | no | Visual. | | | scenes/Coral_Judge_Arena.fsm.txt:2655 |
| Coral_Judge_Arena.16 | Music Fader L, Music Fader R | FSM FSM e4de7bfc1d | Silences music from side gates | no | Audio. | | | scenes/Coral_Judge_Arena.fsm.txt:6156 |
| Coral_Judge_Arena.17 | censer ball ×10 | C# FadeOutByAngle, AnimateRigidBody2DProperties | Hanging censer sway and fade | no | Visual. | | | scenes/Coral_Judge_Arena.objects.txt:537 |
| Coral_Judge_Arena.18 | Cinematic Player ×2 | C# ColorCorrectionCurvesAnimator | Cinematic color grading | no | Visual. | | | scenes/Coral_Judge_Arena.objects.txt:580 |
| Coral_Judge_Arena.19 | Thread Sprite ×3 | C# RandomSprite | Random sprite on pilgrim death | no | Visual. | | | scenes/Coral_Judge_Arena.objects.txt:621 |
| Coral_Judge_Arena.20 | Gate Bells | C# BellShrineGateLock | Bells ring; all-unlocked opens the Great Gate | sure | The gate opens only if the playing hero has every bell-shrine bool, so a partner missing one cannot open it, and opening is a world event for both. | The gate-open result (GATE LOCKS COMPLETE / SeenLastJudgeGateOpen). | Needolin player's shrines must all be rung, or both players' shrines count together. | scenes/Coral_Judge_Arena.objects.txt:670 |
| Coral_Judge_Arena.21 | SC_0060_sc_floor_01 ×53 | C# SmashableTile | Floor tiles smash when stomped by boss | maybe | Random smashChance picks which persistent tiles break, so the floor differs per game. | The smash event with chosen tiles. | | scenes/Coral_Judge_Arena.objects.txt:730 |
| Coral_Judge_Arena.22 | Censer Throw | C# ContinuousTrigger | Repeating trigger on thrown censer | no | Follows the censer. | | | scenes/Coral_Judge_Arena.objects.txt:809 |
| Coral_Judge_Arena.23 | Chain | C# RigidBody2DGroupResetter | Resets censer chain physics | no | Visual. | | | scenes/Coral_Judge_Arena.objects.txt:815 |
| Coral_Judge_Arena.24 | Swing Flame | C# ResetAnimatorOnEnable | Restarts flame animation on enable | no | Visual. | | | scenes/Coral_Judge_Arena.objects.txt:827 |
| Coral_Judge_Arena.25 | Act Card Title ×9 | C# GetTMProLeftVertex | Aligns act card text | no | UI layout. | | | scenes/Coral_Judge_Arena.objects.txt:974 |

## Coral_Tower_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Coral_Tower_01.1 | Memory Group/before | FSM activate memory on tool pickup b0e99a46d5 | Allows memory entry after needolin memory powerup | maybe | Gate depends on hasNeedolinMemoryPowerup, a per-save flag, and the memory is a separate scene. | hasNeedolinMemoryPowerup if shared. | Partner may enter the memory together or only the player who owns the powerup. | scenes/Coral_Tower_01.fsm.txt:14 |

## Weave_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Weave_02.1 | Audio Player Eva Sing | FSM FSM a19aff8d7a | Fades singing audio in | no | Audio. | | | scenes/Weave_02.fsm.txt:4 |
| Weave_02.2 | crest_shrine_break_tube ×4 | FSM Control 898c1c5964 | Breaks after nail hits, sets a PD bool | sure | A broken tube sets its PlayerData bool and persists, while the partner's tube stays whole. | The final tube break. | | scenes/Weave_02.fsm.txt:11 |
| Weave_02.3 | Initial Servitor Scene | FSM Control 0bc18a9218 | Trigger wakes Weaver Servitors | sure | Wake fires when one hero enters the trigger, leaving servitors dormant for the other player. | The wake trigger. | | scenes/Weave_02.fsm.txt:108 |
| Weave_02.4 | loom_break_lamp_weave Mid Chunk ×52 | FSM FSM 6ec4b2f20d | Debris chunk spins | no | Visual. | | | scenes/Weave_02.fsm.txt:115 |
| Weave_02.5 | Song_Gate_set | FSM Control 23f87fd563 | Needolin opens song gate halves | sure | Only the playing player's needolin starts and opens it (OPEN event), so the gate opens in one game only. | The OPEN event. | | scenes/Weave_02.fsm.txt:619 |
| Weave_02.6 | Weaver Sign | FSM Control a838870d26 | Sign lights when hero touches | no | Visual with a harmless persistent flag. | | | scenes/Weave_02.fsm.txt:705 |
| Weave_02.7 | weaver_lift_top, weaver_lift_mid | C# WeaverLift | Charges, then teleports hero to paired lift | maybe | Moves only the local hero with control relinquished; unlock is persistent (weaver_lift_power_chamber). | The teleport as a jump; the unlock flag. | Freeze partner during teleport or leave them free. | scenes/Weave_02.objects.txt:146 |
| Weave_02.8 | weaver_lift_top glows ×6 | C# AnimatedFadeGroup | Glow fades | no | Visual. | | | scenes/Weave_02.objects.txt:148 |

## Weave_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Weave_03.1 | Battle End | FSM Control cc8bef5513 | Journal kill, opens gates, music off | sure | On BATTLE END opens the gates and awards the journal kill only in the game where the battle ended. | Battle end. | | scenes/Weave_03.fsm.txt:1927 |
| Weave_03.2 | Mossbone Mother Ambient Corpse(Clone) ×2 | FSM Death 8e3afb2f15 | Boss death blast, chunks, camera shake | sure | Drops a persistent instant pickup and sets the boss dead for the scene; the black-thread variant plays a different path. | Boss death. | | scenes/Weave_03.fsm.txt:2668 |
| Weave_03.3 | Stals, Stals B | FSM FSM 43d904182c | Random triple or quad stalactite fall | sure | RNG picks which stalactite is omitted, and they damage the hero. | The FALL pattern (which one is omitted). | | scenes/Weave_03.fsm.txt:4245 |
| Weave_03.4 | Tiny Dragonfly ×62 | FSM Control db3685bfd1, FSM 1e402493db | Ambient dragonflies drift, react to needolin | no | Cosmetic critters. | | | scenes/Weave_03.fsm.txt:6149, scenes/Weave_03.fsm.txt:6923 |

## Weave_04

no new elements

## Weave_05b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Weave_05b.1 | Bone Goomba Bounce Fly ×16 | FSM Control 4b65e7e933 | Flyer, dies, refills and flies back in | sure | Has HP, dies and respawns on hazard reload of one hero. | Enemy state, HP, death and respawn. | | scenes/Weave_05b.fsm.txt:554 |
| Weave_05b.2 | Lava_Waterfall Set ×2 | FSM Control a4da08542f | Cycles lava hazards on timers | no | Fixed timers per game; each game damages only its own hero, so phase offset is harmless. | | | scenes/Weave_05b.fsm.txt:1064 |

## Weave_07

no new elements

## Weave_08

no new elements

## Weave_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Weave_10.1 | Crest Upgrade Shrine | FSM Dialogue 12b07b1b13 | Crest NPC: unlock crests and slots | maybe | Long control-locked dialogue that writes crest unlocks and extra slots from the player's own crest points. | Crest and slot unlock flags if shared. | Per-player upgrades or both get the upgrade when either does. | scenes/Weave_10.fsm.txt:4 |
| Weave_10.2 | Approach Singing Audio, Breathing Loop Audio | FSM Tubes Reaction afdb52030d, 20270113e5 | Volume by active tubes | no | Audio. | | | scenes/Weave_10.fsm.txt:515, scenes/Weave_10.fsm.txt:545 |
| Weave_10.3 | crest_shrine_break_tube parent | FSM Record Active Tubes 3bfa543cb7 | Counts intact tubes | no | Follows the tube break events (Weave_02.2). | | | scenes/Weave_10.fsm.txt:569 |
| Weave_10.4 | Parasite_Shield hit response | FSM Extra Tink Audio a72a149de8 | Tink sound | no | Audio. | | | scenes/Weave_10.fsm.txt:873 |
| Weave_10.5 | Crest Upgrade Shrine | C# NpcAnimatorParams | NPC animator flags for dialogue | no | Visual. | | | scenes/Weave_10.objects.txt:730 |

## Weave_11

no new elements

## Weave_12

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Weave_12.1 | weaver_harp_sign_map | FSM Control 979100edaf | Lights on power-chamber unlock; map taken | maybe | Reads weaver_lift_power_chamber and the MAP TAKE event, both per save. | weaver_lift_power_chamber unlock so lifts match. | | scenes/Weave_12.fsm.txt:4 |

## Weave_13

no new elements

## Weave_14

no new elements

## Summary

7.1. Events that should pull the partner to the player who triggered them (all start from one hero touching a trigger and then lock gates, stun or take control):

- Coral_11 Driller duo arena (Coral_11.3, 11.7, 11.8): Start Region closes the gates and the intro roar stuns the hero.
- Coral_27 Giant Driller battle (Coral_27.7): BattleScene locks only the entering hero and the roar stuns.
- Coral_29 Zap Core boss (Coral_29.4): wakes on one hero's range, roar stun, arena opens on death.
- Coral_33 Garmond (Coral_33.1): roar clamps hero X to 53 and the battle gate closes.
- Coral_Judge_Arena Last Judge and gate cinematic (Coral_Judge_Arena.2, .6, .20): defeat flags, scene change and cinematic take control of one hero only.
- Weave_03 Mossbone Mother (Weave_03.1, .2): BattleScene end opens gates only where it ended.
- Coral_19 Lace meet (Coral_19.3): forced cutscene, hero control taken, saves; also Coral_19.4 world-edge dialogue as a lighter case.
- Coral_10 Citadel Quest Starter (Coral_10.2): first entry starts Act 2, quest and autosave.

7.2. RNG and hero-targeting that diverges unless one game decides:

- Boss attack choice: Coral_11.2, Coral_27.1, Coral_29.4, Weave_03.3 (stalactite pattern), Coral_Judge_Arena.6.
- Scene-level random spawn: Coral_38.1 (Pattern A or B via ActivateRandomChild), Coral_27.1 (GetRandomChild charge order).
- Random persistent tile break: Coral_Judge_Arena.21.

7.3. Shared-world events to keep in step (one trigger, then each game replays):

- Coral_02.8 shortcut platform, Coral_23.6 tree activator, Coral_35b.1 tower collapse, Weave_02.5 song gate, Weave_02.3 servitor wake, Weave_02.2 tube break.
- Coral_Judge_Arena.20: the Great Gate opens only if the needolin player has every bell-shrine bool; open question whether shrines of both players count together.

7.4. Per-save progression where the user must pick shared vs per-player:

- Dice game bank and reward (Coral_33.5), Steel Sentinel quest (Coral_37.2), Grindle shop (Coral_42.4), Crest Upgrade Shrine (Weave_10.1), Caravan travel (Coral_Judge_Arena.14), Gate Pilgrim quest (Coral_Judge_Arena.12), Sherma meet (Coral_10.5), Zaza follow target (Coral_35.1).

7.5. Gaps and cautions:

- Act 3 (blackThreadWorld) variants exist in Coral_02, 03, 11, 19, 23, 33, 42 and Coral_Judge_Arena; the flag must match on both sides or the partner sees a different cast.
- StartRoarEmitter with stunHero=True stuns the local hero wherever it runs (Coral_11.7, Coral_27.7, Coral_29.4, Coral_33.1, Coral_Judge_Arena.6): a far-away partner is stunned too if the action is replayed.
- CheckHeroPerformanceRegion based reactions (Coral_11.4 needolin timer, enemy sing responses everywhere in this slice) read only the local hero's needolin, so the partner's needolin never affects them.
- I could not confirm from the dump whether ZapGrassCluster (Coral_29.8) deals damage; marked `no` on that basis.
- New notes written: RangeAttacker, TimedActivator, DropFloorBreaker, SmashableTile, BellShrineGateLock, WeaverLift, ZapGrassCluster in audit/notes/.
