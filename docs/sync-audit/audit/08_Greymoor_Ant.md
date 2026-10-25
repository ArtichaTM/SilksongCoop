# Audit 2.08: Greymoor (incl. Halfway Home) and Hunter's March (Ant)

Reading guide. Verdict: sure / maybe / no = does coop need to handle it. Everything that reads `Hero`, an alert range, `CheckHeroPerformanceRegion` (needolin) or a hero trigger only ever sees the local hero, so "wakes", "aggroes", "enters the arena" and "reacts to needolin" all happen per game. Enemy rows say `sure` because each game runs its own copy that picks its own target; the smallest sync for any enemy is "who woke or aggroed it, hits taken, death". Persistent bools and PlayerData flags are written to the save at scene exit or bench, not at the moment of change, so a live room does not see them change.
Notes written: SuspendedPlatform, BounceBalloon, AntRegion, LiftControl, PlayMakerEventRange, ToolGameObjectActivator. Notes used: AlertRange, BattleScene, Gate, Lever, PersistentBoolItem, CheckHeroPerformanceRegion, VinePlatform, ManualLift, BlackThreadState, TestGameObjectActivator, MemoryOrb, Trapdoor, HarpoonRingChain, HealthManager, ShopOwner.

## Greymoor_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_01.1 | Bell Chain Plat | FSM Tink Effects d0c35333a5 | Tink sparks when nail hits | no | Particles only. | | | scenes/Greymoor_01.fsm.txt:4 |
| Greymoor_01.2 | Bell Chain Plat | FSM Drop fb1638ab54 | Lever opens it: rises, then slams down | sure | Runs only where the OPEN message arrives; it enables a Hero Crusher on the way down and ends in a different position, and the platform has a saved bool, so the other player keeps a stuck or raised platform. | The OPEN trigger (the lever hit that releases it); rise, slam and crusher replay natively. | | scenes/Greymoor_01.fsm.txt:24 |
| Greymoor_01.3 | FlyAway Crow ×N (Normal World, Thread Only Variant, Entry Crows, Floor Control) | FSM Control 965e4ffd38 | Perched crow flees when hero is near | no | Cosmetic scenery crows; wake range reads the local hero and each game flies its own. | | | scenes/Greymoor_01.fsm.txt:58 |
| Greymoor_01.4 | crows - inactive if court in session | FSM FSM 1778195f32 | Hides crow group if Crow Court sits | maybe | Decided by PlayerData CrowCourtInSession and blackThreadWorld at load, so players with different flags see different crows. | Nothing live; both games need the same two PlayerData flags. | | scenes/Greymoor_01.fsm.txt:188 |
| Greymoor_01.5 | Crow ×3 (HP 20) | FSM Behaviour 7394a52aa0 | Roosting crow wakes, swoops, chases hero | sure | Aggro, chase and swoop target the local hero only, and HP and death are per game. | Wake (who alerted it), damage taken, death. | | scenes/Greymoor_01.fsm.txt:201 |
| Greymoor_01.6 | Crow ×3 | C# PlayMakerEventRange | Spreads a crow's cry to nearby crows | maybe | The cascade starts from whichever local crow was alerted, so one game wakes a flock while the other's flock sleeps. | The first crow's wake event; the range cascade then replays by itself. | | scenes/Greymoor_01.objects.txt:866 |
| Greymoor_01.7 | Floor Control Scene | FSM Control b1b7953a0b | Hornet crashes through floor, lands, area title | sure | Cutscene that teleports and freezes the local hero, shakes camera, breaks the floor, sets PlayerData crashedIntoGreymoor and starts music; entry crows fly off. | The start of the crash and the end (floor broken); both games then show the broken floor. | Pull the partner to the crash site and freeze them for the cutscene, leave them free (floor breaks around them), or only show the broken floor afterwards (recommended). | scenes/Greymoor_01.fsm.txt:1452 |
| Greymoor_01.8 | Greymoor Sway Plat Collider ×13 | FSM Enemy Rattle 1703c772ac | Platform jitters when an enemy lands | no | Visual jitter from local enemy collisions. | | | scenes/Greymoor_01.fsm.txt:4593 |
| Greymoor_01.9 | Mite (1) ×N (HP 15) | FSM Control 4a42a2c1fa | Small crawler: patrols, startles, bites hero | sure | Wakes and attacks via local alert ranges and needolin; falls and dies by own physics. | Wake, damage taken, death. | | scenes/Greymoor_01.fsm.txt:5424 |
| Greymoor_01.10 | Rain Region ×N | FSM Control 189fcb1783; C# RainVibrationRegion | Rain particles follow hero; controller rumble | no | Local weather and haptics. | | | scenes/Greymoor_01.fsm.txt:5588, scenes/Greymoor_01.objects.txt:2955 |
| Greymoor_01.11 | rosary_string_machine ×4 | FSM Behaviour (special) b934caf6ba | Pay rosaries to receive a collectable | maybe | Takes currency from and hands the item to whoever interacts, with a hero animation; no shared scene state. | Nothing if purchases stay personal. | One player pays and only they get the item (recommended), or both get it for one payment. | scenes/Greymoor_01.fsm.txt:5605 |

## Greymoor_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_02.1 | Crowman ×8, Crowman Dagger ×7 (HP 50, Normal and Black Thread variants) | FSM Control 1405129942, Control 064c5b9d3e | Flying crow warriors: wildslash, charge, stab, hop | sure | Random attack choice and chase target the local hero; the wake cry (CROW CALL register event) wakes the whole local room. | Wake and target, attack choice or start of each attack, damage taken, death. | | scenes/Greymoor_02.fsm.txt:4, scenes/Greymoor_02.fsm.txt:628 |
| Greymoor_02.2 | Flyaway Scene | FSM Control 50f871c185 | Hero trigger sends FLY AWAY to crows once | no | Cosmetic scenery crows, though it has a saved bool. | | | scenes/Greymoor_02.fsm.txt:4060 |
| Greymoor_02.3 | Seth Sit NPC Greymoor | FSM Location 81c28ab221 | Seth sits here if PlayerData SethNpcLocation matches | maybe | Talking moves Seth on by writing SethNpcLocation, so the NPC is in a different place per player until the flag syncs. | The PlayerData change SethNpcLocation (applied on next load). | | scenes/Greymoor_02.fsm.txt:6250 |
| Greymoor_02.4 | Seth Sit NPC Greymoor (1) | C# DeactivateOnHeroRespawnPoint | Hides NPC if hero respawns at that bench | maybe | Reads the local hero's respawn marker, which differs per player. | | | scenes/Greymoor_02.objects.txt:3536 |
| Greymoor_02.5 | Greymoor Crane - EDITED | C# BounceBalloonCrane | Lever lowers chain that hangs real balloons | sure | Opened by "Greymoor Stand Lever - Crane" (a Lever, saved); the other player's balloons never inflate if only the hitter's game opens it. | The crane lever hit; chain motion, gears and balloon inflation replay natively. | | scenes/Greymoor_02.objects.txt:1767 |
| Greymoor_02.6 | Balloon Anchor ×N (S, M) | C# BounceBalloonCraneAnchor (71 uses) | Swaps fake balloon for real one on crane | no | Follows the crane open state. | | | scenes/Greymoor_02.objects.txt:636 |
| Greymoor_02.7 | Inside Chain | C# MoveOffsetFollowLooped | Scrolls chain sprite with crane | no | Visual. | | | scenes/Greymoor_02.objects.txt:1777 |
| Greymoor_02.8 | greymoor_balloon ×N (82 uses) | C# BounceBalloon | Downward nail hit bounces hero upward | maybe | Bounce steers only the hitting hero; usable once inflated, and inflation follows the crane. | Nothing beyond the crane. | | scenes/Greymoor_02.objects.txt:2587 |

## Greymoor_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_03.1 | Mitefly ×N (HP 15, Normal and Thread variants) | FSM Control bc1ad62e84 | Flying mite: patrol, fly in on ambush, attack | sure | Ambush range, attack range and needolin read the local hero; death by hits is per game. | Wake or ambush start, damage taken, death. | | scenes/Greymoor_03.fsm.txt:1051 |
| Greymoor_03.2 | Mitefly Sprite ×N | FSM Motion b713a6029d | Bobbing drift of the sprite | no | Cosmetic wobble. | | | scenes/Greymoor_03.fsm.txt:1297 |
| Greymoor_03.3 | Nuu Control | FSM FSM 85f035893c | Shows Nuu if journal complete, sets nuuIsHome | maybe | Reads PlayerData (nuuMementoAwarded, JournalIsCompleted, Journal quest) and writes nuuIsHome, so Nuu appears for one player only. | The PlayerData flag nuuIsHome (applied on load). | | scenes/Greymoor_03.fsm.txt:3035 |
| Greymoor_03.4 | Scarecraw (Weathervane Post Quest) | FSM Hit Response 68cbfef5d0 | Spins left or right when hit | no | Cosmetic reaction. | | | scenes/Greymoor_03.fsm.txt:3220 |
| Greymoor_03.5 | weathervane (Pre Quest) | C# InteractiveWeathervane, SetZPerRotation | Spins when nail tinks it | no | Cosmetic animation; Pre and Post Quest variants come from quest state. | | | scenes/Greymoor_03.objects.txt:732, scenes/Greymoor_03.objects.txt:734 |
| Greymoor_03.6 | grey_sign_01 Push React ×20 | C# PushReactNonPhysical | Hanging sign sways when pushed | no | Visual. | | | scenes/Greymoor_03.objects.txt:2922 |
| Greymoor_03.7 | Hornet_cage_rect_lift_greymoor ×7 | C# LiftControl | Multi-stop lift with call plates and doors | sure | Hero standing in the lift or on a plate moves it; position, current stop and unlock are local, so a rider's lift is elsewhere in the partner's game. | Lift stop and movement start (which stop, who called it), plus unlock. | Partner rides along on the same lift, or each player uses their own copy. | scenes/Greymoor_03.objects.txt:3653 |

## Greymoor_04

Two battle scenes live here: Battle Scene (Normal World) and Black Thread Battle Scene (Black Thread World), picked by PlayerData blackThreadWorld.

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_04.1 | grey_lever_gate ×4 | FSM FSM 04e331f3f5 | Relays BG CLOSE or BG OPEN to Gate | sure | Arena gates close only in the game whose hero started the battle, so the partner is not locked in and cannot see the gates. | Battle start and battle end; gates then close and open natively. | Lock both players in once either starts the battle (recommended), or leave the partner outside. | scenes/Greymoor_04.fsm.txt:4 |
| Greymoor_04.2 | Black Thread Battle Gate ×14 | FSM Control 7f40254ff6 | Gate animation and wall collider on BG events | sure | Same as above for the black-thread arena; the wall collider decides who can walk through. | Battle start and end (same trigger as .1). | | scenes/Greymoor_04.fsm.txt:26 |
| Greymoor_04.3 | Roachfeeder Short ×16 (HP 40) | FSM Msg Reactor 726b25304d, Control 6f6f85ed5e | Flying roach: patrols, startles, chases hero | sure | Alert range and needolin are the local hero's; GO UP and GO DOWN messages push its altitude. | Wake, damage taken, death. | | scenes/Greymoor_04.fsm.txt:1073, scenes/Greymoor_04.fsm.txt:1084 |
| Greymoor_04.4 | Farmer Centipede ×7 (HP 50) | FSM Control f51a1154e6 | Burrowing centipede, ambushes, leaps at hero | sure | Chases and leaps toward the local hero; can ambush from the ground when MOORWING ROOSTING is sent. | Wake or ambush start, damage taken, death. | | scenes/Greymoor_04.fsm.txt:1301 |
| Greymoor_04.5 | Farmer Scissors ×11 | FSM Cut Check 37b37ac3c6, Control 906a691a77 | Scissor walker: snips, evades, cuts floor or roof | sure | Cut checks and the "Cut Down Hero Grab" trigger catch only the local hero; it can grab and carry the hero downward. | Wake and target, attack choice, damage taken, death; the grab moves only the hero it caught. | | scenes/Greymoor_04.fsm.txt:2772, scenes/Greymoor_04.fsm.txt:2838 |
| Greymoor_04.6 | propeller ×6 (lever/Control Lever) | FSM Control 709dc86c70, Detect Hit 7a7e53edca | Hitting lever charges propeller, creates updraft | sure | Energy builds only from hits in the local game, so the updraft region exists only for the hitter. | Each lever hit (with its magnitude); energy, spin stages and the updraft replay natively. | | scenes/Greymoor_04.fsm.txt:4022, scenes/Greymoor_04.fsm.txt:4165 |
| Greymoor_04.7 | Bot or Top Propellor ×18 | FSM Animation f2f8a2143f | Spin speed animation by stage | no | Visual follow-up of .6. | | | scenes/Greymoor_04.fsm.txt:4196 |
| Greymoor_04.8 | Scissor Front chunks ×29 | C# BreakablePoleTopLand | Dead scissors' blades stick in ground | no | Corpse debris physics. | | | scenes/Greymoor_04.objects.txt:1869 |

## Greymoor_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_05.1 | Garmond Sleep NPC ×2 | FSM Behaviour b16818dc73 | Sleeping Garmond wakes, roars, first-meets hero | sure | On first wake it takes control of the local hero (hero roar-lock animation, pause disabled), runs the meet dialogue and sets PlayerData metGarmond. | The start of the wake-and-roar (who triggered) and the end of the meeting. | Pull the partner in to watch and freeze them, leave them free, or run the dialogue only for the trigger player (recommended). | scenes/Greymoor_05.fsm.txt:308 |
| Greymoor_05.2 | Zaza Sleep NPC ×3 | FSM Behaviour 60aee37041 | Sleeping Zaza reacts to needolin and startle | no | Cosmetic NPC reaction. | | | scenes/Greymoor_05.fsm.txt:506 |
| Greymoor_05.3 | greymoor_breakable_plat ×6 | FSM Control f088afb378 | Platform breaks when hit by a Wall Breaker | sure | The break (and the optional PlayerData bool it sets) happens only in the game whose hero hit it; the other keeps a solid platform. | The break (platform id); effects replay natively. | | scenes/Greymoor_05.fsm.txt:673 |
| Greymoor_05.4 | Scene Control | FSM Scene Control 9dcb759e13 | Picks boss, jailer cart or farmers by PlayerData | sure | Chooses the whole room's enemy suite from PlayerData (greymoor05_clearedOut, defeatedVampireGnatBoss, VampireGnatBossInAltLoc, slab_cloak_battle_completed, hasWalljump and more), so differing flags give different rooms. | Nothing live; both games need identical values of these flags at room load. | | scenes/Greymoor_05.fsm.txt:2712 |
| Greymoor_05.5 | Vampire Gnat Roosting | FSM Control 23235dfe23 | Perched gnat flies off when hero enters | sure | Sets encounteredVampireGnat_05 and sends MOORWING ROOSTING and MOORWING LEFT (wakes centipede ambushes) in the entering player's game only. | The hero-entry trigger; fly-off and follow-up events replay natively. | | scenes/Greymoor_05.fsm.txt:5251 |
| Greymoor_05.6 | Slab_Cart, Slab Fly Large Cage, Slab_Cart Broken | C# GameObjectMatchActiveState (×3) | Mirrors active state between cart objects | no | Static scene dressing following the cart choice made in .4. | | | scenes/Greymoor_05.objects.txt:992 |

## Greymoor_05_boss

Vampire Gnat (Moorwing) boss scene, enabled by Greymoor_05 Scene Control.

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_05_boss.1 | Vampire Gnat Boss Scene | FSM Control b09f1c8e67 | Fight-start trigger, hero death count, Garmond aid | sure | Starts only when the local hero hits the start range and sends BATTLE START; on Hornet death it adds to vampireGnatDeaths, on BOSS DEATH it sets greymoor05_clearedOut. | Fight start (who entered) and boss death; also whose death ends or restarts the fight. | Pull the partner into the fight when either enters (recommended), or fight separate boss copies. | scenes/Greymoor_05_boss.fsm.txt:1 |
| Greymoor_05_boss.2 | Vampire Gnat (HP boss) | FSM Control 071e32c659, Y Control 965816245f | Boss: flaps, charges, slashes, rage, sickles | sure | Every attack choice (random event picks), chase and its flight height follows the local hero; two runs diverge. | Boss state (attack chosen and target), damage taken, death. | Which player the boss targets (nearest, chosen by one player, or alternating). | scenes/Greymoor_05_boss.fsm.txt:84, scenes/Greymoor_05_boss.fsm.txt:28 |
| Greymoor_05_boss.3 | Vampire Gnat | FSM Charge Dust Effects 861a2ddfc1, Slash Dust Effects 42b8078171 | Dust trail on charge and slash | no | Particles. | | | scenes/Greymoor_05_boss.fsm.txt:52, scenes/Greymoor_05_boss.fsm.txt:67 |
| Greymoor_05_boss.4 | Slash Collider ×12 | FSM Multihitter adf9a4ee0c | Multi-hit slash grabs hero | sure | Hits and parry checks use the local hero; it locks that hero in "Roar and Wound States" while the boss hovers over a position. | Which hero was caught and the slash event; each game plays the hit on its own hero. | | scenes/Greymoor_05_boss.fsm.txt:942 |
| Greymoor_05_boss.5 | Vampire Gnat Corpse(Clone) | FSM Death 9514b64b26 | Boss death blast, falls, drops shard pickup | sure | Sets VampireGnatCorpseInWater, sends VAMPIRE GNAT DEFEATED and spawns a collectable pickup; the fight end must happen in both games. | Boss death (and corpse landing); the pickup and explosion replay natively. | Reward given to both players or only the one who dealt the last hit. | scenes/Greymoor_05_boss.fsm.txt:1053 |

## Greymoor_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_06.1 | belltown_lever_drop_plat | FSM Drop ed980bf792 | DROP: jitters, then falls out of view | sure | The platform is a path element that falls only where the DROP message (from a lever) arrived, so the partner's route stays unchanged. | The lever hit that sends DROP; fall replays natively. | | scenes/Greymoor_06.fsm.txt:4 |

## Greymoor_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_07.1 | Garmond Scene | FSM Control f1ac283ca1 | Chooses Garmond present or away by PlayerData | maybe | Reads VampireGnatBossInAltLoc, defeatedVampireGnatBoss, metGarmond, garmondMoorwingConvoReady and quest "Save the Fleas"; players with different flags see different rooms. | Nothing live; same flags at room load. | | scenes/Greymoor_07.fsm.txt:3339 |
| Greymoor_07.2 | Garmond Aid NPC ×3 | FSM Behaviour 691a1ffa56 | Garmond: asks aid, attacks enemies near hero | sure | Dialogue (accept sets the PlayerData "aid" bool) and combat run per game; he attacks enemies in his range when the hero is hurt. | The accept (PlayerData aid bool) and which enemy he attacks. | | scenes/Greymoor_07.fsm.txt:3365 |
| Greymoor_07.3 | Vampire Gnat Scene | FSM Control e90295a5a3 | Enables gnat scene if not yet seen | maybe | Gated by PlayerData (encounteredVampireGnat_07, CaravanTroupeLocation and more). | Nothing live; same flags at load. | | scenes/Greymoor_07.fsm.txt:4351 |
| Greymoor_07.4 | Vampire Gnat Swoop | FSM Control 5fdb22472d | Gnat sweeps across when hero enters trigger | maybe | Scripted fly-by (camera shake) sets encounteredVampireGnat_07 for the entering player only. | The trigger entry (one-shot); fly-by replays natively. | Replay the fly-by for both players or just the trigger player. | scenes/Greymoor_07.fsm.txt:4368 |
| Greymoor_07.5 | Farmer Scissors | FSM Shift Pos d512e22e8b | Moves enemy to y=86.12 if entry is right1 | no | Level placement per entry gate of the local hero. | | | scenes/Greymoor_07.fsm.txt:4466 |
| Greymoor_07.6 | greymoor_lever_simple | FSM Open For Courier Quest 3f79a39b79 | Opens when courier quest is tracked | maybe | Quest state is per player; the lever opens only for a player who tracks it. | Quest tracking (or just treat it as always open). | | scenes/Greymoor_07.fsm.txt:6657 |

## Greymoor_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_08.1 | Shakra Guard Scene / Scene Folder | FSM Enemy Group Activation 2cc6371f56 | Starts Shakra's guard fight when hero in range | sure | Picks enemy group by PlayerData (mapperLocationAct3, mapperIsFightingAct3, mapperFightGroup) and on hero entry sets seenMapperAct3 and mapperLocationAct3. | The fight start (who entered) and the chosen group. | | scenes/Greymoor_08.fsm.txt:28 |
| Greymoor_08.2 | Mapper StandGuard NPC | FSM Dialogue a1ef0ec67e | Shakra's talk and shop, mapperConvo flags | maybe | Dialogue, shop and flags (mapperConvo_Act3Intro, white-flower convo) are personal. | Nothing live. | | scenes/Greymoor_08.fsm.txt:1250 |
| Greymoor_08.3 | Mapper StandGuard NPC | FSM Attack Enemies e262293ab8 | Shakra teleports and attacks enemies in range | sure | Allied NPC AI picks the nearest enemy and teleports near it; two games would pick different targets. | Target enemy chosen and attack start. | | scenes/Greymoor_08.fsm.txt:1368 |
| Greymoor_08.4 | grey_wood_chain_plat_mid_breakable | FSM Destroy 1998659617 | Platform breaks under a Wall Breaker | sure | Sets PlayerData greymoor08_plat_destroyed and removes the platform only in the hitter's game. | The break; effects replay natively. | | scenes/Greymoor_08.fsm.txt:1663 |
| Greymoor_08.5 | greymoor_drop_propeller | C# SuspendedDropPropeller | Cut vine drops platform, starts propeller updraft | sure | A cut is saved in a PersistentBoolItem; one game drops it and has the updraft, the other still has the hanging platform. | The vine cut; drop and propeller replay natively. | | scenes/Greymoor_08.objects.txt:193 |

## Greymoor_08_boss

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_08_boss.1 | Send Gate Hide Event | FSM FSM 7b5ad680c2 | Sends HIDE TRANSITION GATE at start | maybe | Hides the exit gate for the local hero, part of the boss lock. | Nothing beyond the fight start. | | scenes/Greymoor_08_boss.fsm.txt:1 |
| Greymoor_08_boss.2 | Vampire Gnat Scene | FSM Control ec8cfd050e | Boss fight start, death count, aftermath flags | sure | Fight starts when the local hero enters the start range, sets encounteredVampireGnatBoss, adds to vampireGnatDeaths on hero death and sets VampireGnatDefeatedBeforeCaravanArrived on boss death. | Fight start and boss death. | Same as Greymoor_05_boss.1: pull the partner in, or run separate fights. | scenes/Greymoor_08_boss.fsm.txt:5 |
| Greymoor_08_boss.3 | Tension Range | FSM Control c427c65e64 | Tension music while hero inside | no | Audio. | | | scenes/Greymoor_08_boss.fsm.txt:1373 |

## Greymoor_08_caravan

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_08_caravan.1 | Caravan Lech Wounded ×2 | FSM Wounded Behaviour 908e7a041e | Wounded flea NPC: talk, bounces when hit | maybe | Dialogue writes PlayerData CaravanLechWoundedSpoken; hits and floating physics are local. | Nothing live. | | scenes/Greymoor_08_caravan.fsm.txt:1115 |
| Greymoor_08_caravan.2 | Caravan Troup Member Short ×3 | FSM Behaviour ed2f25deda | Greets hero; sells Flea Brew refills | maybe | Refill spends the player's rosaries and unlocks a tool; personal. | Nothing live. | | scenes/Greymoor_08_caravan.fsm.txt:1329 |
| Greymoor_08_caravan.3 | Caravan Troupe Leader Greymoor NPC | FSM Dialogue d5b3f86dfe | Leader dialogue, waves, flea count, leave check | maybe | Sets MetCaravanTroupeLeaderGreymoor and reads flea count and boss flags; reacts to CARAVAN TRAVEL ENDED. | PlayerData flags (applied on load). | | scenes/Greymoor_08_caravan.fsm.txt:1578 |
| Greymoor_08_caravan.4 | door_caravanTravelEnd ×3 | FSM Travel End 3d71352c19 | Arrival: black screen, sets bench respawn, saves | sure | Blocks input, sets the local hero's respawn marker here and queues a save; the partner is not moved with the caravan. | The caravan travel start (which destination) so the partner also arrives. | Partner travels along (recommended), stays behind, or may follow later by walking. | scenes/Greymoor_08_caravan.fsm.txt:1684 |
| Greymoor_08_caravan.5 | door2 | FSM FSM 375e2d6a84 | Door animation closed or open by entry | no | Animator only. | | | scenes/Greymoor_08_caravan.fsm.txt:1711 |
| Greymoor_08_caravan.6 | Caravan Troupe Flea Swimmer ×5 | FSM Control 588037280a | Fleas dive and splash in water | no | Cosmetic (reacts to needolin locally). | | | scenes/Greymoor_08_caravan.fsm.txt:3557 |
| Greymoor_08_caravan.7 | Caravan Music Control ×6 | C# CaravanMusicControl | Caravan music | no | Audio. | | | scenes/Greymoor_08_caravan.objects.txt:440 |

## Greymoor_08_mapper

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_08_mapper.1 | Mapper Spar NPC | FSM Dialogue b100755e0e | Spar offer: accept starts a duel with Shakra | sure | Accept sets a non-lethal death respawn for the local hero and sends BATTLE HERO and MAPPER SPAR START; the partner's game has no duel. | Duel start (who accepted) and result. | Partner watches (recommended), joins as second fighter, or is held outside. | scenes/Greymoor_08_mapper.fsm.txt:41 |
| Greymoor_08_mapper.2 | Mapper Spar NPC | FSM Unalert Control f657c7fb84 | Toggles "In Unalert Range" during fight | maybe | Reads the local hero's distance to end aggro; part of the duel AI. | Nothing beyond .1. | | scenes/Greymoor_08_mapper.fsm.txt:979 |
| Greymoor_08_mapper.3 | Mapper Spar NPC | C# ParticleEffectsScaleTarget | Scales particles with NPC | no | Visual. | | | scenes/Greymoor_08_mapper.objects.txt:5 |

## Greymoor_10

no new elements

## Greymoor_11

no new elements

## Greymoor_12

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_12.1 | Float Sack ×N (+ Art) | FSM Control 20e59df2a8, Destroy 53c707c261, Art Control c00c0786b1 | Drifting sack, bobs when bopped, pops on damage | no | Decoration; popped sacks drop nothing and are not saved. | | | scenes/Greymoor_12.fsm.txt:1540, scenes/Greymoor_12.fsm.txt:1598, scenes/Greymoor_12.fsm.txt:1692 |
| Greymoor_12.2 | Float Spool ×3 | FSM Destroy 95249731d8 | Pops on damage | no | Decoration, not saved. | | | scenes/Greymoor_12.fsm.txt:2088 |

## Greymoor_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_13.1 | Pilgrim Bellthrower Fly ×N (Thread Only) | FSM Control cc656613c3 | Flies near hero, throws bells | sure | Keeps distance from, aims at, and spawns Throwing Bell projectiles toward the local hero. | Wake, throw (position and target), damage taken, death. | | scenes/Greymoor_13.fsm.txt:168 |

## Greymoor_15

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_15.1 | crow enemies, crow juror enemies, crowcourt_rag_set ×7 | FSM Deactivate? ccb2deebdf | Toggles groups by CrowCourtInSession | maybe | PlayerData-selected variants differ per player. | Nothing live; same flag at load. | | scenes/Greymoor_15.fsm.txt:4 |
| Greymoor_15.2 | Crowman Juror Tiny Variant - Wild ×3 (HP 50) | FSM Behaviour 7085ff2dbc | Crow juror: roosts, wakes, swoops at hero | sure | Wake, chase and swoop target the local hero; PlayMakerEventRange spreads the alarm. | Wake, damage taken, death. | | scenes/Greymoor_15.fsm.txt:1281 |
| Greymoor_15.3 | grey_metal_window_collider ×3 | FSM FSM 758e9da7a6 | Blocks and tinks nail attacks | no | Local impact reaction. | | | scenes/Greymoor_15.fsm.txt:2681 |

## Greymoor_15b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_15b.1 | memory_font (Memory Orb Challenge) | FSM Control 3874ee47cc | Needolin summons orb chase; interact heals hero | sure | Needolin and the orb timer run on the local hero; completion goes to the challenge's saved bool, and interact refills the activator's health to max (MaxHealthKeepBlue). | Challenge start and completion. | Shared progress (any orb counts for both) or personal progress. | scenes/Greymoor_15b.fsm.txt:12717 |
| Greymoor_15b.2 | Mite (1) | FSM FSM 6fc328891e | Dies when it falls below y=9.18 | no | Scenery enemy that kills itself. | | | scenes/Greymoor_15b.fsm.txt:12966 |

## Greymoor_16

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_16.1 | Gnat Giant ×3 (HP 60) | FSM Behaviour 6932013241 | Large gnat: bounds, aggro roar, charges | sure | Aggro range and bounds follow the local hero; it breaks things around it (Breaker). | Aggro and target, damage taken, death. | | scenes/Greymoor_16.fsm.txt:1302 |
| Greymoor_16.2 | Gnat Giant ×3 | FSM Croak Audio 388c9128a6 | Croak sounds | no | Audio. | | | scenes/Greymoor_16.fsm.txt:1437 |
| Greymoor_16.3 | greystore_floor_shortcut (1) | C# SuspendedPlatform | Cut vine drops shortcut floor | sure | Persistent platform that falls and opens a route only in the cutter's game. | The vine cut; fall replays natively. | | scenes/Greymoor_16.objects.txt:2991 |

## Greymoor_17

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_17.1 | Mitefly ×2 | FSM lowpass_audio_by_area 9323987af7 | Muffles audio by area | no | Audio. | | | scenes/Greymoor_17.fsm.txt:701 |

## Greymoor_20b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_20b.1 | Chapel Door Control / Door DoClose | FSM Control e1e913091f | Chapel door slams shut behind hero, then saves | sure | Chapel Door Control decides by the local hero's crest and PlayerData chapelClosed_*, sets the death respawn, closes the door and saves the game; the partner's door stays open. | Door close (the entry moment); slam and lock replay natively. | Lock both players in the chapel challenge, only the one who entered (recommended), or let the partner through. | scenes/Greymoor_20b.fsm.txt:76 |
| Greymoor_20b.2 | scissor_string Tink Detector ×12 | FSM Tink React c85fb4408d | String swings when nail tinks it | no | Cosmetic physics. | | | scenes/Greymoor_20b.fsm.txt:96 |

## Greymoor_20c

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_20c.1 | Battle Scene / Detector | FSM FSM 31e899d9b5 | Starts battle when hero stays after bridge closes | sure | Triggers on the local hero (tag Player) only and skips if the battle bool is done; the partner is not caught. | Battle start and end. | Lock both players in, or only the trigger player (recommended: only if partner is in the room). | scenes/Greymoor_20c.fsm.txt:4 |
| Greymoor_20c.2 | drop bridge | FSM BG Control 5694b3576f | Drop bridge gate seals and opens | sure | Reacts to BG events of the local battle scene. | Battle start and end (same as .1). | | scenes/Greymoor_20c.fsm.txt:31 |
| Greymoor_20c.3 | Music Control ×3 | FSM Control 776b3d3136 | Chapel music unless Reaper crest owned | no | Audio. | | | scenes/Greymoor_20c.fsm.txt:3759 |
| Greymoor_20c.4 | Greymoor_windmill_cog (2), Belltown_0040_cog | C# RotationController | Rotates cog with raw horizontal input | no | Decorative; reads the local keyboard axis. | | | scenes/Greymoor_20c.objects.txt:848 |

## Greymoor_21

no new elements

## Greymoor_22

no new elements

## Greymoor_24

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Greymoor_24.1 | Caravan Lech (Black Thread World) | FSM Custom Dialogue - Act3 Rescue de636fdaaf | Rescued flea talks, sets CaravanLechSaved | maybe | Dialogue and PlayerData writes are personal. | Nothing live. | | scenes/Greymoor_24.fsm.txt:298 |
| Greymoor_24.2 | Caravan Lech Strung Up | FSM Control 7a143b0127 | Strike threads to free flea (+silk per hit) | sure | Each hit gives silk to the hitter and a counter drops; the break sets CaravanLechSaved, counts a flea saved and frees the NPC only where struck. | The hits (or the final break); free-fall and NPC appearance replay natively. | Count hits from both players together (recommended) or per game. | scenes/Greymoor_24.fsm.txt:592 |

## Halfway_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Halfway_01.1 | HH Bartender | FSM Dialogue 7d484b0bab | Quest offer and reward, drink purchase | sure | Offers and completes quest "Crow Feathers", takes rosaries for drinks (heal effect) and sets PlayerData MetHalfwayBartender and HalfwayDrinksPurchased; quest state is per player. | Nothing live; quest and purchase results are personal. | Share quest progress (both count) or keep personal. | scenes/Halfway_01.fsm.txt:4 |
| Halfway_01.2 | HH Bartender, HH Patron Left, HH Patron Right | FSM Sing Reacton 82d0416142, 89f0a79208, d44de830d0; Rest Animation b0785f00d0, eac7c3f5e5, 43a533e0b9 | Idle poses and needolin reaction | no | Cosmetic. | | | scenes/Halfway_01.fsm.txt:277, scenes/Halfway_01.fsm.txt:323, scenes/Halfway_01.fsm.txt:798, scenes/Halfway_01.fsm.txt:842 |
| Halfway_01.3 | HH Patron Left | FSM Cellar Entry 02a8127411 | Pauses patron interaction after cellar trip | no | Local interaction toggle. | | | scenes/Halfway_01.fsm.txt:833 |
| Halfway_01.4 | Hunter Fan Control | FSM Control e21cb73ca5 | Shows Nuu by journal and PlayerData | maybe | Reads nuuIsHome, JournalIsCompleted and Journal quest; differing flags show different scenes. | Nothing live; same flags at load. | | scenes/Halfway_01.fsm.txt:355 |
| Halfway_01.5 | Nuu | FSM Dialogue de05ad1b29; C# tk2dFootstepPlayer | Journal fan: talks, gives memento | maybe | Dialogue sets MetHalfwayHunterFan; the reward is personal; footsteps are audio. | Nothing live. | | scenes/Halfway_01.fsm.txt:381, scenes/Halfway_01.objects.txt:117 |
| Halfway_01.6 | Audio Craws | FSM Control b92cc49bee | Random crow sounds | no | Audio. | | | scenes/Halfway_01.fsm.txt:758 |
| Halfway_01.7 | Pilgrim Crowds | FSM Crowd Control e1ab079fd3 | Shows one of five crowds by PlayerData halfwayCrowd | no | Cosmetic crowd variant. | | | scenes/Halfway_01.fsm.txt:912 |
| Halfway_01.8 | cellar trapdoor set | FSM Trapdoor 71efec2860 | Hero hops in, falls, transitions to cellar | maybe | Interact makes the local hero hop in, sets respawn and begins a scene transition; a lone transition separates the players. | The transition (partner follows or not). | Partner follows into the cellar, or stays. | scenes/Halfway_01.fsm.txt:1039 |
| Halfway_01.9 | Music Control | FSM Control 04ab832aa7 | Rest-area music | no | Audio. | | | scenes/Halfway_01.fsm.txt:1105 |

## Ant_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_02.1 | Battle Scene | FSM Start Control fdbeba2c54 | After guard wakes, hero in range starts battle | sure | Starts only when the local hero is in "Can Start Range" and then sets Endless Guard Range on the Bone Hunter Throw. | Battle start (who triggered) and end. | Lock both players in, or only the trigger player. | scenes/Ant_02.fsm.txt:4 |
| Ant_02.2 | Battle Gate Ant ×10 | FSM BG Control 32ae2b47a6 | Ant gates close and open on BG events | sure | Closed wall collider traps only the local hero; opened only when its own battle ends. | Battle start and end (same trigger as .1). | | scenes/Ant_02.fsm.txt:20 |
| Ant_02.3 | Bone Hunter Throw / Guard Range | FSM Custom Guard Range 452d8d3853 | Shifts guard range if entering from left1 | no | Placement by entry gate. | | | scenes/Ant_02.fsm.txt:466 |
| Ant_02.4 | Tension Music Control | FSM Control d98b35797f | Tension music by PlayerData and entry | no | Audio. | | | scenes/Ant_02.fsm.txt:477 |
| Ant_02.5 | Ant Region (13), Ant Region (15) | C# AntRegion (66 uses) | Ants carry loose corpses and currency away | maybe | Loose objects (corpses, dropped rosaries, shards) are carried off by local ants, so a drop may vanish in one game while still lying in the other. | Nothing if pickups are shared by id; otherwise the pickup. | | scenes/Ant_02.objects.txt:864 |

## Ant_03

no new elements

## Ant_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_04.1 | Hunter Sickle Trap (2) / Slam Effect | FSM FSM 4090562684 | Detaches slam effect object | no | Visual effect cleanup. | | | scenes/Ant_04.fsm.txt:3947 |

## Ant_04_left

no new elements

## Ant_04_mid

Two battle scenes live here: Battle Scene (Normal World) and Battle Scene Black Thread.

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_04_mid.1 | Mapper NPC | FSM Placement Control f9cf849881 | Places Shakra: away, waiting, or present | maybe | Reads ant04_battleCompleted, mapperMetInAnt04 and writes mapperMetInAnt04 and SeenMapperHuntersNest at shop start. | The PlayerData flags (applied on load). | | scenes/Ant_04_mid.fsm.txt:4337 |
| Ant_04_mid.2 | Mapper NPC | FSM Ring Shout 4de420554e | Shakra shouts at hero, camlock, hero pose | sure | First meet seizes the local hero (RunFSM template, camera lock, roar, "Challenge Talk Idle Start") when the hero enters Shout Range. | The shout start (and its end). | Pull and freeze the partner for the cutscene, or leave them free (recommended). | scenes/Ant_04_mid.fsm.txt:4865 |
| Ant_04_mid.3 | Return Scene | FSM Control 0064f3f2fd | Picks returning enemy group by PlayerData | maybe | Uses ant04_enemiesReturn, visitedCitadel and enemyGroupAnt04; differing flags give different enemies. | Nothing live; same flags at load. | | scenes/Ant_04_mid.fsm.txt:5605 |
| Ant_04_mid.4 | Bone Hunter Child (Return) ×2 | FSM Control 94a1922d22 | Bone hunter: ambush, slash, throw, jump | sure | Targeting, attack choice and ambush trigger follow the local hero. | Wake or ambush start, attack choice, damage taken, death. | | scenes/Ant_04_mid.fsm.txt:7020 |
| Ant_04_mid.5 | Shakra Fighter ×2 | FSM Control f42491c30a | Ally fighter: teleports, throws, can be bound | sure | Ally AI picks enemies and teleport points itself, so two games diverge. | Chosen target and each attack start. | | scenes/Ant_04_mid.fsm.txt:8398 |
| Ant_04_mid.6 | Shakra Fighter ×3 | FSM Register Forum Battle End 1304f45236 | On FORUM BATTLE END, marks battle ended | maybe | Follows the battle end event. | Nothing beyond the battle end. | | scenes/Ant_04_mid.fsm.txt:8713 |
| Ant_04_mid.7 | Shakra Fighter / Enemy Range | FSM Deparent fb3d698723 | Detaches range object | no | Setup. | | | scenes/Ant_04_mid.fsm.txt:8731 |
| Ant_04_mid.8 | Shakra Fighter / Victory NPC ×2 | FSM Dialogue 25511ab459 | Post-fight talk, then leaves | maybe | Dialogue is personal; Shakra leaves in the game where it ended. | Nothing live. | | scenes/Ant_04_mid.fsm.txt:8758 |
| Ant_04_mid.9 | Wave 2 - Call Ally / Call Ally | FSM Control 00a9420b4f | Calls Shakra at wave 2 unless regions visited | sure | PlayerData (visitedGreymoor, visitedBellhart, visitedShellwood) decides whether the ally appears; it destroys itself afterward. | The call (wave 2 start and whether Shakra comes). | | scenes/Ant_04_mid.fsm.txt:10117 |

## Ant_05b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_05b.1 | Return Scene / Scene | FSM FSM 30069f6085 | Sets Call Buzzer on a returning bone hunter | maybe | Reinforcement calling follows the return group; enemy flags are local. | The reinforcement call. | | scenes/Ant_05b.fsm.txt:2538 |
| Ant_05b.2 | Bone Hunter Child (Wave 1) | FSM Control b8401c4cb0 | On BATTLE START sets Gotta Battle | sure | Wave start signal that wakes the whole wave; only the starting game fires it. | Battle start. | | scenes/Ant_05b.fsm.txt:3475 |

## Ant_05c

no new elements

## Ant_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_08.1 | BattleStart Inspect Region | FSM Control fd7d9e45bc | Interact with nectar table: invincible, quake, ambush | sure | Interact makes the local hero reach and roar-lock, sets isInvincible, smashes the table, enables ant regions and starts the battle; exit trapdoor is blocked. | The interaction (battle start) and the end ("PICKED UP NECTAR"). | Pull the partner in and lock them in, or leave them free (recommended). | scenes/Ant_08.fsm.txt:4 |
| Ant_08.2 | End Scene | FSM Control 0316c1e2b0 | On battle end shows nectar pickup | sure | Reward appears only in the game where the battle ended; the exit door opens after the pickup. | The battle end and who picked up the item. | Pickup given to both, or only to the picker. | scenes/Ant_08.fsm.txt:64 |
| Ant_08.3 | Wall Chunk - Plat, Wall Chunk A, B, C | FSM Wall Chunk Break 8984b51991, 00b4d2ca3d, 62c6529e18 | Wall pieces and platform break on BREAK | maybe | Follow the battle's BREAK and ANT SMASH PLAT messages; platform exists only in the unbroken game. | Nothing beyond the battle start. | | scenes/Ant_08.fsm.txt:1186, scenes/Ant_08.fsm.txt:1201, scenes/Ant_08.fsm.txt:1216 |
| Ant_08.4 | Wave 2 Starter / Wave Starter | FSM Control 86134ded3d | Rumble, then starts wave 2 | sure | Starts the next wave in the local battle only. | Battle start (waves follow). | | scenes/Ant_08.fsm.txt:5952 |
| Ant_08.5 | Wave 2 / Bone Hunter Throw | FSM FSM f37c09b36e | Toggles "No Dig" by x position | no | Enemy placement helper. | | | scenes/Ant_08.fsm.txt:8289 |

## Ant_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_09.1 | Battle Music (1) | FSM Music Control dbeb4c4b41 | Counts alerted hunters for music | no | Audio. | | | scenes/Ant_09.fsm.txt:4 |
| Ant_09.2 | Bone Hunter Fly | FSM FSM 7b24171bbb | Relays ALERT or UNALERT to music | no | Relay for audio. | | | scenes/Ant_09.fsm.txt:28 |

## Ant_14

no new elements

## Ant_17

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_17.1 | Trap Scene / RestBench | FSM Trap Bench e69a92ae66 | Fake bench: sitting springs the sickle trap | sure | Sitting freezes the hero and sends TRAP SPRUNG to the sickle trap; the trap defuses when PlayerData antBenchTrapDefused is set. | The bench sit (trap sprung) and the get-up. | | scenes/Ant_17.fsm.txt:289 |

## Ant_19

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_19.1 | Boss Scene | FSM Control 1013f24441 | Boss arena: gates, camlock, starts when hero near | sure | Start Range and Early Start Range read the local hero; it closes gates, locks camera, clamps that hero's X; boss is invincible until the fight begins. | Fight start (who entered), boss death. | Pull the partner into the arena and lock them in (recommended), or fight two separate bosses. | scenes/Ant_19.fsm.txt:94 |
| Ant_19.2 | Beastmaster Corpse | FSM Control fa2601bc1f | Reacts to THUMP | no | Cosmetic. | | | scenes/Ant_19.fsm.txt:145 |
| Ant_19.3 | Corpse Giant Bone Flyer(Clone) | FSM Death 6d6eb417de | Boss death: blasts, flings chunks, drops reward | sure | Sends BOSS DEFEATED (opens gates, enables the reward pickup, music) and sets the defeat flag; must end in both games. | Boss death; effects replay natively. | Reward to both or only the killer. | scenes/Ant_19.fsm.txt:958 |
| Ant_19.4 | Enemy Summon Cage ×2 | FSM Control d4bea28295 | Cage drops from random X, spawns enemy | sure | Random X spawn position and summon are per game, so adds appear in different places. | The summon event (cage X and enemy type). | | scenes/Ant_19.fsm.txt:1145 |
| Ant_19.5 | Bone Circler Vicious Smn, Bone Crawler Smn ×2, Bone Flyer Smn ×2 | FSM Control 3d619ae59f, 90da941ba1, 98cb0f0665 | Summoned adds: chase hero, attack | sure | Targeting and attacks follow the local hero; they vanish on BOSS DEFEATED. | Wake or target, damage taken, death. | | scenes/Ant_19.fsm.txt:1236, scenes/Ant_19.fsm.txt:1385, scenes/Ant_19.fsm.txt:1631 |
| Ant_19.6 | Bone Flyer Smn ×2 | C# ExtraDamageable | Empty marker component | maybe | No logic in the class; the real effect is whatever checks for it. | | | scenes/Ant_19.objects.txt:2388 |
| Ant_19.7 | Thump Effects | FSM Control e339a19330 | Thump shake on THUMP | no | Effect. | | | scenes/Ant_19.fsm.txt:1983 |
| Ant_19.8 | Harpoon Ring Ant String ×2 / string | FSM Grab Wobble d2b93ca9c2 | String wobbles when the harpoon connects | no | Visual reaction to a local harpoon grab. | | | scenes/Ant_19.fsm.txt:5929 |
| Ant_19.9 | Music Control | FSM Control e49adc2a76 | Chapel music unless Warrior crest owned | no | Audio. | | | scenes/Ant_19.fsm.txt:6096 |

## Ant_20

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_20.1 | Bone Flyer Rage Control | FSM Control b17a8c9521 | Periodic smash and camera shake while boss lives | no | Cosmetic shake and debris; inert once defeatedBoneFlyerGiant is set. | | | scenes/Ant_20.fsm.txt:4 |
| Ant_20.2 | Chapel Door Control / Door DoClose | FSM Control 0d9e7a1964 | Chapel door slams shut behind hero, saves | sure | Same as Greymoor_20b.1: local crest and chapelClosed_* decide, it sets respawn and saves. | Door close (entry moment). | Lock both players in, only the one who entered (recommended), or let the partner through. | scenes/Ant_20.fsm.txt:119 |
| Ant_20.3 | Enemy Break Cage (2) / Corpse Pilgrim Fly | FSM Spawn Item On Land 0f0fcb1712 | Freed fly's corpse drops collectable on landing | sure | Cage is saved; a breaking hit frees the fly only in one game and its pickup appears there. | The cage break (cage id); the corpse and pickup replay natively. | Pickup for both or the breaker only. | scenes/Ant_20.fsm.txt:341 |

## Ant_21

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_21.1 | ant_item_string | FSM Handle Break 2503c75c1b | Break string reveals an item holder | sure | Saved break: one game drops the item, the other still has the string. | The break (string id). | | scenes/Ant_21.fsm.txt:80 |
| Ant_21.2 | ant_item_string | C# ToolGameObjectActivator | Shows or hides by tool unlock | maybe | Reads the local save's tool unlocks, so players with different tools see different dressing. | Nothing live. | | scenes/Ant_21.objects.txt:1060 |
| Ant_21.3 | Battle Scene Control / Battle Scene Extra, Battle Scene Init | FSM Start Control 5ad0b66283, 41ef70779c | Guard waking sets endless guard range | sure | Start signals for the two battle scenes, triggered by the local ant guards waking. | Battle start (guard woke). | Lock both players in, or only the trigger player. | scenes/Ant_21.fsm.txt:95, scenes/Ant_21.fsm.txt:1215 |

## Ant_Merchant

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_Merchant.1 | Ant Merchant | FSM Dialogue 3712965521 | Merchant meets hero, opens shop | maybe | Dialogue and shop (rosaries, stock) are personal; sets MetAntMerchant. | Nothing live; shop stock only if stock is shared. | Shared stock or per-player stock. | scenes/Ant_Merchant.fsm.txt:4 |
| Ant_Merchant.2 | Ant Merchant | FSM Rest Animation 5dbb73d3f4 | Idle rest animation | no | Cosmetic. | | | scenes/Ant_Merchant.fsm.txt:104 |

## Ant_Queen

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Ant_Queen.1 | Memory Group | FSM To Memory 38658b6e31 | THREAD MEMORY BURST: transitions into the memory | sure | Fades out, sets death respawn, clears hero effects and starts a scene transition for the local hero only. | The transition start (burst), so the partner follows. | Partner enters the same memory, stays outside, or gets a separate copy. | scenes/Ant_Queen.fsm.txt:289 |
| Ant_Queen.2 | door_wakeOnGround ×6 | FSM Wake Up ff2a20c330 | Hero wakes on ground after memory | sure | Blocks input, blanks screen, regains control on arrival; must match the partner's arrival. | Arrival moment (partner arrives too). | | scenes/Ant_Queen.fsm.txt:299 |
| Ant_Queen.3 | Ant Queen BG NPC | FSM Control 603ad32f91 | Memory NPC: talk, deep melody, memory | maybe | Reads metAntQueenNPC, hasNeedolinMemoryPowerup and can start the memory zone; dialogue is personal. | Nothing live. | | scenes/Ant_Queen.fsm.txt:382 |
| Ant_Queen.4 | Deep Memory Tension | FSM Control 4735b91547 | Tension music in deep memory zone | no | Audio. | | | scenes/Ant_Queen.fsm.txt:494 |
| Ant_Queen.5 | Deep memory zone effect, Memory Heartbeat Control, bloom activator ×N, Head | C# DeepMemoryApproachEffect, MemoryHeartBeat, ActivateRandomChildrenTimer, AnimatorTalkAnimNPC | Heartbeat, fog, random blooms, NPC head talk | no | Local visuals, audio and camera FOV pulses. | | | scenes/Ant_Queen.objects.txt:154, scenes/Ant_Queen.objects.txt:155, scenes/Ant_Queen.objects.txt:156, scenes/Ant_Queen.objects.txt:194 |

## Summary

Events that should pull the partner to the player who triggered them (each is a lock-in or forced cutscene on the local hero only):

- Greymoor_01.7 Hornet's crash-through-the-floor intro: hero is teleported, frozen and the floor breaks; the partner would otherwise stand in the old room state. Cheapest: sync floor-broken, partner stays free.
- Greymoor_05.1 Garmond wake and roar: takes control of the hero for the meet dialogue.
- Greymoor_05_boss.1, Greymoor_08_boss.2 Vampire Gnat (Moorwing) boss fights, and Ant_19.1 Bone Flyer Giant boss: arena start reads the local hero's range, closes gates and camlock only there; boss death must complete in both games.
- Battle scenes with gates: Greymoor_04.1 and .2, Greymoor_20c.1 and .2, Ant_02.1 and .2, Ant_04_mid.9 and Ant_05b.2, Ant_08.1, Ant_21.3: gates and camera locks close only for the trigger player.
- Ant_04_mid.2 Shakra ring shout: first-meet cutscene that freezes the hero.
- Greymoor_20b.1 and Ant_20.2 crest chapel doors: door slams, respawn is set and the game saves for the local hero.
- Greymoor_08_mapper.1 Mapper spar duel: duel against the NPC with a non-lethal respawn for the accepter.
- Greymoor_08_caravan.4 and Ant_Queen.1 and .2: scene transitions (caravan travel arrival, memory entry) separate the players by design; decide whether the partner follows.
- Ant_17.1 fake trap bench: sitting freezes the hero and springs a trap; partner needs the trap sprung.
- Halfway_01.8 cellar trapdoor: a lone scene transition.

Persistent world changes that the partner never sees live (need the trigger event shared; everything after replays natively): Greymoor_01.2 bell chain plat (lever), Greymoor_02.5 balloon crane (lever), Greymoor_05.3 and Greymoor_08.4 Wall Breaker platforms, Greymoor_06.1 lever drop plat, Greymoor_08.5 and Greymoor_16.3 cut vines, Greymoor_04.6 propellers (every lever hit counts toward the updraft), Ant_20.3 and Ant_21.1 cages and strings, Greymoor_24.2 strung-up flea, Greymoor_03.7 lift.

Choices that affect many rows:

1. Boss and battle targeting: with two players, which hero a boss or ally AI chases (Greymoor_05_boss.2, Greymoor_08_boss.2, Ant_19.1, Greymoor_08.3, Ant_04_mid.5) is a design question; enemy AI that picks "the hero" now picks the local one.
2. Hornet death in a boss fight: Greymoor_05_boss.1 and Greymoor_08_boss.2 add to vampireGnatDeaths and (in 05) bring Garmond to aid when the hero dies; with two heroes decide whose death counts and whether the fight resets.
3. PlayerData-gated room variants (Greymoor_01.4, Greymoor_05.4, Greymoor_07.1/.3, Greymoor_15.1, Ant_04_mid.3, Halfway_01.4): rooms and enemy suites are chosen at load from flags such as blackThreadWorld, CrowCourtInSession, greymoor05_clearedOut and enemyGroupAnt04; if the two saves differ the partner loads a different room, so decide which player's flags win.
4. Loose pickups (Ant_02.5 AntRegion): ants carry corpses and dropped currency away in each game independently; shared pickups need an owner or they vanish for one player only.
5. Gaps seen: Memory Orb Challenge font (Greymoor_15b.1) heals the activator to full; decide whether challenge progress and the heal are shared. Shop-type NPCs (Ant_Merchant, Mapper, Bartender, Caravan Troupe Member) are personal currency and stock unless the user wants shared purchases.
