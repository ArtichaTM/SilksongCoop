# Audit 2.16: Slab_* and Library_* (catalog/Slab.md lines 1-120, catalog/Library.md lines 1-116)

Verdict key: sure = coop must handle it, maybe = depends on a design decision or only a mild desync, no = local or cosmetic.
Unless stated otherwise, "the other player" means the player whose game did not run the trigger (each player runs a full game; only the local hero collides with triggers, hazards and alert ranges). "Same enemy rule" means: aggro, facing, chase and attack choice read the local hero, so one copy of the enemy must be driven for both (which hero it targets is the user's choice: nearest, first-aggro, or the one who triggered it).

## Slab_01

no new elements

## Slab_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_02.1 | Collectable Item Pickup (under Slab Chain cage_small_break (2)/lamp/Broken) | FSM [Deparent] bae4f053b9 | Detaches dropped pickup, saves its persistent bool | sure | Breaking the hanging cage drops a one-time persistent pickup that either player can take, so it can be collected twice or only exist in one game. | The cage break (the pickup then drops in both games) and the pickup being taken. | Item for whoever grabs it first, or both players get it. | scenes/Slab_02.fsm.txt:272 |

## Slab_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_03.1 | door_slabCaged | FSM [Teleport Arrive] 0d83bb994d | Caged-arrival cutscene: cage falls, hero flung out | sure | Entering through this gate removes control, hides the hero, parents it to the cage, sets the death respawn marker and autosaves SLAB_CAPTURED; the partner sees a hero frozen or missing. | Cutscene start and end for the hero who enters through it; the fall, impact and get-up replay locally. | Partner arriving elsewhere: leave free, freeze at the cage position, or pull to the cage. | scenes/Slab_03.fsm.txt:402 |
| Slab_03.2 | Audio Loop Slab_16 flies distant | FSM [FSM] 8bf801bf4e | Disables distant fly audio unless Cloakless crest | no | Local audio keyed to the local crest. | | | scenes/Slab_03.fsm.txt:695 |
| Slab_03.3 | Slab Fly Mid (1), (2) | FSM [Control] 5526a0becc | Walking fly: evades, slashes or spits at hero, alarm-aware | sure | Same enemy rule; also reacts to the ALARM RING event and to the needolin. | Which hero it targets, each attack start, death. | Same targeting choice as the other enemies. | scenes/Slab_03.fsm.txt:956 |
| Slab_03.4 | Prisoner Waker (child of Slab Fly Mid ×2) | FSM [FSM] 42cd0a35d8 | Sends ALARM when a collider enters trigger | maybe | Fires only when the local copy of the fly overlaps a prisoner, so alarm timing follows whichever game simulates the fly. | Nothing extra if the fly is driven for both. | | scenes/Slab_03.fsm.txt:1581 |
| Slab_03.5 | Slab Lock ×14 (Slab Lock, (1), (2) ...) | FSM [Control] fdbc4e8109 | Key-locked door: inspect, use key, door opens | sure | Unlock sets a PlayerData bool (HasSlabKey A/B/C is only the required key, not consumed) and opens a door; the partner's door stays shut and the unlock flash plays only for one. | The unlock (OPEN); the door animation, camera lock removal and flag replay in both games. | Whether the unlock needs the partner to own the key too (not consumed either way). | scenes/Slab_03.fsm.txt:2222 |
| Slab_03.6 | song_trap_door (2)/Opened Collider, (3)/Opened Collider | FSM [tilt_plat] e7537c30e9 | Plate nudges when tinked | no | Cosmetic reaction to a nail hit. | | | scenes/Slab_03.fsm.txt:3247 |
| Slab_03.7 | Thief NPC Slab Hopaway 2 | FSM [Control] f146180c63 | Grindle waves, runs off; writes grindleSlabSequence | maybe | Only appears with Cloakless crest and sequence 1; alert or leave range of the local hero sets sequence to 2, so players can be at different steps. | grindleSlabSequence as a shared step. | Show the NPC while any player qualifies, or per player. | scenes/Slab_03.fsm.txt:3530 |

## Slab_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_04.1 | Collectable Item Pickup (under Slab Chain cage_small_break/lamp/Broken) | FSM [Deparent] 4fd9ab041e | Detaches dropped pickup, saves its persistent bool | sure | Same as Slab_02.1: one-time pickup dropped by a breakable cage. | The cage break and the pickup being taken. | Same as Slab_02.1. | scenes/Slab_04.fsm.txt:692 |
| Slab_04.2 | slab_cage_plat_thin_fall (2) ×3 | C# SuspendedSwayPlat | Hanging platform falls when landed on; persistent | sure | One player landing drops it for good; the other game keeps a platform that the first game has already destroyed. | The landing that starts the fall (fall, impact shake and landed state replay). | | scenes/Slab_04.objects.txt:771 |

## Slab_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_05.1 | Slab Fly Small ×2 (and Fresh variants, 40 uses) | FSM [Bouncer Control] 9df4fd4117 | Small fly bounces at random angles, hatches from egg | sure | Random bounce angles and hatch timing differ per game, with HP and contact damage, so the two copies drift apart and die separately. | Hatch (egg or mother) and death; flight can stay local. | Whether to align the trivial fodder positions at all. | scenes/Slab_05.fsm.txt:4 |
| Slab_05.2 | Slab Prisoner Leaper New | FSM [Control] 01641c2295 | Chained prisoner wakes, leaps at hero, yankable | sure | Same enemy rule; wake range, noise and needolin read the local hero, and the chain yank is an event. | Wake and leap start (with target), YANK, death. | Same targeting choice as the other enemies. | scenes/Slab_05.fsm.txt:724 |
| Slab_05.3 | spike_trap_slab_jail/pressure_plate | C# TrapPressurePlate | Plate sinks under a body and fires OnPressed | sure | Plate sees only colliders of its own game, so a press by one player fires the spike trap in one game only. | The plate press (OnPressed); spikes then play in both games. | | scenes/Slab_05.objects.txt:962 |

## Slab_06

no new elements

## Slab_07

no new elements

## Slab_08

no new elements

## Slab_10b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_10b.1 | Shrine First Weaver | FSM [Inspection] 9cb253d435 | Summon shrine: hero inspects, boss fight begins | sure | The interacting hero is positioned, locked and invincible; encounter flag, camera locks and arena gates start only in his game. | The interact that starts the summon (INTRO); roar, gates and camlock replay. | Pull the partner into the arena on summon, freeze them, or leave them free. | scenes/Slab_10b.fsm.txt:3939 |
| Slab_10b.2 | First Weaver | FSM [Control] 15848edec8 | Boss: teleports, slashes, charges, casts pins and blasts, binds to heal | sure | Boss with 2 phases whose idle, evade, charge, teleport target and slash side read the local hero and RNG weights, so two copies diverge. | Attack choice with its target, stun and bind-heal events, phase change, HP. | Which hero it targets (nearest, first, or last hit). | scenes/Slab_10b.fsm.txt:301 |
| Slab_10b.3 | First Weaver | FSM [Fake Death] bfa988be35 | On lethal damage, sends FAKE DEATH | sure | The lethal hit decides when the boss staggers and the final sequence begins; it must start for both at once. | The kill (FAKE DEATH). | | scenes/Slab_10b.fsm.txt:1236 |
| Slab_10b.4 | First Weaver | FSM [Control] 15848edec8 (Death Stagger, Hornet Dash, Bind states) | Final blow: hero hidden, dashed to boss, bind, fade, load Memory_First_Sinner | sure | The final blow takes control from the hero, hides him, adds silk and ends in a scene transition to Memory_First_Sinner; the partner would be stranded mid-fight or be dragged along. | The final blow, and whether the transition is taken by each player. | Pull the partner into the memory, let them watch, or let them continue alone. | scenes/Slab_10b.fsm.txt:301 |
| Slab_10b.5 | Corpse First Weaver(Clone) | FSM [Death] 3336281dbf | Boss death: stagger, steam, explosion, shrine NPC activates | sure | Fires BOSS STUNNED, BOOM (opens gates) and SPIRE NPC ACTIVATE; the other game keeps the arena locked. | Boss death; explosion, gates and NPC reveal replay. | | scenes/Slab_10b.fsm.txt:1250 |
| Slab_10b.6 | Gates | FSM [Deactivate] 6ee64e768b | Disables arena gates on BOOM | sure | Follows Slab_10b.5; if missed, the player is locked in. | Nothing extra, follows Slab_10b.5. | | scenes/Slab_10b.fsm.txt:1400 |
| Slab_10b.7 | Blasts; First Weaver Blast ×4 | FSM [Test] cf1093e257; FSM [Control] 82e94929ca | Random-position vertical blasts on BLAST | sure | Spawn X and Y are random per game and damage the local hero, so the hazard is in different places in each game. | The BLAST with its random positions. | | scenes/Slab_10b.fsm.txt:36, scenes/Slab_10b.fsm.txt:45 |
| Slab_10b.8 | Pin Projectiles; FW Pin Projectile ×10 | FSM [Pattern Control] c8de9e4cad; FSM [Control] 131c3e2244 | Pin volleys: rain, sweep, pincer, claw patterns | sure | Pattern choice is random and the sweep side reads the hero X, so the volleys land differently in each game. | Pattern choice with its X and rotation seeds. | | scenes/Slab_10b.fsm.txt:2501, scenes/Slab_10b.fsm.txt:2701 |
| Slab_10b.9 | FW Loose Pin ×15 | FSM [Control] 590295a1db | Wall pins pulled for projectiles, stick or fall | no | Set dressing driven by Slab_10b.8; the random stick-or-fall result is cosmetic. | | | scenes/Slab_10b.fsm.txt:1406 |
| Slab_10b.10 | Shrine First Weaver NPC | FSM [Inspection] b6c1d0118a | Post-boss shrine: dialogue, bind prompt, grants ability | sure | Hero is locked and invincible; ends by setting the ability PlayerData bool (silk bomb), adding silk and autosaving; only the interacting player gets the ability. | The ability grant (PlayerData) and shrine completion. | Each player binds for his own ability, or one bind grants both. | scenes/Slab_10b.fsm.txt:3431 |
| Slab_10b.11 | First Weaver (shrine NPC child) | FSM [Control] 274f26c11e | NPC appear flash and camera shake | no | Presentation. | | | scenes/Slab_10b.fsm.txt:3772 |
| Slab_10b.12 | Boss Scene; Memory_Scene; CamLock Battle, CamLock Battle Intro | FSM [Lingering Effects] 4337c43218; FSM [Memory Activate] 5ece1fc4b9; FSM [End] d18fe139ca | Swaps scene sets after the memory; drops camlocks | maybe | Runs on the LINGERING EFFECTS event after returning from the memory; a player who did not go through it would not get the cage scene. | LINGERING EFFECTS for whoever returns from the memory. | | scenes/Slab_10b.fsm.txt:4, scenes/Slab_10b.fsm.txt:3907, scenes/Slab_10b.fsm.txt:289 |
| Slab_10b.13 | Boss Scene | FSM [Outro] 3304bf277a | After boss death, shrine NPC appears, boss song off | sure | Triggered by SPIRE NPC ACTIVATE; the shrine NPC must exist before the other player can bind. | Follows Slab_10b.5. | | scenes/Slab_10b.fsm.txt:13 |
| Slab_10b.14 | Top Shover ×31 (memory_climb_wall) | FSM [Control] 1fe129c747 | Pushes grounded hero sideways off wall tops | no | Local hero physics in the memory scene. | | | scenes/Slab_10b.fsm.txt:3919 |

## Slab_10c

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_10c.1 | Break Gate Group (4 slab_break_weaver_gate_ring) | FSM [Gate Break] 2cbf7c0395 | Counts broken rings, breaks gate at 4, enables door | sure | Rings are persistent breakables counted per game; breaks by one player do not reach the other's counter, so the gate stays closed there. | Each ring break (JUST BROKE ONE); count, gate break and door enabling follow. | | scenes/Slab_10c.fsm.txt:4 |

## Slab_12

no new elements

## Slab_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_13.1 | trap_spinning_blade_S_bend_slab_jail ×3 | C# SplineBladeTrap | Pressure plate sends a damaging blade along a spline | sure | Three plates (TrapPressurePlate, see Slab_05.3) start blades that hurt both heroes and enemies; a plate press in one game runs the blade only there. | The plate press; the blade run replays natively. | | scenes/Slab_13.objects.txt:825 |
| Slab_13.2 | prop_blade ×3 (WallDust Control) | FSM [WallDust Control] 06bdb53c06 | Dust and camera shake when blade hits wall | no | Cosmetic follow-up of Slab_13.1. | | | scenes/Slab_13.fsm.txt:1215 |
| Slab_13.3 | Pt WallDust ×3 | C# ReparentOnParticleStop | Reparents dust particles when they stop | no | Cosmetic. | | | scenes/Slab_13.objects.txt:896 |

## Slab_14

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_14.1 | Slab Alarm Prisoner (1), (2), Fly (2) | FSM [Control] b663969916 | Chained prisoner roars ALARM on alert, noise or damage | sure | Alert range, noise and hero damage are local, and the ALARM broadcast wakes the other slab enemies in one game only. | The ALARM roar; woken enemies follow. | | scenes/Slab_14.fsm.txt:4 |
| Slab_14.2 | Slab Alarm Prisoner ×3 | FSM [Chain In Place] 93b36a4816 | Chain pull animation | no | Cosmetic. | | | scenes/Slab_14.fsm.txt:127 |
| Slab_14.3 | Slab Prisoner Fly New ×2 | FSM [Control] 371a45f088 | Chained fly aims and attacks in range, yankable | sure | Same enemy rule; attack angle is taken from the local hero. | Attack start with its angle, YANK, death. | Same targeting choice as the other enemies. | scenes/Slab_14.fsm.txt:430 |
| Slab_14.4 | Thief NPC Slab Hopaway | FSM [Control] 16dbccb4d9 | Grindle waves, talks (key lock hint), leaps away | maybe | Appears only with Cloakless crest and no HasSlabKeyA; talk branch reads grindleReleasedFromBonejail and writes grindleSlabSequence. | grindleSlabSequence and the Slab Lock L interactible. | One shared NPC step or per player. | scenes/Slab_14.fsm.txt:1568 |

## Slab_15

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_15.1 | Audio Loop DustFall (Breakable Wall Waterways) | FSM [volume_curve] fd33f8bb36 | Random volume swell of falling dust | no | Audio. | | | scenes/Slab_15.fsm.txt:210 |

## Slab_16

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_16.1 | Event Control | FSM [Control] 8433f2abd2 | Picks battle, visit, completed or broodmother variant | sure | The variant comes from the local Cloakless crest, entry gate and PlayerData (slab_cloak_battle_completed, Broodmother Hunt quest), so players can load different content. | The PlayerData flags; the variant then follows. | Crest mismatch: use each player's own variant or the first entrant's. | scenes/Slab_16.fsm.txt:375 |
| Slab_16.2 | Battle Gate Slab ×9 (Cloaked and Cloakless) | FSM [BG Control] a136fa2aa8 | Arena gates close and open on BG events | sure | Closes only for the player whose battle started; follows the battle scene. | Nothing extra, follows battle start and end. | | scenes/Slab_16.fsm.txt:437 |
| Slab_16.3 | Start Inspect (Cloaked) | FSM [Control] 5c3072eccc | Hero inspects jail gate, is slammed in, battle starts | sure | The interacting hero is locked, kinematic and thrown into the arena, gates close and slab_cloak_battle_encountered is set; the other player is not in the arena. | The interact that starts the battle; slam, camlock and gates replay. | Pull the partner into the arena, wait for them, or leave them outside. | scenes/Slab_16.fsm.txt:533 |
| Slab_16.4 | Slab Fly Large ×17 (waves 1 to 5, cloakless waves) | FSM [Control] 1fae1288a7 | Large fly: grab, stomp, spit, flutter, sleeps in crowd | sure | Same enemy rule; wake, move and attack choice use the local hero and RNG, and the grab collider hurts him. | Wake, attack choice with target, death. | Same targeting choice as the other enemies. | scenes/Slab_16.fsm.txt:585 |
| Slab_16.5 | Item Placer ×2 (Wave 5 - Item, Wave 9 - Item) | FSM [Control] ea1df04685 | Flings reward pickup and ends battle | sure | Spawns a one-time reward when the battle ends, so each game would drop its own and the battle end message is local. | Battle end and the reward being taken. | Reward for the collector or for both players. | scenes/Slab_16.fsm.txt:5027 |
| Slab_16.6 | Jail Gate Door Close Audio Custom | FSM [FSM] 83a9217f72 | Plays close audio on BG CLOSE | no | Audio. | | | scenes/Slab_16.fsm.txt:5144 |
| Slab_16.7 | Jail Gate Door End Control | FSM [Control] fe03ecfcf3 | Opens jail door on BATTLE END | sure | Follows battle end; otherwise the arena exit stays closed. | Nothing extra, follows Slab_16.5. | | scenes/Slab_16.fsm.txt:5150 |
| Slab_16.8 | Dancer Fly, Dancer Fly Death | FSM [Control] c783a74caa; FSM [Footsep Audio] 495005e795; FSM [Control] c60bd9efc4 | Taunting fly dances, is grabbed and killed in intro | maybe | Part of the Slab_16.9 cutscene; taunt type reads PlayerData slabCaptor flags; presentation only. | Nothing extra, follows Slab_16.9. | | scenes/Slab_16.fsm.txt:12225, scenes/Slab_16.fsm.txt:12294, scenes/Slab_16.fsm.txt:12305 |
| Slab_16.9 | Start Inspect (Cloakless) | FSM [Control] ceb057c8e6 | Hero is captured: hidden, dummy grabbed, battle starts | sure | Hides the real hero, parents a dummy, teleports to the dancer and starts the arena; the partner sees a vanished or frozen hero. | The interact that starts the cutscene and battle. | Pull the partner in, freeze them, or leave them free. | scenes/Slab_16.fsm.txt:12333 |
| Slab_16.10 | Gate Return Inspect | FSM [Control] 4061374216 | After victory, hero opens gate and drops out of jail | maybe | Local interaction: gate opens, hero is translated and dropped at -20 velocity; no state saved. | Nothing extra beyond Slab_16.11. | | scenes/Slab_16.fsm.txt:12460 |
| Slab_16.11 | Jail Gate Door - Persistent | FSM [Control] 8b9f02d0e9 | Downward hit opens gate, sets slab_cloak_gate_reopened | sure | A persistent shortcut opened by one player's hit; the other game keeps the gate shut. | The downward hit (TINK DOWN) or the PlayerData flag with a live open. | | scenes/Slab_16.fsm.txt:12495 |
| Slab_16.12 | Kit Guard Cloak, Kit Guard Needle | FSM [Noise React] 1cc43f1cce, 5e8cedca50 | Idle guards chat, startle at noise | no | Cosmetic NPCs reacting to noise. | | | scenes/Slab_16.fsm.txt:16852, scenes/Slab_16.fsm.txt:16875 |

## Slab_16b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_16b.1 | Audio Boss Tension | FSM [Control] b66d331fb0 | Plays tension music near the boss | no | Local music. | | | scenes/Slab_16b.fsm.txt:4 |
| Slab_16b.2 | Audio Loop Broodmother Wail | FSM [Fade Out] b9df8906c9 | Fades wail audio | no | Local audio. | | | scenes/Slab_16b.fsm.txt:26 |
| Slab_16b.3 | Tiny Brood Mother | FSM [Behaviour] 3bf83fcb30 | Post-hunt NPC: dialogue, sing reaction, scuffle | maybe | Dialogue and needolin reactions are local; only the meet counter differs per player. | | Dialogue state per player. | scenes/Slab_16b.fsm.txt:31 |
| Slab_16b.4 | Slab Fly Broodmother | FSM [Control] e9c7ed42e8 | Broodmother boss (700 HP): hatches flies, spits, leaps | sure | Boss in the Broodmother Hunt quest battle; leap, spit and hatch choice use the local hero and RNG, and spawns the fly brood. | Attack choice with target, hatch spawns, stun, death, battle end. | Which hero it targets; pull the partner into the nest battle. | scenes/Slab_16b.fsm.txt:5424 |
| Slab_16b.5 | Slab Fly Small Fresh ×14 (Dripper) | C# Dripper | Spawns dripping goo splatter on the fly | no | Cosmetic particles. | | | scenes/Slab_16b.objects.txt:324 |

## Slab_17

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_17.1 | Heart Piece (under Slab Chain cage_small_break/lamp/Active) | FSM [Deparent] 1eda2de017 | Detaches heart piece from cage lamp | sure | Heart piece is a persistent, one-time progression pickup; either player could take it or the cage opens only in one game. | The cage break and the pickup being taken. | Piece for the collector or for both players. | scenes/Slab_17.fsm.txt:217 |

## Slab_18

no new elements

## Slab_19b

no new elements

## Slab_20

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_20.1 | Thief NPC Slab Saferoom | FSM [Conversation] 8bb72d42e6 | Grindle saferoom meet and repeat dialogue | maybe | Dialogue locks only the talking hero; leaves when Cloakless crest is not equipped, so players differ. | | Per-player dialogue stage or shared. | scenes/Slab_20.fsm.txt:804 |

## Slab_21

no new elements

## Slab_22

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_22.1 | Music Control | FSM [FSM] 3402552282 | Silences snapshot after crest check | no | Local audio. | | | scenes/Slab_22.fsm.txt:71 |
| Slab_22.2 | Slab Alarm Bell (3), (4) | FSM [Control] 907874a8b4 | Rings on ALARM RING, rebroadcasts; hit spawns slash effect | maybe | Rings and re-broadcasts the alarm to enemies in this game only. | The alarm source if it is player-caused. | | scenes/Slab_22.fsm.txt:132 |

## Slab_23

no new elements

## Slab_Cell

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_Cell.1 | Flea Slab Cage (1) | FSM [Flea Control] e74272f261 | Caged flea: break frees it, quest scene bool | sure | Freeing a flea is a one-time rescue counted in the flea quest; one game frees it while the other keeps the cage. | The cage break (BREAK); the flea emerging replays. | Flea credit for the rescuer or for both players. | scenes/Slab_Cell.fsm.txt:4 |
| Slab_Cell.2 | Flea Slab Cage/lamp | FSM [Flea Jitter] 9d5291b7e3 | Lamp jitters while the flea is caged | no | Cosmetic. | | | scenes/Slab_Cell.fsm.txt:155 |

## Slab_Cell_Creature

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Slab_Cell_Creature.1 | Slab Prisoner | FSM [Dialogue] c81cf8006c | Hidden prisoner: dialogue, hums to needolin | maybe | Dialogue locks the local hero and reads PlayerData slabPrisonerSingConvo; reactions use the local needolin. | slabPrisonerSingConvo as a shared flag. | Per-player dialogue stage or shared. | scenes/Slab_Cell_Creature.fsm.txt:4 |

## Slab_Cell_Quiet

no new elements

## Library_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_01.1 | Acolyte Control | FSM [Control] 63afaa8937 | Spawns first cluster or random returning acolytes | maybe | Sets seenScholarAcolytes once; return count and slots are random per game. | seenScholarAcolytes; acolyte count if the same enemies must appear. | | scenes/Library_01.fsm.txt:9 |
| Library_01.2 | Song Scholar Acolyte ×N (51 uses) | FSM [Control] f11688b96a | Hiding acolyte wakes, leaps and dives at hero | sure | Same enemy rule; wake range and dive aim read the local hero and RNG. | Wake with target, leap or dive start, death. | Same targeting choice as the other enemies. | scenes/Library_01.fsm.txt:38 |
| Library_01.3 | Scene Blanker | FSM [Scene Blanker] b65ee43b85 | Toggles a scene blank sprite | no | Visual toggle. | | | scenes/Library_01.fsm.txt:8369 |
| Library_01.4 | Scrollkeeper, Scrollkeeper (1) | FSM [Control] 18b52e95a3 | Persistent 100 HP guard: patrol, wake, ambush, stab | sure | Same enemy rule; also black-thread possession and a wake by damage or noise. | Wake with target, attack start, death. | Same targeting choice as the other enemies. | scenes/Library_01.fsm.txt:8377 |
| Library_01.5 | Library Sliding Plat ×2 | C# HitSlidePlatform | Nail hit slides platform along node graph, persistent | sure | A hit by one player moves it in one game, and it damages heroes and enemies while sliding; the node index is saved per game. | The hit (direction); the slide replays. | | scenes/Library_01.objects.txt:1841 |
| Library_01.6 | Sliding Plat Node ×59 | C# HitSlidePlatformNode | Path node for the sliding platform | no | Static path data. | | | scenes/Library_01.objects.txt:3692 |

## Library_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_02.1 | Battle Scene Control | FSM [Control] 1f8fc2fee2 | Chooses entry battle, acolyte battle or returning acolytes | sure | Choice reads PlayerData completedLibraryEntryBattle, completedLibraryAcolyteBattle and scholarAcolytesInLibrary_02, so players at different progress get different arenas. | The three PlayerData flags. | Use the first entrant's state or each own. | scenes/Library_02.fsm.txt:763 |
| Library_02.2 | Door Closer | FSM [Control] 9377ef4e9e | Trigger closes arena gates and camlocks | sure | Closes only on the hero who touched it, locking one player in. | The trigger (BG CLOSE); camlocks follow. | Pull the partner in, wait for them, or leave them outside. | scenes/Library_02.fsm.txt:5879 |
| Library_02.3 | Battle Scene Acolyte | FSM [Drop Reward - deprecated] beeb7b16f8 | Activates reward pickup on BATTLE END | sure | One-time reward appears when the battle ends; each game would drop its own. | Battle end and pickup taken. | Reward for the collector or both. | scenes/Library_02.fsm.txt:795 |
| Library_02.4 | Song Scholar Acolyte Singer | FSM [Control] 9ed8108efd | Singing acolyte paces, flees on battle start | no | Ambient until BATTLE START, then leaves. | | | scenes/Library_02.fsm.txt:2776 |
| Library_02.5 | Scholar ×8 (waves 5 and 6) | FSM [Control] b55293a171 | Singing preacher; battle starts near hero, bursts, leaps | sure | Battle start trigger (Battle Start Range) and burst/leap aim read the local hero. | Battle start with target; burst and attacks; death. | Same targeting choice as the other enemies. | scenes/Library_02.fsm.txt:4685 |
| Library_02.6 | Return Acolytes | FSM [Control] 00b7f27a93 | Spawns 0 to 3 returning acolytes after release | maybe | Count and slots random per game, gated by scholarAcolytesReleased. | | | scenes/Library_02.fsm.txt:13996 |

## Library_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_03.1 | City Merchant Scavenge Generic | FSM [Dialogue] 995b091198 | Merchant rummages, greets, leaves; sets seen and left flags | maybe | Appears by PlayerData and quest state (Save City Merchant Bridge); dialogue locks the local hero, left flags are written by whoever talks. | cityMerchantInLibrary03Seen and Left flags. | Per-player dialogue stage or shared. | scenes/Library_03.fsm.txt:9 |
| Library_03.2 | Secret Mask Scene/CameraLockArea | FSM [Remove] dc06298832 | Removes the camera lock after the secret reveal | maybe | Camera lock is local; the persistent secret mask reveal (Unmasker) is revealed only in one game. | The secret reveal. | | scenes/Library_03.fsm.txt:142 |

## Library_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_04.1 | Acolyte Control | FSM [Control] baacd49864 | Spawns 7 to 8 returning acolytes after release | maybe | Count and slots random per game. | | | scenes/Library_04.fsm.txt:9 |
| Library_04.2 | Lightbearer ×4 (Normal and Black Thread World) | FSM [Control] 1dadc2a762 | Scrollkeeper-like guard: wakes, ambushes, stabs; minion events | sure | Same enemy rule; wake range and damage hit the local hero, also black thread possession. | Wake with target, attack start, death. | Same targeting choice as the other enemies. | scenes/Library_04.fsm.txt:6659 |
| Library_04.3 | CameraLockArea AtticDrop Control | FSM [FSM] c7870e6101 | Toggles camera lock by hero position | no | Local camera. | | | scenes/Library_04.fsm.txt:8397 |
| Library_04.4 | Music Control | FSM [Control] 2296c759bb | Chooses music by enclave level and entry gate | no | Local audio. | | | scenes/Library_04.fsm.txt:10241 |
| Library_04.5 | Lightbearer ×4 | C# CharacterLightDust | Light dust tint from scene appearance regions | no | Cosmetic. | | | scenes/Library_04.objects.txt:5026 |

## Library_05

no new elements

## Library_06

no new elements

## Library_07

no new elements

## Library_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_08.1 | Librarian (Scene) | FSM [Dialogue] 0f7d3a4819 | Vaultkeeper: roar wake, relic ask, deposit board | sure | Wakes and roar-locks the local hero, gives dialogue and relic deposit; writes LibrarianAskedForRelic, GivenLibrarianRelic and quest SubQuest:Citadel Ascent Melody Librarian Return. | The wake (roar lock) for the hero who triggers it; the relic and quest PlayerData. | One shared relic board and quest, or per player; pull the partner into the roar. | scenes/Library_08.fsm.txt:395 |
| Library_08.2 | Librarian Scene Simple 1, 2/Librarian | FSM [Dialogue] 1876bee835, 1d7d559438 | Smaller Vaultkeeper variants after relic given | maybe | Same wake logic after GivenLibrarianRelic; wake flags and roar are local. | Same as Library_08.1. | Same as Library_08.1. | scenes/Library_08.fsm.txt:76, scenes/Library_08.fsm.txt:192 |
| Library_08.3 | Librarian | FSM [Tentacle Control] 5783dd49b0 | Random tentacle idle and move events | no | Cosmetic. | | | scenes/Library_08.fsm.txt:700 |
| Library_08.4 | Librarian | FSM [Melody Sing] 0b80d1905c | Sings along with needolin melody | no | Presentation tied to the local needolin. | | | scenes/Library_08.fsm.txt:733 |
| Library_08.5 | Gramaphone | FSM [Behaviour] e9fe3102e1 | Plays deposited relic song | no | Local audio. | | | scenes/Library_08.fsm.txt:308 |
| Library_08.6 | Audio Player Actor (Vaultkeeper Mutter) | FSM [FSM] 28130fed3d | Ducks mutter audio when Vaultkeeper emerges | no | Audio. | | | scenes/Library_08.fsm.txt:1926 |
| Library_08.7 | Scene Appearance Region - Library | C# BreakablePoleSimple | Decorative pole splits when nail hit | no | One-shot decoration, nothing saved. | | | scenes/Library_08.objects.txt:2348 |

## Library_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_09.1 | Citadel Bat Flock | FSM [Control - Custom] c3378e8d6e | Trigger releases bat alert, direction from hero | sure | Black-thread-world only; trigger and flight direction come from the local hero. | The trigger (ALERT with direction). | | scenes/Library_09.fsm.txt:9 |
| Library_09.2 | Citadel Bat Large L, R ×9 | FSM [Control] a82e6bd6d3 | Roosting bat wakes, flies and dives at hero | sure | Same enemy rule; ambush and wake range read the local hero. | Wake with target, dive start, death. | Same targeting choice as the other enemies. | scenes/Library_09.fsm.txt:402 |
| Library_09.3 | Garmond Scene | FSM [Control] bd58ccb58a | Hero enters range: Garmond fights pilgrims | sure | Writes garmondLibrarySeen when the local hero enters, places Garmond by hero side and sends CITADEL CHARGE; only one game runs the fight. | The start trigger (Check Dir with hero side) and garmondLibrarySeen. | Pull the partner into the scene or let them watch. | scenes/Library_09.fsm.txt:6211 |
| Library_09.4 | Hornet Defeat Recorder | FSM [Control] 5417a3cb7f | Sets garmondLibraryDefeatedHornet on HORNET DEATH | maybe | Records a death of the local hero during the Garmond fight, so a partner death would not count. | HORNET DEATH as a flag if either player counts. | Count either player's death or the triggerer's only. | scenes/Library_09.fsm.txt:7521 |
| Library_09.5 | Kill Enemies | FSM [Kill] d7b0485f79 | Marks five pilgrim and reed enemies dead in save data | sure | Writes persistent dead flags for the scene enemies, so a kill happens in one game's save only. | The kill event or the persistent flags. | | scenes/Library_09.fsm.txt:7672 |
| Library_09.6 | Spike Collider ×8 | FSM [Set Garmond Dmg] 395b8213d3 | Spikes hurt Garmond's enemies during the battle | no | Only enemies and Garmond's fight; no hero effect. | | | scenes/Library_09.fsm.txt:8748 |

## Library_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_10.1 | Collectable Item Pickup - Melody | FSM [FSM] 70279e69ac | On pickup, locks hero, shows hint, updates melody quest | sure | One-time relic pickup writing PlayerData and quest SubQuest:Citadel Ascent Melody Librarian; either player can take it and the other game keeps it. | The pickup being taken and the quest update. | Relic for the collector or both players. | scenes/Library_10.fsm.txt:215 |
| Library_10.2 | Library Statue NPC | FSM [Interaction] 01f14b1e11 | Statue wakes with roar, speaks, sleeps | maybe | Waking writes libraryStatueWoken, shakes camera and roar-stuns the hero in range; sleeps on exit. | libraryStatueWoken; wake animation for the nearest player. | Whether the roar stun also applies to the partner. | scenes/Library_10.fsm.txt:233 |

## Library_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_11.1 | Vent Pass Through/Inside Trigger | FSM [FSM] c0d28cfbba | Closes vent gate after hero leaves trigger | no | Gate follows the local hero only; nothing saved. | | | scenes/Library_11.fsm.txt:23 |
| Library_11.2 | Move Up/library_lift_large/Platform Stick | C# NoWallClingRegion | Blocks wall cling inside the region | no | Local hero movement rule. | | | scenes/Library_11.objects.txt:70 |
| Library_11.3 | Vent Pass Through/Variants | C# HeroEnterSceneXResponder | Picks vent variant by entry side | no | Reads the local hero's entry side only. | | | scenes/Library_11.objects.txt:658 |

## Library_11b

no new elements

## Library_12

no new elements

## Library_12b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_12b.1 | Exhaust Column Timer (2) | FSM [Control] 94146062c4 | Steam column cycles up and down until defeatedPhantom | maybe | Each game runs its own timer phase, so the steam hurts the local hero but the partner may stand in a column that is off. | The cycle start time once. | | scenes/Library_12b.fsm.txt:1045 |

## Library_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_13.1 | GameObject (1) | FSM [Control] f5ebe965aa | Switches audio snapshot on left entry | no | Local audio. | | | scenes/Library_13.fsm.txt:11 |
| Library_13.2 | Grand Stage Scene | FSM [Control] a93173370f | Picks Trobbio or Tormented Trobbio stage by act and quest | sure | Choice reads blackThreadWorld, defeatedTrobbio and the Tormented Trobbio quest, and writes defeatedTormentedTrobbio on battle end; players at different progress get different arenas. | The PlayerData flags and TORMENTED BATTLE END. | Use the first entrant's stage or each own. | scenes/Library_13.fsm.txt:22 |
| Library_13.3 | Trobbio | FSM [Control] 4116f819b7 | Act 2 boss: tornado, bombs, jumps, flares, entrances | sure | Boss with RNG weights; start trigger (Start Range), spin direction, evade and tornado direction read the local hero; ZERO HP sets defeatedTrobbio. | Start, attack choice with target, stun, phase change, death. | Pull the partner into the boss arena when the Start Range fires. | scenes/Library_13.fsm.txt:4140 |
| Library_13.4 | Tormented Trobbio | FSM [Control] 5e291a520f | Act 3 boss: same moves, phase roar, intro dialogue | sure | Intro dialogue locks the hero, sets encounteredTormentedTrobbio and camlocks; combat reads the local hero and RNG. | Same as Library_13.3 plus the intro dialogue end. | Intro dialogue: pull the partner in, freeze them, or leave them free. | scenes/Library_13.fsm.txt:1337 |
| Library_13.5 | Tormented Trobbio, Trobbio | FSM [Tornado Emission] a5bab8d6ed; FSM [Tween Y Pos] 4573f664db | Tornado spawn height check and Y tween | no | Follow-up of the boss tornado. | | | scenes/Library_13.fsm.txt:2310, scenes/Library_13.fsm.txt:2322 |
| Library_13.6 | Bottom Tornado Sprite, Tornado Event Sender ×3 | FSM [Tornado Event Sender] 3ed7328a27 | Sends TORNADO to bodies in the tornado | maybe | Sends a TORNADO hit to colliders (including the hero); a hit by it on one player only reaches him. | Nothing extra if the boss attack is shared. | | scenes/Library_13.fsm.txt:2331 |
| Library_13.7 | Corpse Tormented Trobbio(Clone) | FSM [Control] 8db814510e | Boss death, explosion, defeat dialogue | sure | Dialogue locks the hero; death sets up the quest item reward and ends the battle. | Boss death and the dialogue end. | Reward for the finisher or both. | scenes/Library_13.fsm.txt:2451 |
| Library_13.8 | Item Spawn (TormentedTrobbio) | FSM [Spawn Item] 73b684c15b | Activates reward pickup on SPAWN ITEM | sure | One-time reward appears once; each game would drop its own. | SPAWN ITEM and the pickup being taken. | Reward for the collector or both. | scenes/Library_13.fsm.txt:1234 |
| Library_13.9 | Trapdoor Bursts (both bosses) | FSM [Control] 304eb01841, 220342e3ab | Random trapdoor burst columns on boss ATTACK | sure | Burst column choice and X shift are random and the columns damage the local hero. | The ATTACK with its pattern choice. | | scenes/Library_13.fsm.txt:2769, scenes/Library_13.fsm.txt:3895 |
| Library_13.10 | Tormented Trobbio TrapdoorWatch | FSM [Control] 3a647a2834 | Trobbio peeks from trapdoor when hero stays quiet | no | Cosmetic easter egg for the local hero (after the Dazzle Bind upgrade). | | | scenes/Library_13.fsm.txt:5421 |
| Library_13.11 | Confetti Shooters, Flare Glitter, Flare Glitter Old, Flare Glitter 1..N, Spotlight L, R, stage lighting ambient, Steam Jets, Fog Follow Hornet | FSM [Control] e66994e758, f5bf506bbc, 503b4058aa, c784780799, 448daa8ae1, 5e835ba14d, 001850b8ba; FSM [Fade] 6267ce3561, 05bdffcb84; FSM [spotlight_control] b49c8db669; FSM [FSM] 82161d710e, 38bf687a81 | Stage lights, confetti, steam and fog presentation | no | Pure presentation of the fight. | | | scenes/Library_13.fsm.txt:94, scenes/Library_13.fsm.txt:104, scenes/Library_13.fsm.txt:288, scenes/Library_13.fsm.txt:424, scenes/Library_13.fsm.txt:2969, scenes/Library_13.fsm.txt:1330, scenes/Library_13.fsm.txt:1100, scenes/Library_13.fsm.txt:1243, scenes/Library_13.fsm.txt:3841, scenes/Library_13.fsm.txt:1253, scenes/Library_13.fsm.txt:1321, scenes/Library_13.fsm.txt:3812 |
| Library_13.12 | Trobbio Bump Floor ×32 | FSM [Control] eca9b60ce3 | Floor tile flips when stepped on | no | Cosmetic. | | | scenes/Library_13.fsm.txt:732 |
| Library_13.13 | trobbio_signpost | FSM [Hit Animate] f3a3113698 | Signpost wobbles when hit | no | Cosmetic. | | | scenes/Library_13.fsm.txt:5383 |
| Library_13.14 | FakeDeath Range (Trobbio) | C# CameraAspectScaler | Scales a collider with the camera aspect | no | Local camera setup. | | | scenes/Library_13.objects.txt:2127 |

## Library_13b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_13b.1 | Desk Inspect and Quill | FSM [FSM] b45c095dac | Shows collectable quill or inspect by QuillState | sure | Reads and writes PlayerData QuillState (2 or 3 by act) and hasQuill; a quill taken in one game stays on the desk in the other. | QuillState and the pickup being taken. | Quill for the collector or both players. | scenes/Library_13b.fsm.txt:53 |
| Library_13b.2 | Collectable Item Pickup - Quill Purple, Red | FSM [FSM] 66f2aa36d0 | Pickup prompt, collects quill or shows lines | sure | One-time progression pickup behind a yes or no prompt. | Same as Library_13b.1. | Same as Library_13b.1. | scenes/Library_13b.fsm.txt:105 |

## Library_14

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_14.1 | Scholar Scene | FSM [Control] 7a88764c35 | Picks ambush or revisit scholar by library_14_ambush | sure | The ambush contact sets library_14_ambush in one game only, so the other game keeps ambush while the first has revisit. | library_14_ambush and the ambush trigger. | | scenes/Library_14.fsm.txt:210 |

## Library_15

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Library_15.1 | Acolyte Return Scene | FSM [Control] c09da1713c | Spawns one random returning acolyte | maybe | Random pick per game, gated by seenScholarAcolytes. | | | scenes/Library_15.fsm.txt:4 |
| Library_15.2 | Acolyte Scene | FSM [Control] 4a0298f44c | After lever OPEN, lights on and acolytes leave | maybe | Driven by the Song_lever_side Lever_tk2d (not in this catalog); only the hitter's game opens it. | The lever hit (OPEN); lights, darkness and acolyte exit replay. | | scenes/Library_15.fsm.txt:2799 |
| Library_15.3 | Leap Away Acolyte ×32 | FSM [Control] 05c9f00cc4 | Acolyte leaps away on LEAVE | no | Cosmetic reaction to Library_15.2. | | | scenes/Library_15.fsm.txt:2816 |
| Library_15.4 | Acolyte Knock Lectern | FSM [Control] 8ebb63b970 | Lectern topples on KNOCK | no | Cosmetic. | | | scenes/Library_15.fsm.txt:3776 |
| Library_15.5 | Hiding Song Acolyte ×12 | FSM [Control] d8e41e7046 | Acolyte hides and emerges by hero alert range | no | Cosmetic, nothing saved, local alert range. | | | scenes/Library_15.fsm.txt:3783 |
| Library_15.6 | Darkness Region | FSM [Darkness Region] 0137589ab6; FSM [Lever Deactivate] 4819dfd683 | Dark zone while hero inside; lever turns it off | maybe | Darkness is local to the hero inside; the lever deactivates it, only for the game where it was hit. | The lever hit (OPEN) as in Library_15.2. | | scenes/Library_15.fsm.txt:4335, scenes/Library_15.fsm.txt:4369 |
| Library_15.7 | Lamp Covers | C# AmbientLightAnimator | Sets scene ambient intensity | no | Local lighting. | | | scenes/Library_15.objects.txt:111 |

## Library_16

no new elements

## Summary

Notes written for C# types without one: audit/notes/TrapPressurePlate.md, SplineBladeTrap.md, HitSlidePlatform.md, BreakablePoleSimple.md, HeroEnterSceneXResponder.md. SuspendedSwayPlat is covered by the existing SuspendedPlatform note.

### Events that should pull the partner to the player who triggered them

1. Slab_03.1 caged-arrival cutscene (door_slabCaged): hero is hidden, parented to a falling cage and autosaved; a partner not in it looks frozen or missing.
2. Slab_16.3 and Slab_16.9 jail Start Inspect (cloaked and cloakless): the hero is slammed into the arena, gates close, camlocks start and battle flags are set.
3. Slab_10b.1 First Weaver summon shrine, and Slab_10b.4 final blow: hero is dashed to the boss, binds, and a scene transition loads Memory_First_Sinner.
4. Slab_16b.4 Broodmother nest battle: boss and its brood spawn only once a hero is in the nest arena.
5. Library_02.2 Door Closer and Library_02.5 Scholar battle: closes gates and camlocks on one hero only.
6. Library_13.3 and Library_13.4 Trobbio and Tormented Trobbio: Start Range trigger, intro dialogue and camlock for one hero.
7. Library_09.3 Garmond Scene: hero entering the range starts a scripted fight.
8. Library_08.1 Librarian wake: StartRoarLock stuns the local hero; Library_10.2 statue wake stuns too.

### Other choices and gaps

1. Crest mismatch: Slab_16.1 picks the arena content from the local Cloakless crest and entry gate, so players with different crests load different content. Decide whether the scene follows the first entrant or each player.
2. Different progress: Library_02.1, Library_13.2, Library_14.1 and Slab_16.1 select scene content from PlayerData flags. Decide shared or per-player flags.
3. One-time rewards (Slab_02.1, Slab_04.1, Slab_17.1, Slab_Cell.1, Slab_16.5, Library_02.3, Library_10.1, Library_13.8, Library_13b.1): decide whether the collector or both get the item, and make the pickup disappear in both games.
4. Shared alarm: Slab_14.1, Slab_22.2 and Slab_03.4 send ALARM and ALARM RING events that wake all slab enemies; decide if one player's alarm wakes the other's enemies (recommended).
5. Persistent break and press events (Slab_10c.1 rings, Slab_04.2 platform, Slab_05.3 and Slab_13.1 plates, Library_01.5 slide platforms, Slab_16.11 jail gate) all need a single shared hit or press event.
6. Lever dependency: Library_15.2 and Library_15.6 depend on a Lever_tk2d (Song_lever_side) not cataloged here; make sure that lever hit is shared.
7. NPC state: Grindle (Slab_03.7, Slab_14.4, Slab_20.1, grindleSlabSequence), Librarian relic board (Library_08.1), City Merchant (Library_03.1) share PlayerData flags; decide per-player or shared dialogue progress.
8. Battle scenes (Slab_16 waves, Slab_16b brood, Library_02 waves) rely on the BattleScene component; the enemies listed above only need hero-target decisions on top.
