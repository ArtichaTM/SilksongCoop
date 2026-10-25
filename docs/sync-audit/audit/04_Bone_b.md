# Audit 2.04: Bone_05b to Bone_Steel_Servant

Slice: catalog/Bone.md from `## Bone_05b` to the end. Several scenes have an Act 3 copy of their content under `Black Thread States/Black Thread World`; the same logic applies to both copies.

## Bone_05b

no new elements

## Bone_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_06.1 | Flea Rescue Branches | FSM Control 88e5961aed | Nail hits free trapped flea | sure | Rescued flag is saved per scene, so one game still shows the trapped flea after the other freed it, and both can collect it. | The final break hit (RESCUE); each game then plays the flea release itself. | Flea credit for both players, or only for the player who freed it. | scenes/Bone_06.fsm.txt:3647 |
| Bone_06.2 | Flea Rescue Activation | FSM Control 5a20021012 | Flea emerges, flies off, grants flea item | sure | Grants the quest flea item to the local hero, so the other player gets nothing and may see the flea again. | Covered by the break hit of Bone_06.1. | Same as Bone_06.1. | scenes/Bone_06.fsm.txt:3742 |
| Bone_06.3 | Flea Distressed | FSM Call Out 80ebae6e57 | Flea faces hero, barks when near | no | Cosmetic reaction to alert range only. | | | scenes/Bone_06.fsm.txt:3695 |
| Bone_06.4 | Shell Fossil Mimic AppearVariant | FSM Drop Shards cb28bc661f | Each hit flings random shell shards | sure | Random shard drops and a saved remaining-shards counter differ per game, so shards can be farmed twice or mismatch. | The hit event, so one game rolls the shard drop and both show the same. | Shards for the hitter only, or shared pickups. | scenes/Bone_06.fsm.txt:4477 |
| Bone_06.5 | Shell Fossil Mimic AppearVariant | FSM Control eee403e461 | Fossil ambushes, flies at hero, charges | sure | Wakes on damage, alert or needolin and chases only the hero of the game running it. | Enemy state, as for any enemy. | | scenes/Bone_06.fsm.txt:4513 |
| Bone_06.6 | Water Drip ×N | C# WaterDrip | Random timed ambient water drips | no | Pure ambience. | | | scenes/Bone_06.objects.txt:61 |
| Bone_06.7 | Flea Rescue Branches ×2 | C# SceneLintLogger | Editor lint logging | no | Editor only. | | | scenes/Bone_06.objects.txt:2098 |

## Bone_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_07.1 | Bone Crawler (4) | FSM Control b6e11d2e6a | Roof crawler, spike-stabs when hero near | sure | Faces and attacks the hero in its own game, so the other player is ignored. | Enemy state, as for any enemy. | | scenes/Bone_07.fsm.txt:720 |
| Bone_07.2 | Corpse Bone Crawler(Clone) | FSM corpse_chunk_flinger a7b0de1002 | Flings corpse chunks on death | no | Cosmetic debris. | | | scenes/Bone_07.fsm.txt:794 |
| Bone_07.3 | tool_metal_activate | FSM FSM 5d6979b7c8 | Shows ore deposit if nothing blocks | no | Local setup check at scene load. | | | scenes/Bone_07.fsm.txt:1759 |
| Bone_07.4 | tool_metal_deposit | FSM Control f59d366ebd | Nail-breakable tool metal ore, drops item | sure | Break is saved per scene, so the ore stays whole in the other game and its item can be taken twice. | The final break hit; the rock-broken look and item reveal follow in both. | Item for the breaker only, or one shared pickup. | scenes/Bone_07.fsm.txt:1775 |

## Bone_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_08.1 | Bone Hunter Fly | FSM Control aa7f1ccb87 | Flyer keeps distance, throws javelins, slashes | sure | Targets the local hero and picks attacks by random roll, so the other player is never aimed at and rolls diverge. | Enemy state, as for any enemy. | | scenes/Bone_08.fsm.txt:4 |
| Bone_08.2 | Bone Hunter Fly | FSM Go Up 97b288e6c9 | Relays GO UP event | no | Trivial helper. | | | scenes/Bone_08.fsm.txt:519 |
| Bone_08.3 | lever | C# Lever | Nail-hit lever unfolds extender platforms | sure | Lever state is saved per scene, so the other game's platforms stay folded until reload. | The lever hit; platforms unfold natively in both. | | scenes/Bone_08.objects.txt:253 |
| Bone_08.4 | lever | C# SetTriggerRandom | Random shine animation on lever | no | Cosmetic. | | | scenes/Bone_08.objects.txt:253 |
| Bone_08.5 | belltown_extender_plats bottom | C# ExtenderPlatsController | Unfolds platform chain when lever opens | sure | Only runs when the lever fires. | Nothing extra; follows Bone_08.3. | | scenes/Bone_08.objects.txt:1284 |
| Bone_08.6 | Link ×35 | C# ExtenderPlatLink | Rotating link segment of the chain | no | Visual part driven by the controller. | | | scenes/Bone_08.objects.txt:1286 |
| Bone_08.7 | Spawner ×44 | C# ReplaceWithTemplate | Swaps itself for a prefab on Awake | no | Static setup. | | | scenes/Bone_08.objects.txt:1288 |

## Bone_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_09.1 | Dock Worker ×3 | FSM Behaviour 0f3258d82e | Walker: jabs hero, prays, digs, or joins battle | sure | Walks and faces toward the local hero only; the Garmond-scene copies wake when the fight starts. | Enemy state, as for any enemy. | | scenes/Bone_09.fsm.txt:325 |
| Bone_09.2 | Dock Worker ×3 | C# WalkerV2 | Walk, turn, chase hero in aggro range | sure | Chase direction always uses the local hero. | Enemy state; same as Bone_09.1. | | scenes/Bone_09.objects.txt:1902 |
| Bone_09.3 | Garmond Fighter | FSM Water Detect 77204cfd33 | Detects water surface below | no | Local terrain query. | | | scenes/Bone_09.fsm.txt:590 |
| Bone_09.4 | Garmond Fighter | FSM Bind Response 7a905535af | Plays friend-bind effect when hero binds near | maybe | Reacts to a bind by the local hero only; a partner bind nearby would not trigger it. | The bind event, only if the effect matters. | | scenes/Bone_09.fsm.txt:609 |
| Bone_09.5 | Garmond Fighter | FSM Control 92653430e1 | Garmond fights enemies or Hornet, sets PD flags | sure | Boss-like encounter: auto-targets the nearest valid target, sets PD encounter bools, can lock camera, respawn marker and fight Hornet. | Encounter start and its state, as a boss. | Pull the partner into the fight, or let each game run its own sparring. | scenes/Bone_09.fsm.txt:621 |
| Bone_09.6 | Garmond Fighter | FSM Gnat Battle End 298c0d1f2e | After vampire gnat dies, Garmond walks to victory spot | sure | Triggered by a boss death event local to one game. | The gnat-defeated event. | | scenes/Bone_09.fsm.txt:1618 |
| Bone_09.7 | Citadel Library NPC | FSM Dialogue e5f1ee6ae2 | Garmond library talk, offer, saved PD flags | sure | Sets garmondLibrary PD bools and offers an item exchange per player. | Nothing live; the PD bools. | Per-player conversation, or shared story state. | scenes/Bone_09.fsm.txt:1636 |
| Bone_09.8 | Defeated NPC | FSM Dialogue 2bc9053658 | Garmond line after beating Hornet | maybe | Only reached after a fight outcome in one game. | | | scenes/Bone_09.fsm.txt:1722 |
| Bone_09.9 | Victory NPC | FSM Dialogue 10cad4e471 | Garmond talks after fight, sets meet PD | sure | Sets metGarmond and similar PD bools only for the local hero. | The PD bools. | Per-player conversation, or shared story state. | scenes/Bone_09.fsm.txt:1794 |
| Bone_09.10 | Stun Effect | FSM Stun Effect ed3d28d568 | Spawns stun visual | no | Cosmetic. | | | scenes/Bone_09.fsm.txt:1774 |
| Bone_09.11 | Tink Detector | FSM Tink 5fb7e9d518 | Sends TINK when something touches it | no | Feeds Garmond Control; no state of its own. | | | scenes/Bone_09.fsm.txt:1783 |
| Bone_09.12 | Dock Flyer | FSM Behaviour 062e9c6d81 | Flyer hovers and throws rocks at hero | sure | Aims and flies relative to the local hero only; random fire speed. | Enemy state, as for any enemy. | | scenes/Bone_09.fsm.txt:3384 |
| Bone_09.13 | Dock Flyer | FSM Go Direction Velocity 5b3f63c669 | Pushes flyer up or sideways on event | no | Small movement helper. | | | scenes/Bone_09.fsm.txt:3634 |
| Bone_09.14 | Music Control | FSM Control 964219c352 | Sets music by entry gate | no | Local audio. | | | scenes/Bone_09.fsm.txt:4488 |
| Bone_09.15 | Wall Detector | C# EnemyWallRange | Tells flyer if a wall is near | no | Terrain only. | | | scenes/Bone_09.objects.txt:1712 |
| Bone_09.16 | Target Range | C# TrackTriggerObjectsLineOfSight | Counts targets in line of sight | sure | Garmond picks its target from this range, which only holds colliders in the local game. | Part of Bone_09.5. | | scenes/Bone_09.objects.txt:2505 |

## Bone_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_10.1 | Fixer Sitting Bone_10 | FSM Dialogue dedd83fdea | Fixer talks, ends delivery quest, rewards item | sure | EndQuest and reward item go to the local hero only. | Nothing live; quest state per player. | Quests per player, or shared quest log. | scenes/Bone_10.fsm.txt:851 |
| Bone_10.2 | Caravan | FSM Quest End Sequence 6d1a3c5030 | Fades out, loads caravan travel scene | sure | Takes control, sets caravan location PD and changes scene, which splits the players. | The trigger event (QUEST END TRAVEL). | Pull partner along, hold them in place, or leave them free. | scenes/Bone_10.fsm.txt:892 |
| Bone_10.3 | Caravan Ambient Fly Audio | FSM Control 0d7d279b4f | Fly buzz audio | no | Audio. | | | scenes/Bone_10.fsm.txt:950 |
| Bone_10.4 | Caravan Lech | FSM Dialogue bcfeb311b2 | Lech talks, spa offer, sets PD flags | sure | Sets CaravanLech PD bools and unlocks the spa door for the local hero. | Nothing live; the PD bools. | Per-player conversation, or shared story state. | scenes/Bone_10.fsm.txt:1052 |
| Bone_10.5 | Caravan Lech | FSM Rest Animation 620975fc5f | Idle animation | no | Cosmetic. | | | scenes/Bone_10.fsm.txt:1169 |
| Bone_10.6 | Caravan Lech Wounded | FSM Wounded Behaviour 641f4ce5ef | Wounded Lech reacts to hits, talks | maybe | Physics and nail reactions run in each game separately; PD spoken flag only. | | | scenes/Bone_10.fsm.txt:1206 |
| Bone_10.7 | Caravan Troup Member Short/Tall | FSM NPC Title 06d0a40ebe | Shows NPC name title | no | Cosmetic. | | | scenes/Bone_10.fsm.txt:1420 |
| Bone_10.8 | Caravan Troup Member Short | FSM Alert Reaction 6d7a9b52e0 | Turns to look at hero | no | Cosmetic. | | | scenes/Bone_10.fsm.txt:1430 |
| Bone_10.9 | Caravan Troup Member Tall | FSM Alert Reaction 9117c34f04 | Turns to look at hero | no | Cosmetic. | | | scenes/Bone_10.fsm.txt:1462 |
| Bone_10.10 | Caravan Troupe Hunter | FSM Dialogue 20a7938c43 | Opens pin shop dialogue | maybe | Opens a menu and spends rosaries of the local hero only. | | Per-player map pins, or shared pins. | scenes/Bone_10.fsm.txt:1484 |
| Bone_10.11 | Caravan Troupe Hunter | C# CaravanTroupeHunter | Shop selling map pin packs | maybe | Purchase sets hasPinFlea PD bools of the buyer. | The PD bool if pins are shared. | Per-player map pins, or shared pins. | scenes/Bone_10.objects.txt:2096 |
| Bone_10.12 | Caravan Troupe Leader Bone NPC | FSM Dialogue 2905d211a1 | Offers Save the Fleas quest | sure | Quest accept and met flags are per player; flea progress comes from rescues. | Nothing live; quest state. | Quest per player, or shared quest log. | scenes/Bone_10.fsm.txt:1630 |
| Bone_10.13 | door2 | FSM FSM 3e8bcd0297 | Spa door opens or closes | maybe | State follows Lech PD flags, leading to Room_Caravan_Spa. | The door state, or the PD flag behind it. | | scenes/Bone_10.fsm.txt:2561 |
| Bone_10.14 | Giant Flea Idle/Sleeping | FSM Behaviour 6eb418a1fe | Ambient flea idles, startles near hero | no | Cosmetic. | | | scenes/Bone_10.fsm.txt:2571 |
| Bone_10.15 | Caravan Troupe Flea Saved ×N | FSM Behaviour d6e1be870c | Ambient saved flea idles, sings | no | Cosmetic. | | | scenes/Bone_10.fsm.txt:2985 |
| Bone_10.16 | Flea Bench States | FSM States c5f3752cd1 | Random flea on or off bench | no | Cosmetic random choice. | | | scenes/Bone_10.fsm.txt:4406 |
| Bone_10.17 | Fleas | C# SavedFleaActivator | Enables random fleas by rescued count | no | Crowd size only; harmless if it differs. | | | scenes/Bone_10.objects.txt:2339 |
| Bone_10.18 | Caravan flea ×276 | C# SendEventToRegisterMethod | Forwards an event name | no | Helper for ambient events. | | | scenes/Bone_10.objects.txt:2366 |

## Bone_11

no new elements

## Bone_11b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_11b.1 | Silk Spool | FSM Bob 2c1c0d1f8a | Bobbing animation | no | Cosmetic. | | | scenes/Bone_11b.fsm.txt:4 |
| Bone_11b.2 | Silk Spool | FSM Control 1e233761e3 | Pickup: silk spool upgrade, locks hero, UI | sure | One-time pickup saved per scene with a PD bool; taker gets the upgrade and a short control lock, the other game keeps the spool. | The pickup event, so the spool vanishes in both. | Both players get the upgrade, or only the taker. | scenes/Bone_11b.fsm.txt:10 |
| Bone_11b.3 | Plate | C# PersistentPressurePlate | Hero-weight plate, saved once pressed | sure | A plate only reacts to the local hero, so the partner pressing it does nothing live. | The plate press. | | scenes/Bone_11b.objects.txt:293 |
| Bone_11b.4 | Bone Drop Plat Sequence | C# AnimatorControlSequence | Plays platform drop animation once | sure | Appears to be the plate result (inferred from the scene); would play only in the pressing game. | Nothing extra; follows Bone_11b.3. | | scenes/Bone_11b.objects.txt:464 |
| Bone_11b.5 | Camera Rumble | C# CameraShakeWhileEnabled | Camera shake while enabled | no | Cosmetic. | | | scenes/Bone_11b.objects.txt:503 |

## Bone_12

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_12.1 | _SceneManager | FSM FSM 95dd27e316 | Deprecated, disables itself | no | Empty. | | | scenes/Bone_12.fsm.txt:4 |
| Bone_12.2 | Ladybug Craft Pickup | FSM FSM e8a321870b | Spend tool metal to craft item, cutscene | sure | Consumes the local hero's tool metal, walks hero to table with fade, grants item; saved once per scene. | The craft completion, so the table is used up in both. | Each player crafts their own, or one craft shared. | scenes/Bone_12.fsm.txt:9 |
| Bone_12.3 | Jail Scene 1 | FSM Control Pilgrim f5bb1994bd | Swaps prisoner dummy for pilgrim on door break | sure | Follows the saved door break; one game keeps the dummy. | Follows Bone_12.4. | | scenes/Bone_12.fsm.txt:889 |
| Bone_12.4 | jail_door ×2 | FSM Control 96bc6faecf | Nail-breakable jail door, saved | sure | Break is saved per scene and frees the occupant, so the other game keeps its door and occupant. | The final break hit. | | scenes/Bone_12.fsm.txt:911 |
| Bone_12.5 | Jail Scene 2 | FSM FSM 1adb06ef19 | Switches Grindle dummy to freed Grindle | sure | Follows the door break of Bone_12.4. | Follows Bone_12.4. | | scenes/Bone_12.fsm.txt:1494 |
| Bone_12.6 | Thief Dummy | FSM Conversation af821b9bb2 | Trapped Grindle waves and talks | no | Dialogue before the door breaks. | | | scenes/Bone_12.fsm.txt:1578 |
| Bone_12.7 | Thief NPC Save | FSM Conversation d52769bf9e | Freed Grindle, sets grindleReleasedFromBonejail | sure | Sets a PD bool and dialogue progress for the local hero. | Nothing live; the PD bool. | | scenes/Bone_12.fsm.txt:1648 |
| Bone_12.8 | Thief NPC Saved | FSM Conversation 3e769bfa06 | Grindle offers rosary-for-service deals | sure | Takes rosaries and advances a saved step counter per player. | Nothing live; saved step. | Per-player deals, or shared progress. | scenes/Bone_12.fsm.txt:1705 |
| Bone_12.9 | Lady Bug Large | FSM Convo 914b8c31c3 | Pin gallery host: offer, fee, start challenge | sure | Takes rosaries, sets WasInPinChallenge and starts the minigame for the local hero. | The challenge start. | One player at a time, or each plays alone. | scenes/Bone_12.fsm.txt:1849 |
| Bone_12.10 | Lady Bug Large | FSM Duck Reaction 4873db5615 | Ducks when attacked nearby | no | Cosmetic. | | | scenes/Bone_12.fsm.txt:2242 |
| Bone_12.11 | Lady Bug Small Idle | FSM Control 376e085b33 | Faces hero | no | Cosmetic. | | | scenes/Bone_12.fsm.txt:2271 |
| Bone_12.12 | Pin Challenge | FSM Pin Thunk Tracker 5e06dbbeb4 | Counts missed pins, ends after fails | sure | Counts only the local hero's missed throws. | Part of Bone_12.13. | | scenes/Bone_12.fsm.txt:2275 |
| Bone_12.13 | Pin Challenge | FSM Pin Challenge d7f293f099 | Pin throwing minigame: camlock, target hits, payout | sure | Locks camera, moves hero to a start spot and pays rosaries; counts pinGalleriesCompleted; random target patterns. | The challenge start and result. | One player at a time, or each plays alone. | scenes/Bone_12.fsm.txt:2296 |
| Bone_12.14 | Lady Bug Tiny | FSM Horizontal Motion 5837c27651 | Moves target bug side to side | maybe | Random speed and distance; matters only if the minigame is shared. | | | scenes/Bone_12.fsm.txt:2549 |
| Bone_12.15 | Lady Bug Tiny | FSM Gong Control 63ca7e7a71 | Target bug movement and hit scoring | maybe | Random positions; a hit scores for the local hero only. | The target position, only if the minigame is shared. | | scenes/Bone_12.fsm.txt:2574 |
| Bone_12.16 | Music | FSM Control 9115fb35f0 | Challenge music | no | Audio. | | | scenes/Bone_12.fsm.txt:2843 |
| Bone_12.17 | Pin Game Counter Variant | C# SimpleCounter | Challenge hit counter UI | no | UI. | | | scenes/Bone_12.objects.txt:956 |
| Bone_12.18 | Icon | C# CurveScaleAnimation | Counter icon animation | no | Cosmetic. | | | scenes/Bone_12.objects.txt:959 |
| Bone_12.19 | break_drape_mid_01 (1) | C# ParentSwayConfig, PlayChainSound | Hanging drape sway and sound | no | Cosmetic. | | | scenes/Bone_12.objects.txt:1020 |
| Bone_12.20 | break_drape_mid_01 (1) | C# ChainInteraction, ChainAttackForce | Nail-cuttable hanging drape | no | Cut drape is only decor and is not saved. | | | scenes/Bone_12.objects.txt:1022 |
| Bone_12.21 | jail_door_0007_2 ×N | C# SetZPerHeroSide | Depth swap by hero side | no | Visual. | | | scenes/Bone_12.objects.txt:1035 |
| Bone_12.22 | Slab Chain Lamp chain ×2 | C# AutoGenerateHangingRope | Generates hanging rope | no | Visual. | | | scenes/Bone_12.objects.txt:1081 |
| Bone_12.23 | Pin Challenge | C# PinChallenge | Pauses bench while pin tools thrown | sure | Part of the single-hero minigame. | Part of Bone_12.13. | One player at a time, or each plays alone. | scenes/Bone_12.objects.txt:1211 |
| Bone_12.24 | Gong Collider, Dummy_01/02 | C# AttackDetonator | Marks hittable target flags | maybe | Marker for hits; used for the pin target. | | | scenes/Bone_12.objects.txt:1214 |
| Bone_12.25 | Fail Pole ×N | C# AnimatorSetStartTime | Randomises animation start time | no | Visual. | | | scenes/Bone_12.objects.txt:1240 |
| Bone_12.26 | Pole ×3 | C# SpriteExtruder | Pole sprite extrusion | no | Visual. | | | scenes/Bone_12.objects.txt:1243 |

## Bone_14

no new elements

## Bone_15

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_15.1 | Audio Loop Tension | FSM FSM 05eb6b4cc1 | Tension audio on enter | no | Audio. | | | scenes/Bone_15.fsm.txt:4 |
| Bone_15.2 | Boulders Battle | FSM Drop 90d530a06d | Drops random boulders during Skull King fight | sure | Picks random boulders per game, so falling hazards differ and would hit only one hero. | Which boulder drops and when. | | scenes/Bone_15.fsm.txt:170 |
| Bone_15.3 | Corpse Skull King SkullFragment(Clone) | FSM Death 5b96ca3bce | Skull King death: sets PD, fanfare, ends fight | sure | Sets skullKingDefeated, plays fanfare and triggers SKULL KING KILLED; both games must end together. | The boss death. | | scenes/Bone_15.fsm.txt:4272 |
| Bone_15.4 | Run Loop | FSM lowpass f98dd25243 | Audio lowpass in vents | no | Audio. | | | scenes/Bone_15.fsm.txt:4351 |
| Bone_15.5 | Hunter Fan Outside Rummage (1) | FSM Dialogue f4cf82f392 | Hunter fan NPC talk | no | Flavour only; talked flag local. | | | scenes/Bone_15.fsm.txt:4400 |
| Bone_15.6 | Corpse | C# Tk2dSpriteSetKeywordsConditional | Shader keyword toggle | no | Visual. | | | scenes/Bone_15.objects.txt:1260 |
| Bone_15.7 | Thread Spinner | C# ThreadSpinner | Nail-hit spool gives silk each hit | sure | Each hit gives silk to the hitter and shrinks a saved spool, so the other game's spool and silk differ. | The hit, so the spool shrinks in both. | Shared spool, or one spool per player. | scenes/Bone_15.objects.txt:1980 |
| Bone_15.8 | Thread Spinner | C# FlashAnimator, ResetDynamicHierarchy | Flash and reset helpers | no | Visual. | | | scenes/Bone_15.objects.txt:1980 |
| Bone_15.9 | Fling Part | C# CollisionEnterEvent | Fires event on collision | no | Debris sound or effect. | | | scenes/Bone_15.objects.txt:1983 |

## Bone_16

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_16.1 | temp fog ×N | C# SetShaderPropertyRotation | Rotates fog shader property | no | Visual. | | | scenes/Bone_16.objects.txt:79 |
| Bone_16.2 | Bone Carriage | C# ManualLift | Carriage moves when hero rides or calls it | sure | Moves only for the local hero standing on its buttons, and keeps a saved unlock state. | Carriage position and unlock. | Partner counts as pressing the plate, or each rides alone. | scenes/Bone_16.objects.txt:689 |
| Bone_16.3 | struts | C# CogRotationController | Spins cogs with lift | no | Visual. | | | scenes/Bone_16.objects.txt:705 |
| Bone_16.4 | Hornet_pressure_plate_small L/R | C# TempPressurePlate | Plate calls or unlocks the lift | sure | Reacts to the local hero only. | The plate press and release. | Same as Bone_16.2. | scenes/Bone_16.objects.txt:824 |

## Bone_17

no new elements

## Bone_18

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_18.1 | Battle End | FSM Control 2730e689a6 | Battle end: quest complete, control lock, reward pickup | sure | Locks control and sets invincible, completes the quest, enables the silk spool or locket pickup and opens gates. | The BATTLE END event. | Reward pickup for both, or first taker. | scenes/Bone_18.fsm.txt:4 |
| Bone_18.2 | Battle Gate | FSM BG Control acbadc7630 | Arena gate closes and opens | sure | Closes only for the hero in the arena; the partner's gate stays open. | The close and open events. | Lock the partner in too, or leave them out. | scenes/Bone_18.fsm.txt:31 |
| Bone_18.3 | Battle Gate Bone | FSM BG Control 94573c783b | Bone arena gate | sure | Same as Bone_18.2. | Same as Bone_18.2. | Same as Bone_18.2. | scenes/Bone_18.fsm.txt:90 |
| Bone_18.4 | Battle Gate Pusher | FSM Control 2838b80cbe | Pushes hero out of gate zone | sure | Follows the gate; pushes only the local hero. | Follows gate events. | | scenes/Bone_18.fsm.txt:147 |
| Bone_18.5 | Pilgrim Stomper Fight NPC | FSM Dialogue 069abe7c6a | Starts the pilgrim fight by talk or wake | sure | The talking hero starts the arena fight; the other player is not in the arena. | The fight start. | Pull the partner into the arena, or let them join by walking in. | scenes/Bone_18.fsm.txt:156 |
| Bone_18.6 | Pilgrim Corpse Spawner | FSM Control 349cc95b0e | Wave spawns an enemy from pilgrim corpse | sure | Spawning an enemy per wave must match in both games. | Wave spawn events. | | scenes/Bone_18.fsm.txt:2873 |
| Bone_18.7 | Spawn Sack | FSM Control dfe5aefcf3 | Wave breaks sack, spawns enemy | sure | Same as Bone_18.6. | Same as Bone_18.6. | | scenes/Bone_18.fsm.txt:3096 |
| Bone_18.8 | Pilgrim Stomper NPC | FSM Dialogue 662bd61662 | Quest giver for pilgrim fight | sure | Quest accept and met PD flags are per player. | Nothing live; quest state. | Quest per player, or shared quest log. | scenes/Bone_18.fsm.txt:4176 |
| Bone_18.9 | Corpse Pilgrim 03 Transient(Clone) | C# CorpseRegular | Enemy corpse effects | no | Cosmetic. | | | scenes/Bone_18.objects.txt:340 |

## Bone_19

no new elements

## Bone_Steel_Servant

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Bone_Steel_Servant.1 | Steel Servant Scene | FSM Control 3b5a469bb6 | Start range captures hero, ritual talk, summons boss | sure | Forced cutscene, camlock and boss summon trigger when the local hero enters; sets EncounteredSummonedSaviour. | The encounter trigger. | Pull partner in and freeze them for the cutscene, freeze only on entry, or leave free. | scenes/Bone_Steel_Servant.fsm.txt:4 |
| Bone_Steel_Servant.2 | entrance_door | FSM FSM f4b1b5f1eb | Entrance door closes during battle | sure | Closes only for the hero who started the battle. | The BG CLOSE and BG OPEN events. | Lock the partner in too, or leave them out. | scenes/Bone_Steel_Servant.fsm.txt:87 |
| Bone_Steel_Servant.3 | Music Control | FSM Control 8acc2a3ba8 | Music on and off by range | no | Audio. | | | scenes/Bone_Steel_Servant.fsm.txt:100 |
| Bone_Steel_Servant.4 | Abyss Mass | FSM Control cda00ca8f2 | Boss: three phases, dash, shot, whip, vomit | sure | Boss with random attacks; aims at the local hero and roars, stunning the hero. | Boss state, as for any boss. | | scenes/Bone_Steel_Servant.fsm.txt:110 |
| Bone_Steel_Servant.5 | Damager | FSM FSM 6cceb1acbf | Ball attack damage and multi-hit on hero | sure | Damages only the local hero it overlaps. | Which hero is hit, per game. | | scenes/Bone_Steel_Servant.fsm.txt:246 |
| Bone_Steel_Servant.6 | Abyss Heavy Vomit Attack | FSM Control 036fd9252f | Vomit volley with random sideways speed | sure | Random shot velocity must match across games. | The attack start and rolls. | | scenes/Bone_Steel_Servant.fsm.txt:357 |
| Bone_Steel_Servant.7 | Steel Servant NPC | FSM Control ee9b444758 | Post-fight talk, starts Steel Sentinel Pt2 quest | sure | Starts a quest for the talker only; scuttles away afterward. | Nothing live; quest state. | Quest per player, or shared quest log. | scenes/Bone_Steel_Servant.fsm.txt:564 |
| Bone_Steel_Servant.8 | Black Thread Enemy Effect | C# ParticleCulling | Culls particles by distance | no | Visual. | | | scenes/Bone_Steel_Servant.objects.txt:864 |

## Summary

Events that should pull the partner to the player who triggered them (or at least hold them), each with its reason:

1. Bone_Steel_Servant.1: entering the start range captures the hero in a ritual cutscene, locks the camera and summons the Abyss Mass boss. The partner would otherwise miss the cutscene or be left outside the arena.
2. Bone_Steel_Servant.2 and Bone_18.2/3: arena doors close only for the hero in the fight. Decide whether the partner is locked in or kept outside.
3. Bone_18.5: the pilgrim fight starts from one hero's dialogue or wake trigger; all waves and gates run in that game.
4. Bone_10.2: the caravan quest-end sequence fades the screen and changes scene (Room_Caravan_Interior_Travel), which splits the players across rooms.
5. Bone_09.5: the Garmond encounter takes the camera and respawn marker and may turn on Hornet; the partner would not see or be part of the fight.
6. Bone_15.3 and Bone_15.2: Skull King death and boulder rolls must end the boss together in both games, which needs the Skull King fight events from the earlier catalog slice.

Choices only the user can make:

1. Pickups and collectables: Silk Spool upgrade (Bone_11b.2), tool metal ore item (Bone_07.4), flea rescue credit (Bone_06.1/2), shell shards (Bone_06.4), silk spool at battle end (Bone_18.1). Both players get it, or only the taker.
2. Quests and NPC state (Bone_09.7/9, Bone_10.1/4/12, Bone_12.7/8, Bone_18.8, Bone_Steel_Servant.7): all set per-player PD bools, quest state and rosary payments. Per-player or shared story progress.
3. Pin gallery (Bone_12.9 to 12.15, 12.23): a single-hero minigame with a camlock, forced start position and rosary payout. One player at a time, or each plays alone.
4. Lift and plates (Bone_16.2/4, Bone_11b.3): they only count the local hero, so one player alone can never press a plate that the other is on. Decide whether the partner counts as standing there.

Gaps and notes:

1. Enemies here (Bone Crawler, Bone Hunter Fly, Dock Worker/Flyer, Mimic, Pilgrim waves, Abyss Mass) all target only the local hero and roll attacks by random choice, so their attacks must be taken from one game.
2. Persistent switches (lever, tool metal ore, jail doors, rescue branches, plate) write their saved flag only on scene exit; a live change in the other game needs the hit event itself.
3. Bone_15 and Skull King: the Skull King Behaviour/Control FSMs are not in this slice's catalog; only the corpse death and boulder dropper are covered here.
4. Notes written: audit/notes/ThreadSpinner.md, CaravanTroupeHunter.md, SavedFleaActivator.md, PinChallenge.md, TrackTriggerObjectsLineOfSight.md, EnemyWallRange.md.
