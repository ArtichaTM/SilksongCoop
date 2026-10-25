# Audit 2.09: Shellwood_01 .. Shellwood_Witch and Room_Caravan_Interior .. Room_Witch (catalog/Shellwood.md, catalog/Room.md)

Verdict key: sure = coop must handle it, maybe = depends on a design decision or only a mild desync, no = local or cosmetic.
Unless stated otherwise, "the other player" means the player whose game did not run the trigger. Each player runs a full game, so a hero-reading FSM in each game reads its own local hero.
Rows marked "follows X" add nothing to sync beyond the row they follow.

## Shellwood_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_01.1 | Pilgrim Fisher Enemy ×3 (normal and Black Thread variants) | FSM [Control] 57986fdf42 | Flying fisher, silk-possessed, throws projectiles at hero | sure | Alert, possession (writes PlayerData pilgrimFisherPossessed), chase and aimed throw all read the local hero, so two copies fight different players. | The possession trigger (ALERT or TOOK DAMAGE) and the throw start with its aim; movement and death follow. | Which hero a shared fisher targets: nearest, first-aggro, or each game its own copy. | scenes/Shellwood_01.fsm.txt:291 |
| Shellwood_01.2 | Shellwood Goomba ×3 | FSM [Control] f01e5e983c | Hidden crawler wakes near hero, roof-drop or emerge, hurts | sure | Wake (alert range, needolin), facing and the random emerge animation depend on the local hero; BATTLE START makes them fall in a wave. | Wake or emerge event and death. | Same targeting choice as Shellwood_01.1. | scenes/Shellwood_01.fsm.txt:1560 |
| Shellwood_01.3 | Pilgrim Fisher NPC Walker | FSM [Behaviour] 1ba193abab; C# CrossSceneWalker; C# SplineWalker; C# RandomSpeechNPC | Fisher NPC walks a path on a global timer, talks | maybe | Position comes from PlayerData FisherWalkerTimer ticking per game, so the NPC stands at different spots; talking pauses only the local walker. | Nothing, unless the timer value should match once at scene entry. | | scenes/Shellwood_01.fsm.txt:1834, scenes/Shellwood_01.objects.txt:2878 |
| Shellwood_01.4 | Pilgrim Fisher NPC Walker (voice and range helpers) | FSM [Walking Voice Control] d21ad5fa5b; C# InteractableActiveRange; C# EnviroRegionListener | Humming audio, interact range, environment region | no | Audio and local interact gating only. | | | scenes/Shellwood_01.fsm.txt:1860, scenes/Shellwood_01.objects.txt:2878 |
| Shellwood_01.5 | Pond Skater ×3 | FSM [Control] 9fc826c4f0 | Water skimmer, submerges, dies, respawns after 8 s | no | Random wandering is cosmetic, it respawns, and the only side effects are shell shards for the killer and PlayerData hasKilled. | | | scenes/Shellwood_01.fsm.txt:1868 |
| Shellwood_01.6 | Corpse Shellwood Goomba(Clone) ×19 | FSM [Custom Corpse] 54d88ca8b3 | Corpse chunks after enemy death | no | Cosmetic debris. | | | scenes/Shellwood_01.fsm.txt:2128 |
| Shellwood_01.7 | Big Flower | FSM [Control] 9d33eb64f9 | Flower hit several times, bursts, persistent | maybe | Strikes and the hit counter run in one game only; the other game keeps an unbroken flower and a different hit count. | The breaking hit (DESTROY); wobble and pickup drop replay natively. | | scenes/Shellwood_01.fsm.txt:2369 |
| Shellwood_01.8 | Nectar Pickup | FSM [Control] f6001406b4 | Falling nectar collected on touch, gives Shell Flower | sure | Collected by whichever hero touches it in each game, so the item can be duplicated or only given to one player. | The collect event, so the other copy vanishes. | Item goes to the collector only, or both players get one. | scenes/Shellwood_01.fsm.txt:2462 |
| Shellwood_01.9 | Shellwood Wasp ×13 (free and hive) | FSM [Control] 0d3da9a556 | Flying wasp, chases hero, charge or shoot, returns to hive | sure | Alert, chase target, charge aim and shoot angle read the local hero. | Alert event, charge or shoot start with its direction, death. | Same targeting choice as Shellwood_01.1. | scenes/Shellwood_01.fsm.txt:2515 |
| Shellwood_01.10 | Shell Shard Fossil Shellwood ×2 | FSM [Control] 88df492f6c | One hit drops it, breaks into shell shards, persistent | maybe | Hit in one game only; the other game keeps the fossil and shards are currency that only the breaker's game spawns. | The first hit (HIT). | Shards for the breaker only, or each player gets a share. | scenes/Shellwood_01.fsm.txt:2995 |
| Shellwood_01.11 | shellwood_plat_float_thin ×11 | FSM [lift_platform] dc827aaf8d | Platform dips when something lands | no | Cosmetic bob triggered by the local landing. | | | scenes/Shellwood_01.fsm.txt:4351 |
| Shellwood_01.12 | Shellwood Hive | FSM [Destroy] 5fa9253d71; FSM [Control] f98fd213ec; FSM [DAMAGE EFFECTS] c2f9a602b4 | Hive with HP, spawns wasps at HP steps, destroyed persistent | sure | Wasp waves spawn at 70, 50 and 35 percent HP and the final break (journal kill, camlock off, mask fade, 1 or 2 random wasps) happens only in the game that dealt the damage. | Hive hits, or at least the destroy event with its random wasp count. | Each game keeps its own wasps, or the hive owner's wasps are the only ones. | scenes/Shellwood_01.fsm.txt:4609, scenes/Shellwood_01.fsm.txt:4654, scenes/Shellwood_01.fsm.txt:4784 |

## Shellwood_01b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_01b.1 | Battle Gate Shellwood ×3 | FSM [BG Control] 85dcbd0e22 | Arena gates close on BG CLOSE, open on BG OPEN | sure | The arena is a persistent BattleScene with camera locks: only the hero who enters is locked in, and the end only opens gates in the game that finished it. | Battle start and battle end; gates, camlocks and the persistent flag replay natively. | Pull the partner in and lock both, or leave the partner free. | scenes/Shellwood_01b.fsm.txt:4 |
| Shellwood_01b.2 | Flower Drifter ×29 (waves 1 and 3) | FSM [Control] 76e33931de | Floating flower, chases hero, slow attack, can sing | sure | Alert range, chase and attack read the local hero; wave members are separate copies per game. | Which hero it chases and the attack start; movement and death follow. | Which hero a shared drifter chases: nearest, first-aggro, or each game its own copy. | scenes/Shellwood_01b.fsm.txt:136 |
| Shellwood_01b.3 | Stick Insect ×4 and Stick Insect Charger ×2 | FSM [Behaviour Base] 73f410ae25; FSM [Attack] 414bff2008, ce26ab3861 | Hidden insect bursts out, runs at hero, attacks or charges | sure | Awake trigger, facing, run direction and attack all read the local hero; the BATTLER flag skips hiding so each game's wave starts on its own. | Wake or burst-out event, attack or charge start with direction, death. | Same targeting choice as Shellwood_01b.2. | scenes/Shellwood_01b.fsm.txt:862, scenes/Shellwood_01b.fsm.txt:762, scenes/Shellwood_01b.fsm.txt:1048 |
| Shellwood_01b.4 | Corpse Stick Insect(Clone) ×10 | FSM [Rotater] fe353dc1b4, 54c9d57966 | Corpse spins after death | no | Cosmetic. | | | scenes/Shellwood_01b.fsm.txt:1475, scenes/Shellwood_01b.fsm.txt:3073 |
| Shellwood_01b.5 | Bloom Puncher ×16 | FSM [Control] f151d0ace6; FSM [FSM] 8eb1822ca4 (Lift Range) | Fixed plant, punches when hero in range, drops head on death | maybe | Punch triggers on the local hero's range, so the partner is never punched in the other game; killing it in one game leaves it alive for the other. | Its death (ZERO HP). | | scenes/Shellwood_01b.fsm.txt:4711, scenes/Shellwood_01b.fsm.txt:5083 |

## Shellwood_02

no new elements

## Shellwood_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_03.1 | Bloom Shooter ×11 | FSM [Control] 3f36991b97 | Fixed plant, fires 3 pollen shots at hero | maybe | Shot angle reads the local hero with random jitter, so the partner is never shot at in the other game. | Nothing needed for damage; share the shot start only if the shots must be visible. | | scenes/Shellwood_03.fsm.txt:4 |

## Shellwood_04b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_04b.1 | Barrel Plat Lift Thin ×22 (and Barrel Plat Lift) | FSM [Control] c2a28ced66 | Platform rises or lowers to Raise Y on OPEN | sure | Moves with velocity and carries the hero; the sender of OPEN is not in the dump, so if only one game is triggered the platform sits at different heights. | The OPEN trigger (whatever sends it); the move replays natively. | | scenes/Shellwood_04b.fsm.txt:91 |
| Shellwood_04b.2 | Hero Land ×20 | FSM [Land] 72bb5328b0 | Platform and chain dip when hero lands | no | Cosmetic. | | | scenes/Shellwood_04b.fsm.txt:125 |
| Shellwood_04b.3 | Shellwood Twig Wall (1) ×2 | FSM [Break for Courier] 0dedba59f2 | Wall removed once quest Save Courier Short is tracked | maybe | Depends on the player's own quest state, so a partner without the quest still sees the wall. | The wall's ACTIVATED (broken) state, or share the quest. | Quest per player, or accepting it for both. | scenes/Shellwood_04b.fsm.txt:3845 |

## Shellwood_04c

no new elements

## Shellwood_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_08.1 | Silk Possession Obj Sing ×8 | C# ActivateObjectDelay; C# SetRotationThreadPossess | Silk string visuals for singing or possession | no | Pure visual effect. | | | scenes/Shellwood_08.objects.txt:777, scenes/Shellwood_08.objects.txt:779 |

## Shellwood_08c

no new elements

## Shellwood_10

no new elements

## Shellwood_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_11.1 | Remasker New Twig Wall | FSM [Wait For Disable] 3166ca7deb | Hides mask when vines were cut | no | Reads a vines-cut broadcast; visual only, follows the vine cut. | | | scenes/Shellwood_11.fsm.txt:1274 |

## Shellwood_11b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_11b.1 | Silkfly Control and Silkfly Ambient NP ×21 | FSM [Control] 5499d5fb5d, a5e7a59531; FSM [FSM] 1becd1f9a2 | Ambient silkflies, set by entry gate | no | Ambience chosen from the local entry gate. | | | scenes/Shellwood_11b.fsm.txt:278, scenes/Shellwood_11b.fsm.txt:296, scenes/Shellwood_11b.fsm.txt:302 |

## Shellwood_11b_Memory

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_11b_Memory.1 | Boss Scene | FSM [Control] cab2d41759 | Memory fight intro: roar stuns hero, floor breaks, camlock, roof, start | sure | Forced cutscene on the entering hero (roar stun, hero X clamped, camera locked, writes encounteredFlowerQueen); the other player is in another scene. | Fight start. | The memory is a separate scene: partner stays out, is pulled in, or watches. | scenes/Shellwood_11b_Memory.fsm.txt:4 |
| Shellwood_11b_Memory.2 | Boss Scene | FSM [Scene End] 9e10580afe | After boss death: white fade, return to Shellwood_11b | sure | Scene transition for the local hero only; a pulled-in partner has to return too. | Boss defeat, to return everyone. | Return the partner with the hero or let each leave alone. | scenes/Shellwood_11b_Memory.fsm.txt:145 |
| Shellwood_11b_Memory.3 | Flower Queen Boss | FSM [Control] 880191b47a | Boss: walk, pollen shots, leap, spike summon, slash, two phases | sure | Random attack choice, shot angle and leap target read the local hero. | Boss attack choice with its target; phase shift and HP. | Which hero the boss targets: nearest, first-aggro, or each game its own copy. | scenes/Shellwood_11b_Memory.fsm.txt:156 |
| Shellwood_11b_Memory.4 | Corpse Flower Queen(Clone) | FSM [Death] 5e78ae8301; C# HeroInvincibilitySource | Death sequence, floor break, writes defeatedFlowerQueen | sure | Writes PlayerData defeatedFlowerQueen and breaks the arena only in the winner's game; the source makes only the local hero invulnerable. | Boss defeat (BOSS DEFEAT). | | scenes/Shellwood_11b_Memory.fsm.txt:668, scenes/Shellwood_11b_Memory.objects.txt:646 |
| Shellwood_11b_Memory.5 | NPC (on the Flower Queen corpse) | FSM [Control] 78a4b44e45 | Hornet leaps, grabs and pulls out the queen's heart | sure | Forced finisher: hero hidden, made kinematic and moved to a dummy for several seconds. | The finisher start, then BOSS DEFEAT. | Freeze the partner, leave free, or show the partner's figure in the cutscene. | scenes/Shellwood_11b_Memory.fsm.txt:727 |
| Shellwood_11b_Memory.6 | Respawn Plat | FSM [Control] 234d968d0e | Platform appears after the local hero's spike respawn | no | Reacts to the local hero's spike hit only. | | | scenes/Shellwood_11b_Memory.fsm.txt:1841 |
| Shellwood_11b_Memory.7 | Spike Balls, Spike Balls P1 and Flower Queen Spike ×8 | FSM [Control] f7c07ddc9a, e61b617646, 8d1510631b | Spike balls erupt at shuffled positions on SPIKE ATTACK | sure | Position order is a random shuffle, so two games show different spikes. | Nothing extra if the boss attack carries the shuffle result. | | scenes/Shellwood_11b_Memory.fsm.txt:1880, scenes/Shellwood_11b_Memory.fsm.txt:1917, scenes/Shellwood_11b_Memory.fsm.txt:1946 |
| Shellwood_11b_Memory.8 | Plat ×6 (Terrain Phase 1) | FSM [Jitter] 594132e030 | Platforms shake before floor break | no | Cosmetic. | | | scenes/Shellwood_11b_Memory.fsm.txt:2898 |
| Shellwood_11b_Memory.9 | Wall Spike L and Wall Spike R | FSM [Control] 755b87eab8 | Brambles rise or drop on boss events | sure | Damaging hazards driven by boss attack events; follows Shellwood_11b_Memory.3. | | | scenes/Shellwood_11b_Memory.fsm.txt:2934 |
| Shellwood_11b_Memory.10 | door_wakeInMemory ×5 | FSM [Wake Up] 3ce4dc7694 | Entry: take control, prostrate, fade up, give control back | maybe | Local entry cutscene that holds the arriving hero for seconds. | | Whether an arriving partner replays it. | scenes/Shellwood_11b_Memory.fsm.txt:3008 |
| Shellwood_11b_Memory.11 | Exit Edge Trigger ×5 | C# SceneTransitionZone | Edge trigger to another scene | no | Ordinary scene exit for the hero who touches it. | | | scenes/Shellwood_11b_Memory.objects.txt:441 |
| Shellwood_11b_Memory.12 | Legs Idle, Legs Walk, Intro Flower Queen Legs | C# RemapTk2DSpriteAnimator | Sprite animation remap | no | Visual. | | | scenes/Shellwood_11b_Memory.objects.txt:595 |

## Shellwood_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_13.1 | Bell Wall Tall ×3 | C# ProjectileBreakTrigger | Trigger that destroys projectiles entering it | no | Only breaks the projectile (the wall is not broken by it). | | | scenes/Shellwood_13.objects.txt:2195 |

## Shellwood_14

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_14.1 | Ambush Scene | FSM [Control] 82bc70f396 | After flea rescue or heart piece, enemies burst in from up and down | sure | The trigger is a pickup collected by one player; the ambush (writes shellwood14_ambushed) only starts in that game, and its enemies then chase that hero. | The ambush start (FLEA RESCUED or HEART PIECE COLLECTED). | Ambush both players, or only the collector. | scenes/Shellwood_14.fsm.txt:4 |

## Shellwood_15

no new elements

## Shellwood_16

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_16.1 | Scene Control | FSM [Control] affbaeab71 | Picks scene variant from PlayerData, sets seenEmptyShellwood16 | no | Read once at scene load; PlayerData should already match. | | | scenes/Shellwood_16.fsm.txt:80 |

## Shellwood_18

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_18.1 | Boss Return Scene (Bud Control, Bud 1, Bud 2, Bud 3) | FSM [Bud Control] 6bc5c8a86f; FSM [Control] 45c61859d7, 2bc32a8c8b | Sprout grows by timer, a nail slash cuts it | maybe | Cutting writes splinterQueenSproutCut in one game only; the other game keeps the sprout. | The cut (SLASH). | | scenes/Shellwood_18.fsm.txt:28, scenes/Shellwood_18.fsm.txt:53, scenes/Shellwood_18.fsm.txt:83 |
| Shellwood_18.2 | Hunter Fan Outside ×2 | FSM [Dialogue] 29e295df67 | NPC chat, waves, can be slapped | no | Talk is per player; only PlayerData nuuSlappedOutside differs. | | | scenes/Shellwood_18.fsm.txt:100 |
| Shellwood_18.3 | Boss Scene | FSM [Battle Control] ce2768181b | Entering range locks camera and starts the Splinter Queen fight | sure | Only the hero entering the range gets the camlock and starts the fight; the partner is unlocked and the fight is not running for them. | Fight start and BOSS DEFEAT (gates, camlock release, cleanup). | Pull and lock the partner, or leave the partner free. | scenes/Shellwood_18.fsm.txt:219 |
| Shellwood_18.4 | Approaches | FSM [Control] 50b029d4aa | Chooses left or right entrance by hero side at load | sure | Each game picks the arena side from its own hero position, so the gate sides can differ. | | | scenes/Shellwood_18.fsm.txt:260 |
| Shellwood_18.5 | Splinter Queen Gate Spike ×80 and RoofShaker Idle ×89 | FSM [Control] 8a1ce40f87; FSM [Control] ea1bfbd412 | Spear gate closes behind the hero, tinks, shatters on defeat | sure | The gate closes only in the game of the hero who walks in; on defeat GATE SPIKES DESTROY runs in the winner's game only. | Follows Shellwood_18.3. | | scenes/Shellwood_18.fsm.txt:281, scenes/Shellwood_18.fsm.txt:431 |
| Shellwood_18.6 | Audio Loop Tension, Antic Audio Control | FSM [FSM] f809c7c239; FSM [Antic Audio Control] 00c134a9bf | Boss tension music and antic sound | no | Audio only. | | | scenes/Shellwood_18.fsm.txt:7481, scenes/Shellwood_18.fsm.txt:7581 |
| Shellwood_18.7 | Splinter Queen Spike ×6 | FSM [Control] 231a1b644a; FSM [Damage Effects] 95d467d080 | Summoned spike with HP, shatters on spell, explosion or heavy damage | sure | Which spike and where is chosen at random by the queen; two games show different spikes. | Follows Shellwood_18.8 (spike choice and position). | | scenes/Shellwood_18.fsm.txt:7599, scenes/Shellwood_18.fsm.txt:7590 |
| Shellwood_18.8 | Splinter Queen | FSM [Control] c018c89181 | Boss: slam, roar and summon, spikes, two phases | sure | Slam target X reads the local hero; roar summon and spike positions are random. | Attack choice with its target and positions; phase and HP. | Which hero the boss targets: nearest, first-aggro, or each game its own copy. | scenes/Shellwood_18.fsm.txt:8475 |
| Shellwood_18.9 | Corpse Splinter Queen(Clone) | FSM [Death] a0e83402c2 | Death: cleanup spikes and minions, BOSS DEFEAT | sure | Cleanup and defeat run only in the winner's game, leaving the partner's arena closed. | Boss defeat. | | scenes/Shellwood_18.fsm.txt:9128 |
| Shellwood_18.10 | Stick Insect Flyer ×6 | FSM [Control] 7824bafe67; FSM [Spin Recoil] 5d992335c0, 117b6240da | Summoned flyer chases hero, spin attack | sure | Chase target, offset and spin direction read the local hero. | Summon event and spin start; movement and death follow. | Same targeting choice as Shellwood_18.8. | scenes/Shellwood_18.fsm.txt:9221, scenes/Shellwood_18.fsm.txt:9199, scenes/Shellwood_18.fsm.txt:9904 |
| Shellwood_18.11 | Corpse Stick Insect Flyer(Clone) ×6 | C# CorpseChunker | Corpse breaks into chunks | no | Cosmetic. | | | scenes/Shellwood_18.objects.txt:143 |

## Shellwood_19

no new elements

## Shellwood_20

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_20.1 | Seth Stand NPC ×2 | FSM [Dialogue] 442bcd62c9 | Seth chat, meet or repeat, writes MetSethNPC | no | Talk is per player; only the MetSethNPC flag differs. | | | scenes/Shellwood_20.fsm.txt:150 |

## Shellwood_22

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_22.1 | Flower Gate ×2 (Boss Scene and Group) | FSM [Control] 54f6f8654e, e495d6680b | Vine wall breaks when Seth is defeated | sure | Break plays only where SETH DEFEATED ran; the other game keeps its exit blocked. | Follows Shellwood_22.2 (SETH DEFEATED). | | scenes/Shellwood_22.fsm.txt:23, scenes/Shellwood_22.fsm.txt:2955 |
| Shellwood_22.2 | Seth | FSM [Control] dad5f6b0f1 | Optional boss: offer, roar, slash combos, dive, shield throws, teleports | sure | The fight and its offer dialogue (yes or no, hero stunned by roar, writes encounteredSeth) run for the hero in the battle range; all attacks aim at the local hero. | Offer accept and fight start; attack choice with target; phase and HP. | Who answers the offer, pull the partner in, and which hero Seth targets. | scenes/Shellwood_22.fsm.txt:41 |
| Shellwood_22.3 | Seth | FSM [Shield Collider] b4d8d5d712; FSM [Stun] 43565ca2f5; FSM [Phase Control] 2050e94a2f | Shield by facing, stun gauge, phase at 75 and 40 percent HP | sure | Facing is checked against the local hero and hits accumulate per game, so stun and phases diverge. | Follows Seth's HP and damage (share hits). | | scenes/Shellwood_22.fsm.txt:1321, scenes/Shellwood_22.fsm.txt:1333, scenes/Shellwood_22.fsm.txt:1426 |
| Shellwood_22.4 | Seth | FSM [Circle Slash Catch] cb6cb5a7cd | Slash catches hero and moves them to a target | sure | Moves the local hero's position for the catch; the other game's copy of that player is not moved. | The catch start with its target position. | | scenes/Shellwood_22.fsm.txt:1457 |
| Shellwood_22.5 | Corpse Seth(Clone) | FSM [Death] 7d1e588ddd | Death, sends SETH DEFEATED | sure | Defeat plays only for the winner. | Boss defeat. | | scenes/Shellwood_22.fsm.txt:1481 |
| Shellwood_22.6 | Shield Projectile | FSM [Control] a02e19311c | Thrown shield flies, bounces off floor and walls | sure | Aimed at the local hero when thrown. | Follows the Seth throw (angle). | | scenes/Shellwood_22.fsm.txt:1978 |

## Shellwood_25

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_25.1 | door1 | FSM [FSM] aebb2ebcb6 | On entry sets bench respawn here and saves game | no | Per-player respawn and save; each player's own entry does it. | | | scenes/Shellwood_25.fsm.txt:12 |
| Shellwood_25.2 | Witch Chapel Door Control | FSM [Control] 883499f6b5 | Closes the door behind a hero entering via door1 | no | Local door visual by entry gate. | | | scenes/Shellwood_25.fsm.txt:2409 |
| Shellwood_25.3 | Witch_Cluster_Vine ×3 | FSM [Control] ba0ce090e3 | Vine takes 4 nail hits, then breaks (persistent) | maybe | Hits and the break (BROKEN, stumps) happen in one game; the other game keeps the vine. | The final break. | | scenes/Shellwood_25.fsm.txt:2426 |

## Shellwood_25b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shellwood_25b.1 | Atmos Control | FSM [FSM] 47d8130ecb | Audio snapshot if gainedCurse | no | Audio. | | | scenes/Shellwood_25b.fsm.txt:9 |
| Shellwood_25b.2 | door_curseSequenceEnd | FSM [Curse Sequence] c8e5acc3f3 | Hero bound, mash to break free, curse gained, autosave | sure | Forced cutscene: removes control, hides hero, writes gainedCurse and bindCutscenePlayed, blocks silk regen, saves; the partner would not get the curse. | The sequence start and its end. | Curse both players, freeze the partner, or keep it per player. | scenes/Shellwood_25b.fsm.txt:19 |

## Shellwood_26

no new elements

## Shellwood_Witch

no new elements

## Room_Caravan_Interior

no new elements

## Room_Caravan_Interior_Travel

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_Caravan_Interior_Travel.1 | _SceneManager | FSM [map_isroom] 2a992690a1 | Deprecated, disables itself | no | Dead code. | | | scenes/Room_Caravan_Interior_Travel.fsm.txt:4 |
| Room_Caravan_Interior_Travel.2 | _SceneManager | FSM [Cutscene Control] 27664f4449; C# InGameCutsceneInfo | Caravan ride: fade, skip prompt, then transition to troupe's scene | sure | Fast-travel scene transition driven by the local PlayerData CaravanTroupeLocation; the partner stays behind or lands elsewhere. | The ride start and its destination. | Partner rides along, stays, or travels separately. | scenes/Room_Caravan_Interior_Travel.fsm.txt:9, scenes/Room_Caravan_Interior_Travel.objects.txt:88 |
| Room_Caravan_Interior_Travel.3 | Caravan Troupe Flea Travel Air ×5 | FSM [Behaviour] 1842626108 | Fleas buzz, react to hero and needolin | no | Ambient. | | | scenes/Room_Caravan_Interior_Travel.fsm.txt:39 |
| Room_Caravan_Interior_Travel.4 | Hornet Spider Travel | FSM [Control] 566d77ccfe | Hornet sits idle, plays needolin | no | Animation control of the local hero during the ride. | | | scenes/Room_Caravan_Interior_Travel.fsm.txt:214 |
| Room_Caravan_Interior_Travel.5 | Jostle Parts | FSM [FSM] 85fabe7418; C# JostleChildren | Cart parts jostle at random | no | Cosmetic. | | | scenes/Room_Caravan_Interior_Travel.fsm.txt:253, scenes/Room_Caravan_Interior_Travel.objects.txt:56 |

## Room_Caravan_Spa

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_Caravan_Spa.1 | Caravan Lech Spa Peek | FSM [Lech Peek] 097bd311ae | Lech peeks out, hit sets CaravanLechSpaAttacked | no | Easter egg that hides when looked at; one PlayerData flag. | | | scenes/Room_Caravan_Spa.fsm.txt:4 |
| Room_Caravan_Spa.2 | left1 | FSM [Set Target Scene] c43cf67434 | Exit leads to the troupe's current scene | maybe | Destination comes from the local CaravanTroupeLocation, so players leave to different places. | CaravanTroupeLocation if the troupe must match. | | scenes/Room_Caravan_Spa.fsm.txt:66 |

## Room_CrowCourt

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_CrowCourt.1 | _SceneManager | FSM [Is Room] 82e777ffa0 | Deprecated, disables itself | no | Dead code. | | | scenes/Room_CrowCourt.fsm.txt:4 |
| Room_CrowCourt.2 | Balloon Raiser and CamLock Balloon | FSM [Control] 32af1de709; FSM [Bot Balloons] 20c0d1dcd5; FSM [FSM] a534762c4d | After battle raises balloon platforms, lever hit frees camlock | sure | Raises only where BATTLE END ran, reads hitCrowCourtSwitch, and CROW LEVER HIT only disables the local camlock. | Battle end and the crow lever hit. | | scenes/Room_CrowCourt.fsm.txt:1695, scenes/Room_CrowCourt.fsm.txt:1720, scenes/Room_CrowCourt.fsm.txt:1731 |
| Room_CrowCourt.3 | greymoor_flip_bridge_BG | FSM [BG Control] 1392a567fb | Trapdoor bridge opens or closes with battle state | sure | Opens on BG OPEN from the battle in one game only. | Follows the battle end. | | scenes/Room_CrowCourt.fsm.txt:1737 |
| Room_CrowCourt.4 | court_door | FSM [Door] 0a2f174416 | Spends a Craw Summons item to open the court door (OpenedCrowSummonsDoor) | sure | The item is taken from one player's inventory and the door opens only there. | The door open (OpenedCrowSummonsDoor). | Item spent by the opener only, or the door opens for both. | scenes/Room_CrowCourt.fsm.txt:9093 |
| Room_CrowCourt.5 | Crowman Juror Crowd Idle ×32 | FSM [Control] 09ea57fba8 | Crowd cheers, reacts to Crawfather death | no | Ambient. | | | scenes/Room_CrowCourt.fsm.txt:9204 |
| Room_CrowCourt.6 | Support Plat terrain collider ×8 | FSM [depress_plat] b74f7508d0 | Platform dips on landing, resets on hazard respawn | no | Cosmetic, local landing. | | | scenes/Room_CrowCourt.fsm.txt:11702 |

## Room_CrowCourt_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_CrowCourt_02.1 | Battle Scene | FSM [Revisit] 441a57e40a | If defeatedCrowCourt, unparents floor and gate | no | Scene-load check of PlayerData. | | | scenes/Room_CrowCourt_02.fsm.txt:41 |
| Room_CrowCourt_02.2 | Battle Start | FSM [Battle Start] 86455d4bee | Trigger: close gates, lights, crowd roar stuns hero, boss title, waves | sure | The entering hero is stunned by the roar and locked in; the partner's game has no fight. | Fight start (BG CLOSE then BATTLE START). | Pull the partner into the court or leave them free. | scenes/Room_CrowCourt_02.fsm.txt:282 |
| Room_CrowCourt_02.3 | Court Floor | FSM [Floor Shove] 152c408cfe | Floor jolts up when Crawfather pecks | maybe | Raises a solid floor the hero stands on; follows Room_CrowCourt_02.8. | | | scenes/Room_CrowCourt_02.fsm.txt:337 |
| Room_CrowCourt_02.4 | Crowman Dagger Juror ×4 | FSM [Control] 9b41b8d333 | Crow juror: drops, hops, slashes, flies | sure | Drop and attack choice read the local hero; waves are separate copies per game. | Attack start with direction and death. | Which hero a shared juror targets: nearest, first-aggro, or each game its own copy. | scenes/Room_CrowCourt_02.fsm.txt:352 |
| Room_CrowCourt_02.5 | Crowman Juror ×4 (pin wielder) | FSM [Control] f947d9e18d | Roosting juror wakes, wildslash and charge | sure | Wake range and attack aim read the local hero. | Same as Room_CrowCourt_02.4. | Same targeting choice. | scenes/Room_CrowCourt_02.fsm.txt:797 |
| Room_CrowCourt_02.6 | Crowman Juror Tiny ×6 | FSM [Behaviour] 74ff8b580c | Small crow, roosts, launches at hero | sure | Alert, launch and random speed read the local hero. | Same as Room_CrowCourt_02.4. | Same targeting choice. | scenes/Room_CrowCourt_02.fsm.txt:1386 |
| Room_CrowCourt_02.7 | Wave Starter ×3 (waves 1, 3, 5) | FSM [Control] 9d0b29b6fc, 3b85e8feca, fc2ca148a8 | Starts its wave on BATTLE START | sure | Waves advance when each game's own enemies die, so progress diverges. | Wave advance (BATTLE START per wave) from one shared kill count. | | scenes/Room_CrowCourt_02.fsm.txt:5488, scenes/Room_CrowCourt_02.fsm.txt:7230, scenes/Room_CrowCourt_02.fsm.txt:9269 |
| Room_CrowCourt_02.8 | Crawfather | FSM [Control] 20eedb9b19; FSM [Flap Sfx] 3f9e318b97 | Boss: peck, evade, chase, fly, dive, chain attack | sure | Range checks, peck and dive aim, run-back read the local hero. | Attack choice with target; HP. | Which hero the boss targets: nearest, first-aggro, or each game its own copy. | scenes/Room_CrowCourt_02.fsm.txt:9287, scenes/Room_CrowCourt_02.fsm.txt:9937 |
| Room_CrowCourt_02.9 | Crawfather Attack Chain ×6 | FSM [Control] 65b66d13b3; FSM [fornet_multi_wounder] c46feccfee | Chain strikes ground, multi-hit damager on the hero | sure | Chain aim follows the boss attack; the damager hits only the local hero. | Follows Room_CrowCourt_02.8. | | scenes/Room_CrowCourt_02.fsm.txt:9949, scenes/Room_CrowCourt_02.fsm.txt:10035 |
| Room_CrowCourt_02.10 | Corpse Crawfather(Clone) | FSM [Death] 0323644d36 | Death: CRAWFATHER DEFEATED, crowd roar, floor shove | sure | Defeat ends the battle only in the winner's game. | Boss defeat. | | scenes/Room_CrowCourt_02.fsm.txt:10873 |

## Room_Diving_Bell

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_Diving_Bell.1 | Travel Sequence Control | FSM [Sequence] a9da6e6f76 | Bench or travel: hero grabbed, cinematic descent, scene change | sure | Forced sequence and scene transition for the sitting hero; the partner stays in the bell. | Travel start with destination. | Partner rides along, stays, or travels separately. | scenes/Room_Diving_Bell.fsm.txt:4 |
| Room_Diving_Bell.2 | Gramaphone Interact ×2 | FSM [Dialogue] 096976b4f0; C# NPCEventResponder | Dialogue toggles bell between bench and travel mode | maybe | Mode lives in the FSM, so one player's switch does not reach the other. | | Mode per player or shared. | scenes/Room_Diving_Bell.fsm.txt:178, scenes/Room_Diving_Bell.objects.txt:107 |
| Room_Diving_Bell.3 | Camera Rotator ×3 | C# SetGameCameraRotation | Rotates the game camera during descent | no | Local camera. | | | scenes/Room_Diving_Bell.objects.txt:100 |

## Room_Diving_Bell_Abyss

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_Diving_Bell_Abyss.1 | Wake Up Scene | FSM [Dialogue] c78314ab31 | Arrival: black dialogue, prostrate, get up, save, bench respawn | sure | Forced cutscene on arrival; sets visitedAbyss and a per-player respawn. | The arrival, for a partner who arrives later. | Replay it for the partner, skip it, or hold them. | scenes/Room_Diving_Bell_Abyss.fsm.txt:16 |

## Room_Diving_Bell_Abyss_Fixed

no new elements

## Room_Forge

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_Forge.1 | Forge Daughter | FSM [Dialogue] 6ad8ed1ca9 | NPC and shop, writes MetForgeDaughter | maybe | Talk is per player; shop stock limits (outside this FSM) may be shared. | | Shop per player or shared. | scenes/Room_Forge.fsm.txt:53 |
| Room_Forge.2 | Idle Work Loop | FSM [Range Control] a2213993e6 | Work loop shown while hero is above a height | no | Reads local hero Y; cosmetic. | | | scenes/Room_Forge.fsm.txt:507 |
| Room_Forge.3 | Forge Worker | FSM [Effects] 3d2310b3f8; FSM [Combo Hit Control] b08f36e15e; C# SendEventToRegisterOnDisable | Shovel rock, laugh on hits, disable event | no | Cosmetic reactions. | | | scenes/Room_Forge.fsm.txt:517, scenes/Room_Forge.fsm.txt:661, scenes/Room_Forge.objects.txt:260 |
| Room_Forge.4 | Forge Worker | FSM [Dialogue] 565d70c3b4 | Dialogue, sets BallowMovedToDivingBell | maybe | Talk is per player; one PlayerData flag. | | | scenes/Room_Forge.fsm.txt:534 |
| Room_Forge.5 | Extra Scene Doors | FSM [Control] 4ce7ca7648 | After battle raises pole, retracts trapdoor levers | sure | Runs on BATTLE END or COMPLETED only in the game that finished the battle. | Follows the battle end. | | scenes/Room_Forge.fsm.txt:732 |
| Room_Forge.6 | song_knight_lock | FSM [Smash] a7e0b6c076 | Item unlock makes sealed dock gate burst | sure | One player uses the item; the gate bursts only in that game. | The UNLOCK event. | Item spent by the user only, or unlocked for both. | scenes/Room_Forge.fsm.txt:3760 |

## Room_Huntress

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_Huntress.1 | Huntress Runt | FSM [Dialogue] 9920b49c6d | Meets runt, begins Huntress Quest Runt | maybe | Starts a quest in the talker's save only. | | Quest per player or shared. | scenes/Room_Huntress.fsm.txt:804 |
| Room_Huntress.2 | Huntress | FSM [Dialogue] 375b71006b | Meet and repeat dialogue | no | Local talk. | | | scenes/Room_Huntress.fsm.txt:928 |

## Room_Pinstress

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_Pinstress.1 | Clean Point ×4 and Pinstress Interior NPC | FSM [Set Z] 915e2f14a3, d6de43d3bf | Layer depth while resting on bench | no | Visual depth. | | | scenes/Room_Pinstress.fsm.txt:9, scenes/Room_Pinstress.fsm.txt:1309 |
| Room_Pinstress.2 | Pinstress States | FSM [States] 3548383eb9 | Picks Pinstress pose from PlayerData at load | no | Scene-load choice from PlayerData. | | | scenes/Room_Pinstress.fsm.txt:25 |
| Room_Pinstress.3 | Pinstress Interior Ground Sit | FSM [Behaviour] 29eff529c7 | Accept teaching, sparring, writes hasChargeSlash | sure | Grants the Charge Slash ability only to the player who accepts. | The grant (hasChargeSlash). | Grant both players or per player. | scenes/Room_Pinstress.fsm.txt:54 |
| Room_Pinstress.4 | Inspect Region | FSM [Quest Inspect] be4066c8b4 | Inspectable summons, hides once quest Pinstress Battle is tracked | maybe | Depends on the player's own quest state. | | Quest per player or shared. | scenes/Room_Pinstress.fsm.txt:271 |
| Room_Pinstress.5 | Pinstress Interior NPC | FSM [Behaviour] 521b17e012 | Standing NPC cleans, waves, teleports between points | no | Ambient NPC behaviour and local dialogue. | | | scenes/Room_Pinstress.fsm.txt:1145 |

## Room_Witch

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Room_Witch.1 | Pollip Rite Control | FSM [Control] 92c4693474 | Rite shakes camera, particles, burst on end | no | Cosmetic effect started by the witch. | | | scenes/Room_Witch.fsm.txt:9 |
| Room_Witch.2 | Wood Witch | FSM [Dialogue] 03907438ee | Meet, item and rite quests, grabs hero, scene change to curse sequence | sure | Dialogue and quests are per player; the grab hides the hero and sends only them to the curse scene (writes MetWoodWitch, completedMemory_witch). | The grab and transition. | Pull the partner along, leave them, or run it for the talker only. | scenes/Room_Witch.fsm.txt:35 |

## Summary

Cutscenes and transitions that should decide what happens to the partner (each is a choice for the user):

1. Shellwood_25b curse sequence (Shellwood_25b.2, after Room_Witch.2): removes hero control, writes gainedCurse and saves. Choose: curse both, freeze the partner, or per player.
2. Room_Witch.2 witch grab: carries the hero to Shellwood_25b. Choose: pull the partner too or leave them.
3. Shellwood_11b_Memory.1, .2 and .5: memory fight is a separate scene (entered and left by a scene change, with a roar stun and a finisher cutscene). Choose: partner stays out, is pulled in, or follows.
4. Room_Caravan_Interior_Travel.2 and Room_Diving_Bell.1: fast-travel scene transitions (ride or descent cinematic). Choose: partner rides along, stays, or travels separately; Room_Diving_Bell_Abyss.1 then plays a forced wake-up on arrival.

Arenas and bosses (partner is not locked in by default; the end only opens gates in the winner's game):

5. Shellwood_01b (persistent battle with gates, camlocks, wave enemies), Shellwood_18 Splinter Queen, Shellwood_22 Seth (optional, started by an offer dialogue), Room_CrowCourt and Room_CrowCourt_02 Crawfather, Shellwood_11b_Memory Flower Queen. For each, share fight start and fight end; decide pull-in versus leave-free.
6. Boss attack choice, spike positions, summon picks and shield/shot angles are random or read the local hero (Shellwood_11b_Memory.3 and .7, Shellwood_18.7 and .8, Shellwood_22.2 to .6, Room_CrowCourt_02.8 and .9). Sharing the boss's attack decision with its target removes the duplicates.
7. Only the winner gets the end effects (flower gates Shellwood_22.1, gate spears Shellwood_18.5, trapdoors Room_CrowCourt.3, forge doors Room_Forge.5, camlocks): a single shared defeat or battle-end event replays all of them.

Other gaps and choices:

8. Item and ability grants go to one player only: Nectar Pickup (Shellwood_01.8), Craw Summons door (Room_CrowCourt.4), song knight lock (Room_Forge.6), Charge Slash (Room_Pinstress.3). Choose per item whether both players get it.
9. Quest gates read each player's own quest (Shellwood_04b.3, Room_Huntress.1, Room_Pinstress.4); sharing quests or not is a global design choice.
10. Persistent breakables broken in one game only: Big Flower, Shell Shard Fossil, hive, Witch_Cluster_Vine, sprout (Shellwood_01.7, .10, .12, Shellwood_25.3, Shellwood_18.1). Sharing the break event is enough.
11. Shellwood_04b.1 lift is driven by an OPEN event whose sender is not visible in the dump; worth tracing before deciding what to sync.
12. Stationary plants (Bloom Puncher, Bloom Shooter) hit only the local hero in each game, so damage stays correct; only deaths and visible shots differ.
13. New notes written: audit/notes/CrossSceneWalker.md, ProjectileBreakTrigger.md, HeroInvincibilitySource.md.
