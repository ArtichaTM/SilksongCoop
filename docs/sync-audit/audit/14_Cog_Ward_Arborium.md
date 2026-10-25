# Audit 2.14: Cog, Ward, Arborium (Cog_04 .. Cog_Pass, Ward_01 .. Ward_09, Arborium_01 .. Arborium_Tube)

Slice: catalog/Cog.md (Cogwork Core), catalog/Ward.md (Whiteward), catalog/Arborium.md (Memorium). Verdict means: does coop need to handle it. Enemy rows are `sure` when alert, chase, target or attack choice reads only the local hero or uses a random roll; where a row says "one game decides", the other game replays enemy state, HP and death. Follower FSMs (walk RNG, z-depth, audio, jitter) are `maybe` or `no`: they only matter if the enemy itself is not driven by one game. New shared notes in `audit/notes/`: CogEnergyTimeline, CogCylinderPuzzle, HarpoonRingSlider, HarpoonRingSlideLock, PersistentFolderReset, LoopMovementTrack, CogPlatController.

## Cog_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_04.1 | Song Automaton 01 ×N | FSM Control 1e5a0332ed | Walker automaton: alerts, multi-slash combo, sings | sure | Alert range, facing and attack start read the local hero; sing reacts to local needolin. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Cog_04.fsm.txt:1301 |
| Cog_04.2 | Song Automaton 02 ×2 | FSM Control 24a56c0b69 | Steam automaton: alerts, steam-blast attack, sings | sure | Alert, facing and steam attack target the local hero; random flip on init. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Cog_04.fsm.txt:214 |
| Cog_04.3 | Song Automaton 01/02, Song Automaton Goomba | FSM Walk 5d17842050, b83966827a | Random step pattern for automaton walking | maybe | Step count, speed and turn are random per game, so positions drift unless one game drives the enemy. | Covered by the enemy state of the owning automaton. | | scenes/Cog_04.fsm.txt:100, scenes/Cog_04.fsm.txt:3697 |
| Cog_04.4 | Song Automaton 02 | FSM Steam Dmg Effects 4ce25db85f | Spawns steam death effect on hero | no | Cosmetic effect at the local hero. | | | scenes/Cog_04.fsm.txt:323 |
| Cog_04.5 | Steam Hit ×N | FSM multi_wounder 0701353768 | Multi-hit damager: checks parry, damages local hero | maybe | Damages only the local hero, which is correct per game; fine as long as the attack itself plays in both games. | | | scenes/Cog_04.fsm.txt:330 |
| Cog_04.6 | Song Automaton Goomba ×2 | FSM Control 1f4637533b | Spike goomba: walks, spikes up when hero near | sure | Spike attack fires on the local hero's alert range; random flip on init. | One game decides target and attack; share enemy state, HP and death. | | scenes/Cog_04.fsm.txt:3615 |
| Cog_04.7 | cog_lever ×2 | FSM Control 582924e672 | Lever hit sets PD bool, opens mechanism | sure | The PD bool and the open mechanism change only in the hitting game; the partner's gate stays shut. | The lever hit; each game then sets the bool and plays the opening. | | scenes/Cog_04.fsm.txt:2073 |
| Cog_04.8 | cog_lever/Handle ×2 | FSM Detect Hit 186970a623 | Detects nail hit in range, sends LEVER HIT | sure | Part of the lever: only the local hero's hit registers. | Same event as Cog_04.7 (the lever hit). | | scenes/Cog_04.fsm.txt:2283 |
| Cog_04.9 | Cogwork Energy Gate ×2 | FSM Control d31b8eb10d | Gate rises with energy, crushes hero | sure | Driven by the lever ACTIVATE; without it the partner's gate stays closed and may block their path. | Nothing extra; follows the lever hit in Cog_04.7. | | scenes/Cog_04.fsm.txt:2315 |
| Cog_04.10 | Cogwork Energy Gate ×2 | FSM FSM 515ce18ec5 | Gate audio for rise and retract | no | Audio only. | | | scenes/Cog_04.fsm.txt:2364 |
| Cog_04.11 | cog_plat_spin ×16 | C# LoopMovementTrack | Moves spinner platforms on a looping track | maybe | Phase comes from Time.time, so the two games show the platforms at different spots and the partner seems to stand on air. | Phase offset (time base) of the track, once per scene. | Accept the visual mismatch, or align the time base. | scenes/Cog_04.objects.txt:1285 |
| Cog_04.12 | Move Audio ×30 | C# AudioSourceMovingVolume | Audio volume follows moving object | no | Audio only. | | | scenes/Cog_04.objects.txt:2982 |

## Cog_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_05.1 | Plat Mid, Plat Mid L, Plat Mid R ×10 | FSM Control 9cb736efa9 | Arena platform moves up and down on command | maybe | Driven by the wave setup FSMs; matches if the battle waves match. | Nothing extra; follows the wave start. | | scenes/Cog_05.fsm.txt:77 |
| Cog_05.2 | Wave Spawner (Aftermath) | FSM Control 78b2f08a97 | On enter, activates a random aftermath wave | sure | Trigger and wave choice are local and random, so each game spawns a different wave or only one game spawns it. | The chosen wave and the spawn moment. | | scenes/Cog_05.fsm.txt:242 |
| Cog_05.3 | Automaton_trapdoor_spawner ×18 | FSM Control 6e3b31f8c9 | Trapdoor drops an automaton into the battle | sure | Spawn timing has a random pause and adds the enemy to the local battle count. | Nothing extra if the wave start is shared; the spawned enemy then needs enemy state sync. | | scenes/Cog_05.fsm.txt:251 |
| Cog_05.4 | Song Automaton Shield ×6 | FSM Control 9a611338cd | Shield automaton: blocks, overhead and rapid stabs | sure | Alert, above-check, block facing and attack choice read the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Cog_05.fsm.txt:4322 |
| Cog_05.5 | Song Automaton Shield ×6 | FSM Automaton Walk e738444e24 | Random step pattern for shield automaton | maybe | Random per game; covered once one game drives the enemy. | Covered by Cog_05.4 enemy state. | | scenes/Cog_05.fsm.txt:4570 |
| Cog_05.6 | Song Automaton Shield ×6 | FSM Friend Bonk fab95727f2 | Recoils from bonking a fellow automaton | no | Follows enemy-to-enemy contact; replays from enemy state. | | | scenes/Cog_05.fsm.txt:4660 |
| Cog_05.7 | Battle Gate Cog ×2 | FSM BG Control 705e41e980 | Battle gate closes and opens | sure | Gates close only in the game whose hero started the battle; the partner can walk through or get locked out. | The battle start and battle end; gates follow. | When a battle starts, pull the partner into the arena or leave them outside. | scenes/Cog_05.fsm.txt:5719 |
| Cog_05.8 | Setup Wave 1..6 | FSM Control 2260e34df3, 27914a9866, 0029cb5513, ce198ed7ea, f2c48c4b10, 665baf97d3 | Per wave: moves platforms, spawns, sets hazard respawn | sure | Wave progress follows the local enemy count; wave 2 also picks a spawn side from the local hero's position. | The wave start (wave index) from one game. | | scenes/Cog_05.fsm.txt:6292, scenes/Cog_05.fsm.txt:7126, scenes/Cog_05.fsm.txt:7939, scenes/Cog_05.fsm.txt:8829, scenes/Cog_05.fsm.txt:9733, scenes/Cog_05.fsm.txt:10600 |
| Cog_05.9 | Song Automaton Fly Spike ×9 | FSM Control b99e0b884c | Flying spiker: patrols, spikes out, faces hero | sure | Facing and sing react to the local hero; patrol destination is picked per game. | One game decides; share enemy state, HP and death. | | scenes/Cog_05.fsm.txt:7621 |
| Cog_05.10 | Song Automaton Fly Spike BG ×3 | FSM Control 0393c44ef7 | Background spiker patrol, faces hero | no | Background decoration that only turns toward the local hero. | | | scenes/Cog_05.fsm.txt:11615 |
| Cog_05.11 | cog_plat_solo | C# CogPlatController, CogPlat, CogPlatArm | Timed rotating platform drops and rises | maybe | Tick timers start at scene load, so the drop timing differs per game. | Tick phase once per scene. | Accept the mismatch, or align the ticker. | scenes/Cog_05.objects.txt:1801 |

## Cog_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_06.1 | cog_door_open | FSM Audio b4df85c3a5 | Door rise and retract audio | no | Audio only. | | | scenes/Cog_06.fsm.txt:32 |

## Cog_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_07.1 | ball_wall L, ball_wall R | FSM Control 73671bb536 | Wall opens and releases the ball automaton | sure | Part of the battle intro; runs only in the game whose battle started. | The battle start. | | scenes/Cog_07.fsm.txt:4 |
| Cog_07.2 | Intro Control | FSM Control 73491e31ae | Battle intro: rumble, camera locks, battle music | sure | Camera lock and intro happen only in the triggering game. | The battle start. | When a battle starts, pull the partner into the arena or leave them outside. | scenes/Cog_07.fsm.txt:174 |
| Cog_07.3 | Song Automaton Ball L, R | FSM Control ff3558dcec | Rolling ball automaton: launches at hero, bounces, mines | sure | Launch angle is random and facing targets the local hero. | One game decides; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Cog_07.fsm.txt:217 |
| Cog_07.4 | Song Automaton Ball L, R | FSM Place Key Item On Death f27108ad09 | On death moves item placer to corpse | sure | Item ends up where the enemy died in the local game. | Covered by the enemy death sync. | | scenes/Cog_07.fsm.txt:202 |
| Cog_07.5 | Song Automaton Ball L, R | FSM Z Pos fe6b4ec343 | Sets z-depth from height | no | Render depth only. | | | scenes/Cog_07.fsm.txt:209 |
| Cog_07.6 | Item Placer | FSM Control 44f0d64956 | After battle start, activates the key item pickup | sure | A collectable pickup appears only in the game whose battle ran; both players may grab it. | The pickup taken. | Item for both players, or only the one who grabbed it. | scenes/Cog_07.fsm.txt:886 |
| Cog_07.7 | Repairable Scene | FSM Control f03a273556 | Chooses automaton state from cog7 PD bools | sure | Reads cog7_automatonDestroyed and Repairing and writes RepairingComplete; games disagree if PD differs. | The PD bools cog7_automatonDestroyed and cog7_automatonRepairing. | | scenes/Cog_07.fsm.txt:897 |
| Cog_07.8 | Song Automaton Ball Inert | FSM Control 8fee81a14e | Inert ball: tinks on hit, smashed sets destroyed PD | sure | The smash writes cog7_automatonDestroyed only in the smashing game. | The smash (COG SMASH) of the inert ball. | | scenes/Cog_07.fsm.txt:960 |
| Cog_07.9 | Song Automaton Ball Inert | FSM Push Msg d5549b81f3, Work Jitter aa6d7e9812, Bounce 369c4a0fa1 | Push, jitter and bounce reactions | no | Local physical and visual reactions. | | | scenes/Cog_07.fsm.txt:953, scenes/Cog_07.fsm.txt:1011, scenes/Cog_07.fsm.txt:1022 |
| Cog_07.10 | Enemy Damager | FSM Control 8d47047ab3 | Toggles an enemy damager | no | Follows the ball state. | | | scenes/Cog_07.fsm.txt:1035 |
| Cog_07.11 | Wheel Audio ×21 | FSM Control 4bcc24c2c1 | Wheel rolling audio | no | Audio only. | | | scenes/Cog_07.fsm.txt:1047 |
| Cog_07.12 | cog_lever CameraLockArea (8) | FSM FSM 4d8e6c6539 | Disables camera lock after lever opens | no | Local camera; follows the lever hit. | | | scenes/Cog_07.fsm.txt:3222 |
| Cog_07.13 | steam_vent_single ×16 | FSM Control 81241aa276 | Timed steam vent hazard | no | Hazard on its own timer; damages whoever stands in it per game. | | | scenes/Cog_07.fsm.txt:3725 |
| Cog_07.14 | Pusher ×2 | FSM FSM 4d87a36865 | Steam pushes objects in range | no | Pushes the local hero only; correct per game. | | | scenes/Cog_07.fsm.txt:3894 |

## Cog_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_08.1 | Harpoon Ring Rail Slider ×9 | FSM Rail Control 0c40fb76ed | Ring slides along rail carrying hero | maybe | The ring moves only in the riding game; the partner's copy flies while their own ring stays put, and both cannot share one ring. | Ring grab and release, so the other game moves its ring too. | Let both ride the same ring, or treat the ring as local. | scenes/Cog_08.fsm.txt:268 |

## Cog_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_09.1 | City_spot_lamp_wheeled_interactive ×2 | FSM Tink Response bfa9d4971d | Wheeled lamp rolls on hit, can break | maybe | Hit direction comes from the local hero; the lamp ends in a different spot or breaks only in one game. | The hit (direction) and the break. | | scenes/Cog_09.fsm.txt:28 |
| Cog_09.2 | Wheel ×64 | C# ChildWheelAnimation | Spins wheels while object moves | no | Visual only. | | | scenes/Cog_09.objects.txt:2175 |
| Cog_09.3 | puzzle cylinders | FSM Cylinder States 826ebb6ccd | Puzzle complete: plate, needolin melody, locks hero | sure | Locks the hero, disables inventory and plays the Architect melody sequence only in the game that finished the puzzle. | Puzzle completion and the melody sequence start. | Run the melody cutscene for both (pull partner in) or only for the one who played it. | scenes/Cog_09.fsm.txt:196 |
| Cog_09.4 | puzzle cylinders | FSM Prompt Control a9ecb54199 | Needolin prompt; toggles pause and inventory | maybe | Sets disablePause and disableInventory on the local hero only; harmless if the sequence runs per game. | | | scenes/Cog_09.fsm.txt:369 |
| Cog_09.5 | puzzle cog lever ×16 | FSM Control 8b71cd400c | Cog lever: hit rotates its cog | sure | Each hit turns one cog in the hitting game only; the puzzle state diverges and completion fires in one game. | Each cog lever hit. | | scenes/Cog_09.fsm.txt:391 |
| Cog_09.6 | puzzle cog lever/Handle ×16 | FSM Detect Hit 89dcfcfe67 | Detects nail hit, sends LEVER HIT | sure | Same as Cog_09.5. | Same event as Cog_09.5. | | scenes/Cog_09.fsm.txt:418 |
| Cog_09.7 | Cog_Choir_Cylinder ×4, cylinder puzzle cog ×16 | C# CogCylinderPuzzle, CogCylinderPuzzleCog | Cylinder done when all cogs aligned; saved bool | sure | Completion writes a scene bool only in the game where the last cog aligned; cog positions are not saved. | Nothing extra if every cog lever hit is shared (Cog_09.5). | | scenes/Cog_09.objects.txt:366, scenes/Cog_09.objects.txt:520 |

## Cog_09_Destroyed

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_09_Destroyed.1 | City_spot_lamp_wheeled_interactive (1) | FSM Tink Response Custom 2081739780 | Wheeled lamp tinks and breaks on hit | maybe | Same as Cog_09.1. | The hit and the break. | | scenes/Cog_09_Destroyed.fsm.txt:22 |
| Cog_09_Destroyed.2 | Weakness Cog Drop Scene | FSM Control 99bc8d3f4b | Hero lands weakened, fades, walks slowly | sure | Takes over the local hero (invincible, weak walk, fade) only for the hero who fell in; the partner sees the other Hornet acting oddly. | The scene start, so the partner's game knows the other is in a forced sequence. | Leave the partner free (recommended), or freeze them until it ends. | scenes/Cog_09_Destroyed.fsm.txt:241 |

## Cog_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_10.1 | Song Knight Sleeping | FSM Control b288ce4372 | Uses 3 Cog Heart Pieces, wakes knight, dialogue | sure | Consumes items and sets wokeSongChevalier only in the game that used them; the tube stays sealed for the partner. | The heart use (YES on the prompt), then each game plays the wake. | Who pays the 3 heart pieces; pull the partner into the dialogue or leave them free. | scenes/Cog_10.fsm.txt:301 |
| Cog_10.2 | Cracks | FSM Control 68f6424e9c | Tube crack visuals per hit stage | no | Visual; follows the tube Breakable. | | | scenes/Cog_10.fsm.txt:430 |

## Cog_10_Destroyed

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_10_Destroyed.1 | Audio Abyss Tension | FSM Control 9e0c0f7e22 | Tension audio by hero position | no | Audio only. | | | scenes/Cog_10_Destroyed.fsm.txt:4 |
| Cog_10_Destroyed.2 | Roof Blocker | FSM FSM e58534b086 | Roof collider on with battle gates | sure | Follows battle gates; closes only in the battle game, so the partner's roof stays open. | The battle start and end. | | scenes/Cog_10_Destroyed.fsm.txt:250 |
| Cog_10_Destroyed.3 | Wake Scene | FSM Control 877a3f26b3 | Trigger starts black-thread wake battle, quake | sure | Triggered by the local hero entering; the battle, camera locks and gates run in one game only. | The battle start. | When a battle starts, pull the partner into the arena or leave them outside. | scenes/Cog_10_Destroyed.fsm.txt:258 |
| Cog_10_Destroyed.4 | black_thread_impacter_intro ×5 | FSM Control 01a093e39f | Thread strands impact with random delays | no | Visual intro on random timers; follows the wake. | | | scenes/Cog_10_Destroyed.fsm.txt:303 |
| Cog_10_Destroyed.5 | Understore Poker ×14 | FSM Control fe0f4bd293 | Flyer: wakes, chases, pokes and charges hero | sure | Wake range, DistanceFly, charge aim and pokes target the local hero with random angles. | One game decides; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Cog_10_Destroyed.fsm.txt:518 |
| Cog_10_Destroyed.6 | Understore Small ×7 | FSM Behaviour 197a7933e1 | Walker: attacks, flurries, protects, evades hero | sure | Attack, protect and evade ranges read the local hero; idle and run choice is random. | One game decides; share enemy state, HP and death. | | scenes/Cog_10_Destroyed.fsm.txt:1004 |
| Cog_10_Destroyed.7 | Understore Heavy ×9 | FSM Behaviour 1840c3c36a | Heavy: swipe combos, jump slash, floor break | sure | Attack choice and ranges read the local hero; jump drops can break floors. | One game decides; share enemy state, HP and death. | | scenes/Cog_10_Destroyed.fsm.txt:1368 |
| Cog_10_Destroyed.8 | Understore Thrower ×17 | FSM Control f9f997a6e8 | Flyer: throws, cartwheels at hero | sure | Throw range, distance and aim read the local hero; random angles. | One game decides; share enemy state, HP and death. | | scenes/Cog_10_Destroyed.fsm.txt:1890 |
| Cog_10_Destroyed.9 | Corpse Understore Thrower(Clone) ×14 | FSM FSM 5edfafd4bf | Empty corpse FSM | no | Does nothing. | | | scenes/Cog_10_Destroyed.fsm.txt:2454 |

## Cog_Bench

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_Bench.1 | Audio Rest Music | FSM Leaving Scene 357fb92072 | Starts rest area music | no | Audio only. | | | scenes/Cog_Bench.fsm.txt:4 |
| Cog_Bench.2 | Cog Map Station, Cog Bench Retractor | C# CogEnergyTimeline | Energy timeline; full energy enables the real bench | sure | The bench becomes usable only in the game where the unlock (lever or ring) fired. | The unlock event that calls Open on it. | | scenes/Cog_Bench.objects.txt:206 |
| Cog_Bench.3 | Cog Bench Retractor | C# CaptureAnimationEventMuter | Mutes animation events | no | Audio helper. | | | scenes/Cog_Bench.objects.txt:576 |

## Cog_Dancers

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_Dancers.1 | Boss Scene | FSM Sequence 180941f4a6 | Dancers boss flow, rotation, lift, NPC talks, needolin | sure | Sets encountered/defeatedCogworkDancers, journal, achievement, UnlockedMelodyLift, locks the hero and can transition to Song_Tower_01, all in one game only. | Boss start (gates close), boss defeat (BATTLE END), the melody-lift unlock and the lift ride. | Boss start: pull the partner into the arena or not; lift ride to Song_Tower_01: take both players or only the rider. | scenes/Cog_Dancers.fsm.txt:77 |
| Cog_Dancers.2 | gate ground | FSM OpenClose 73f62b206a | Arena floor gate opens and closes | sure | Follows the boss sequence; the partner's floor stays in the old state. | Nothing extra; follows Cog_Dancers.1. | | scenes/Cog_Dancers.fsm.txt:1059 |
| Cog_Dancers.3 | Atmos Fader | FSM FSM 044a1dadc1 | Audio snapshot change | no | Audio only. | | | scenes/Cog_Dancers.fsm.txt:1361 |
| Cog_Dancers.4 | pendulum_pivot | FSM Control 949a0f06b5 | Pendulum swings on boss beat | no | Visual on the boss beat. | | | scenes/Cog_Dancers.fsm.txt:1365 |
| Cog_Dancers.5 | Green Prince Kneel | FSM Dialogue 2de3fd11be | Green Prince dialogue, moves him to Clover | sure | Writes GreenPrinceLocation in the talking game only; the NPC is gone in one game and stays in the other. | The dialogue end (location change). | Pull the partner into the dialogue or leave them free. | scenes/Cog_Dancers.fsm.txt:1481 |
| Cog_Dancers.6 | cog_ring_shortcut | C# HarpoonRingSlider | Ring slides up while hero hangs | maybe | Moves only in the riding game; partner's ring stays down. | Ring grab and release. | Let both ride, or treat the ring as local. | scenes/Cog_Dancers.objects.txt:1061 |
| Cog_Dancers.7 | slide_gate_ring ×5 | C# HarpoonRingSlideLock | Ring drop unlocks gate; saved bool | sure | The drop writes a scene bool and opens unlockables only in the game whose hero hung on it. | The ring drop (completion). | | scenes/Cog_Dancers.objects.txt:1493 |
| Cog_Dancers.8 | cog ×7 | C# VectorCurveAnimatorRamp | Ramps a curve animator | no | Visual only. | | | scenes/Cog_Dancers.objects.txt:1497 |

## Cog_Dancers_boss

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cog_Dancers_boss.1 | Dancer Control | FSM Control bd41b82bee | Boss director: phases, random sequences and positions | sure | Every sequence, pattern and dancer position is a random pick, so two games run different fights. | One game decides; share each chosen sequence and positions, boss HP and phase. | | scenes/Cog_Dancers_boss.fsm.txt:1 |
| Cog_Dancers_boss.2 | Dancer Control | FSM Beat Control c81c5a4aa1 | Sends the beat tick | maybe | Beat timing drives attacks; matches only if the director is shared. | Covered by Cog_Dancers_boss.1. | | scenes/Cog_Dancers_boss.fsm.txt:1859 |
| Cog_Dancers_boss.3 | Dancer Control | FSM Music Control 5202cf7386 | Boss music per phase | no | Follows phase events. | | | scenes/Cog_Dancers_boss.fsm.txt:1871 |
| Cog_Dancers_boss.4 | Audio Loop Tension | FSM FSM 6a147b78ad | Tension audio loop | no | Audio only. | | | scenes/Cog_Dancers_boss.fsm.txt:1915 |
| Cog_Dancers_boss.5 | Dancer A, Dancer B | FSM Control 974a7584ff | Dancer: aims beams, dashes, clashes, roars, stuns | sure | Faces the local hero, random L/R picks, roar stuns the hero; HP and stun local. | One game decides; share dancer state, HP and death. | | scenes/Cog_Dancers_boss.fsm.txt:1926 |
| Cog_Dancers_boss.6 | Dancer A, Dancer B | FSM Check Corner 0bab88b41b, Check Height 99dbfd21f9 | Checks dancer corner and height | no | Helper flags for the dancer; follow its state. | | | scenes/Cog_Dancers_boss.fsm.txt:2937, scenes/Cog_Dancers_boss.fsm.txt:2953 |
| Cog_Dancers_boss.7 | Clasher ×2 | FSM Do Clash 16d044eb8d | Dancers clash: flash and shake | no | Effect when the two dancers meet; follows their state. | | | scenes/Cog_Dancers_boss.fsm.txt:2965 |
| Cog_Dancers_boss.8 | Chunk ×3 | FSM arena impact f163103eb8 | Death chunks fly at random angle | no | Cosmetic debris. | | | scenes/Cog_Dancers_boss.fsm.txt:4030 |
| Cog_Dancers_boss.9 | Pattern 1 ×24 | FSM Control f6d06b9c28 | Shuffles and fires a dive pattern | sure | Attack order is shuffled per game. | Covered by Cog_Dancers_boss.1 (chosen pattern). | | scenes/Cog_Dancers_boss.fsm.txt:4057 |

## Cog_Pass

No new elements.

## Ward_01

No new elements.

## Ward_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ward_02.1 | Pipe_Vent_Hatch | FSM Open At Battle End 19248bcf8c | After boss win, opens hatch near hero | sure | Reads wardBossDefeated and writes wardBossHatchOpened in the game whose hero walks near. | The hatch open (wardBossHatchOpened). | | scenes/Ward_02.fsm.txt:4 |
| Ward_02.2 | Respawn Scene | FSM Control a611735c79 | Random husk respawn choice on certain entries | sure | Range check and the random BOTH / 2 SLAMMER / 2 SLASHER pick are per game. | The chosen spawn set and the spawn moment. | | scenes/Ward_02.fsm.txt:22 |
| Ward_02.3 | Slammer 1, Slammer 2 | FSM Control 5988116eae | Husk slammer: chases, slams at hero | sure | Alert, chase and slam aim read the local hero; random speeds. | One game decides; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Ward_02.fsm.txt:72 |
| Ward_02.4 | Slasher 1, Slasher 2 | FSM Control 6f0a8db522 | Husk slasher: flies at hero, multi-hit slashes | sure | Alert, fly-to and slash angle read the local hero. | One game decides; share enemy state, HP and death. | | scenes/Ward_02.fsm.txt:610 |
| Ward_02.5 | Slasher 1, Slasher 2 | FSM Wobble 8f12fe9bda | Random idle wobble | no | Cosmetic. | | | scenes/Ward_02.fsm.txt:878 |
| Ward_02.6 | conductor_web_floor ×9 | FSM Control be27ecf1e9 | Web floor jitters when stood on | no | Cosmetic. | | | scenes/Ward_02.fsm.txt:1721 |
| Ward_02.7 | Ward Chair ×7 | FSM Hit Response 59d13aefff, Pivot Control 4fcda72a36 | Chair wobbles on hit, can tip over | no | Prop reaction; hit direction from local hero. | | | scenes/Ward_02.fsm.txt:1865, scenes/Ward_02.fsm.txt:1913 |

## Ward_02_boss

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ward_02_boss.1 | Boss Scene | FSM Control b2fee22bf6 | Conductor boss flow: phases, husk waves, silk heart | sure | Start trigger, phase progression, wardBossEncountered/Defeated and the silk heart run in one game only. | Boss start, each phase change and the boss defeat. | Boss start: pull the partner into the arena or not; silk heart: give to both or only the collector. | scenes/Ward_02_boss.fsm.txt:1 |
| Ward_02_boss.2 | Attack Spears | FSM Store Last Spear X 68a0a84733 | Stores last spear x | no | Helper variable. | | | scenes/Ward_02_boss.fsm.txt:227 |
| Ward_02_boss.3 | Ward Boss Spear ×5 | FSM Control 71b5f5fd19 | Spear aims at hero, drops and pierces | sure | Aim and attack x come from the local hero plus random offsets. | Covered by the boss decision (spear target). | | scenes/Ward_02_boss.fsm.txt:230 |
| Ward_02_boss.4 | Conductor Boss | FSM Control 57ab547dc6 | Boss: teleports, shoots, charges, spear barrages | sure | Attack choice is random and aimed at the local hero. | One game decides; share boss state, HP and phase. | | scenes/Ward_02_boss.fsm.txt:730 |
| Ward_02_boss.5 | Conductor Boss | FSM Custom Stun Control 75899960cc | Stun per phase, death trigger | sure | Stun and death follow local damage. | Covered by boss HP and phase. | | scenes/Ward_02_boss.fsm.txt:1238 |
| Ward_02_boss.6 | Conductor Boss | FSM Head Invuln Check 53cfa88023 | Head invulnerable while hero in range | maybe | Reads the local hero range; each game may disagree on hits. | | | scenes/Ward_02_boss.fsm.txt:1273 |
| Ward_02_boss.7 | Ward Corpse Projectile 1 ×5 | FSM Control cec6dc92e8 | Thrown corpse launches, lands, takes heavy damage | maybe | Launched by the boss; matches once the boss attack is shared. | Covered by the boss decision. | | scenes/Ward_02_boss.fsm.txt:1285 |
| Ward_02_boss.8 | Slasher 1 | FSM Control 44bd2b72d5 | Boss-wave husk slasher | sure | Same as Ward_02.4 but spawned by the boss. | One game decides; share enemy state, HP and death. | | scenes/Ward_02_boss.fsm.txt:2168 |
| Ward_02_boss.9 | Ward Boss Suckspear ×4 | FSM Control 95e835859e | Spears sucked into boss head | no | Phase visual; follows the boss. | | | scenes/Ward_02_boss.fsm.txt:2944 |

## Ward_02b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ward_02b.1 | Husk Item Ambush | FSM Control 2a4737f2cf | Item pickup then range trigger: threaded husk ambush | sure | The ambush fires only in the game whose hero took the item and entered the range. | The item pickup and the ambush start. | Item for both or only the taker. | scenes/Ward_02b.fsm.txt:584 |
| Ward_02b.2 | Husk Dummy | FSM Dummy Wobble 3ea0910a64, Custom Lowpass 6d45e51965 | Dummy wobble and voice lowpass | no | Cosmetic and audio. | | | scenes/Ward_02b.fsm.txt:631, scenes/Ward_02b.fsm.txt:764 |
| Ward_02b.3 | Ward Bed ×7 | FSM Hit Response abd050c591 | Bed wobbles on hit | no | Prop reaction. | | | scenes/Ward_02b.fsm.txt:1550 |
| Ward_02b.4 | corpse | FSM Fling Corpse 003db561ae | Bed hit flings corpse, drops item pickup | sure | The item pickup drops only in the game that hit the bed. | The fling (bed hit). | Item for both or only the taker. | scenes/Ward_02b.fsm.txt:1574 |

## Ward_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ward_03.1 | Song Creeper ×3 | FSM Control 6bcfcd2bbb | Creeper: wakes, blocks, slash combos, leaps at hero | sure | Block, slash and patrol ranges and leap aim read the local hero. | One game decides; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Ward_03.fsm.txt:745 |
| Ward_03.2 | Song Creeper Wake | FSM Control e88bf21238 | Sends soft wake to creepers once | maybe | Fires once from a local trigger; partner's creepers stay asleep. | The wake event. | | scenes/Ward_03.fsm.txt:2056 |
| Ward_03.3 | Song Pilgrim 02 ×8 | FSM Attack 5234b1d843 | Pilgrim: digs, teleports, thread attack at hero | sure | Attack range, dig point (closest to local hero) and timings read the local hero. | One game decides; share enemy state, HP and death. | | scenes/Ward_03.fsm.txt:2098 |
| Ward_03.4 | Song Pilgrim 02 ×8 | FSM Pull Hero 8445190b30 | Pulls or pushes the hero by velocity | maybe | Moves only the local hero, which is correct if the attack plays in both games against its target. | | Should the pull hit both players in range, or only the target? | scenes/Ward_03.fsm.txt:2070 |
| Ward_03.5 | Attack Threads ×8 | FSM Threads 29201df797 | Thread damager on and off | no | Follows the pilgrim attack. | | | scenes/Ward_03.fsm.txt:2419 |

## Ward_04

No new elements.

## Ward_05

No new elements.

## Ward_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ward_06.1 | Breakable Wall ×3 | FSM Shake 03729929af | Wall jitters on heavy work | no | Cosmetic. | | | scenes/Ward_06.fsm.txt:210 |
| Ward_06.2 | Understore Mite Giant ×5 | FSM Control ab65ee85c4 | Giant mite: wakes, leaps, multislash at hero | sure | Attack and ignore ranges read the local hero; random jump and unalert times. | One game decides; share enemy state, HP and death. | | scenes/Ward_06.fsm.txt:2501 |

## Ward_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ward_07.1 | Junk Hatch | FSM Control a43fa0847c | Hatch opens, drops corpse and item, wakes enemies | sure | Opening, the item pickup and the enemy wake happen only in one game. | The hatch open and the item pickup. | Item for both or only the taker. | scenes/Ward_07.fsm.txt:118 |

## Ward_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ward_09.1 | Sherma Rescue NPC | FSM Conversation Control a1e9c6822a | Sherma meet, junk battle, quest end, spool piece | sure | Dialogue, battle start, EndQuest (Save Sherma) and the spool piece run only in the talking game. | Battle start, battle end and the quest end. | Pull the partner into the battle and dialogue; spool piece for both or only the talker. | scenes/Ward_09.fsm.txt:3993 |
| Ward_09.2 | Sherma Rescue NPC | FSM Anim 257a5fe8a1 | Scared idle anims, random turns | no | Cosmetic. | | | scenes/Ward_09.fsm.txt:4248 |
| Ward_09.3 | ward_junk_pile_break | FSM lighten on break 4e418ee292 | Lightens darkness when pile breaks | maybe | Darkness changes only in the breaking game. | The pile break. | | scenes/Ward_09.fsm.txt:4306 |

## Arborium_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Arborium_01.1 | Arborium Keeper (2) ×4 | FSM Control 3d1a8b3dde | Flying keeper: throws, slash combo, blocks, at hero | sure | Alert, distance fly, throw and slash ranges read the local hero. | One game decides; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Arborium_01.fsm.txt:19 |
| Arborium_01.2 | Moss Vine Cluster | FSM FSM 859d0ffbf4 | One-way vine disables if hero enters below | maybe | Decided by the local hero's entry side; the partner may see the vine in a different state. | | | scenes/Arborium_01.fsm.txt:866 |

## Arborium_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Arborium_02.1 | Rhino Scene | FSM Control c3e82eeafd | Moves rhino behind hero on entry | maybe | Position chosen from the local hero's side at load; the two games may place it differently. | Rhino start side, once per scene. | | scenes/Arborium_02.fsm.txt:2372 |

## Arborium_03

No new elements.

## Arborium_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Arborium_04.1 | Enemy Respawner | C# PersistentFolderReset | Respawns enemy folder on rest or death | maybe | Each game respawns on its own hero's rest or death, so one game can have enemies back while the other does not. | The reset moment, if enemies are shared. | | scenes/Arborium_04.objects.txt:65 |

## Arborium_05

No new elements.

## Arborium_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Arborium_06.1 | Coral Goomba Large ×3 | FSM Behaviour 2c3483ce98 | Ambusher: emerges, charges hero, breaks walls | sure | Awake, alert and patrol ranges read the local hero; the charge can break a wall in one game only. | One game decides; share enemy state, HP and death, and the wall break. | | scenes/Arborium_06.fsm.txt:1127 |

## Arborium_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Arborium_07.1 | End Scene | FSM Control 96b997f974 | After battle, container opens with item | sure | The reward appears only in the game whose battle ended. | Battle end and the pickup taken. | Item for both or only the taker. | scenes/Arborium_07.fsm.txt:4 |
| Arborium_07.2 | Bola Chucker ×2 | FSM Control 251bece5cb | Keeper corpse spawns lightning bola | maybe | Spawns a hazard from the local corpse; matches if the death is shared. | Covered by enemy death sync. | | scenes/Arborium_07.fsm.txt:689 |
| Arborium_07.3 | Corpse Pilgrim 03 | FSM Face hero c04705965e | Corpse faces hero | no | Cosmetic. | | | scenes/Arborium_07.fsm.txt:913 |
| Arborium_07.4 | Zap Worm Lightning Control | FSM Control 09cf34dad1 | Zaps when hero in attack range | maybe | Triggered by the local hero's range; the partner sees no zap when only the other is near. | The zap start. | | scenes/Arborium_07.fsm.txt:951 |

## Arborium_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Arborium_08.1 | Giant Flea Cage | FSM Flea Control 6ae80b5598 | Cage hit and break frees giant flea | sure | Reads uncaged/tamedGiantFlea; the break runs only in the hitting game. | The cage break. | | scenes/Arborium_08.fsm.txt:4 |
| Arborium_08.2 | Giant Flea | FSM Control c7eeeedd33 | Flea fight: random dashes, stun, sets uncaged | sure | Random dash angles, local alert and camera lock; uncagedGiantFlea is written in one game. | One game decides; share flea state and the uncaged result. | Pull the partner into the flea fight or leave them free. | scenes/Arborium_08.fsm.txt:51 |

## Arborium_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Arborium_09.1 | Lilypad Plat ×40 | FSM Control 3538ca72a1 | Drifting lilypad platform, random drift, trap | maybe | Drift direction and speed are random, so the pads are in different places in each game. | The drift picks, or accept local pads. | Accept different pad positions, or share drift. | scenes/Arborium_09.fsm.txt:4 |
| Arborium_09.2 | Lilypad Plat/Enemy ×40 | FSM Control 3e192b3509 | Chomper pad snaps and catches hero | sure | The catch and kill run on the local hero; the partner's pad stays intact. | The snap of that pad. | | scenes/Arborium_09.fsm.txt:518 |
| Arborium_09.3 | MossBone Crawler Fat ×2 | FSM Behaviour bca8096b26 | Crawler: munches, roars, charges hero; extractable | sure | Wake, alert and chase ranges and the silk extract read the local hero. | One game decides; share enemy state, HP and death. | | scenes/Arborium_09.fsm.txt:923 |

## Arborium_10

No new elements.

## Arborium_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Arborium_11.1 | Citadel Bat ×7 | FSM Hero Aggro Detect 62a062d7eb | Bat aggros on hero, sets target | sure | Sets the bat's target to the local hero and broadcasts BATS AGGRO in one game. | Covered by the bat enemy state (target). | Target nearest player, or only the one who woke it. | scenes/Arborium_11.fsm.txt:268 |
| Arborium_11.2 | Wave 2 | FSM Flock Effect 8693f848e0 | Activates the spawn flock | maybe | Follows the battle wave. | Covered by the wave start. | | scenes/Arborium_11.fsm.txt:1362 |
| Arborium_11.3 | City Merchant Bridge | FSM Meet Control bc36ece5a7 | Saved merchant dialogue, ends quest | sure | Dialogue and the Save City Merchant Bridge quest end run in the talking game only. | The quest end. | Pull the partner into the dialogue or leave them free. | scenes/Arborium_11.fsm.txt:2842 |
| Arborium_11.4 | Citadel Bat Flock ×2 | FSM Control 573ed6eaea | Ambient bat flock flies off on enter | no | Random ambience triggered by the local hero. | | | scenes/Arborium_11.fsm.txt:2999 |

## Arborium_Tube

No new elements.

## Summary

### Events that should pull the partner to the trigger player
- Cog_Dancers.1 (Cogwork Dancers boss): arena gates close and floor gate shifts, defeat writes defeatedCogworkDancers and unlocks the melody lift; the lift ride transitions to Song_Tower_01.
- Ward_02_boss.1 (Conductor / Whiteward boss): phase waves and the silk heart; a split boss is unwinnable together.
- Cog_05.7/.8 (six-wave cog arena), Cog_07.2 (ball automaton arena), Cog_10_Destroyed.3 (black-thread wake battle): battle gates or roof close only in the triggering game.
- Ward_09.1 (Sherma rescue): dialogue, battle and quest end are one flow.
- Arborium_08.2 (Giant Flea fight): camera lock plus a persistent uncaged result.

### Random-driven fights (one game must decide)
- Cog_Dancers_boss is the worst case: every sequence, pattern (ArrayShuffle) and dancer position is a random pick; the dancers' roar stuns the hero.
- Conductor boss (Ward_02_boss.4) picks attacks with SendRandomEventV4 and aims spears at the local hero.
- Ward_02.2 picks a random husk respawn set; Cog_05.2 picks a random aftermath wave.

### Shared progress that is only local today
- Lever and puzzle chains: Cog_04 cog_lever (PD bool, energy gate), Cog_09 cog cylinder puzzle (16 cog levers, a saved bool per cylinder; cog positions before completion are not saved, so every hit must be shared), Cog_Dancers ring lock (saved bool) feeding CogEnergyTimeline (bench unlock).
- PlayerData: cog7_automatonDestroyed (Cog_07.8), wokeSongChevalier plus 3 Cog Heart Pieces consumed (Cog_10.1), wardBossHatchOpened (Ward_02.1), GreenPrinceLocation (Cog_Dancers.5), uncagedGiantFlea (Arborium_08), quests Save Sherma and Save City Merchant Bridge.
- Item pickups that spawn in one game only: Cog_07.6, Ward_02b.1/.4, Ward_07.1, Arborium_07.1.

### Open design choices
- Who pays for shared costs (3 Cog Heart Pieces for the Song Knight).
- Harpoon rings (Cog_08.1, Cog_Dancers.6): both ride one ring, or keep rings local.
- Time-based platforms (Cog_04 LoopMovementTrack, Cog_05 CogPlatController, Arborium_09 lilypads): accept per-game positions or align the timing. The other Hornet will look like it stands on air when they differ.
- Cog_09_Destroyed.2 forced weakness sequence: leave the partner free (recommended) or freeze them.
