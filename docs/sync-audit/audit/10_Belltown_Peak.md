# Audit 2.10: Belltown .. Belltown_Shrine and Peak_01 .. Peak_Mask_Maker (catalog/Belltown.md, catalog/Peak.md)

Verdict key: sure = coop must handle it, maybe = depends on a design decision or only a mild desync, no = local or cosmetic.
"The other player" means the player whose game did not run the trigger. Each player runs a full game, so a hero-reading FSM in each game reads its own local hero, and every PlayerData write lands in the save of the game that ran it.
General rule used for NPC chatter: "met", conversation counters and repeat lines are per-player and rated no; shops, quests, item grants and anything that changes the room for good are rated higher.
Rows marked "follows X" add nothing to sync beyond the row they follow.

## Belltown

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown.1 | Couriers Quest Giver | FSM [Dialogue] f04146d1ac; C# SimpleQuestsShopOwner | Courier quest shop: meet, offer, start quests | maybe | Meet flag, quest start and delivery item are written only for the hero who talked, and a purchase advances the in-game clock (TimePasses). | Nothing, unless quests are shared: then the accepted quest and its delivery item. | Quest log shared between players, or each player their own courier quests. | scenes/Belltown.fsm.txt:1344, scenes/Belltown.objects.txt:312 |
| Belltown.2 | Courier Short, Courier Tall | FSM [Anim Runner] 187e17733a | Wake and wave animations | no | Pure animation, driven by the local needolin or approach. | | | scenes/Belltown.fsm.txt:1521 |
| Belltown.3 | Courier Short Bellhart | FSM [Dialogue] 0efae98e94 | Rescued courier talks, rests, wakes | no | Presence reads the local "Save Courier Short" quest state and nothing shared is written. | | | scenes/Belltown.fsm.txt:1601 |
| Belltown.4 | Crawbell (1) | FSM [Control] dd3962825f | Deposit box: install item, collect rosaries and shards | maybe | Installing consumes the item and sets CrawbellInstalled, and collecting pays the stored currency only to the hero who pressed it. | The collect event, so the stored amount is paid once. | Deposit box shared (first to collect wins) or one box per player. | scenes/Belltown.fsm.txt:1845 |
| Belltown.5 | Mapper Control | FSM [Control] ae98c30d52; C# SceneLintDisableChildren | Picks mapper state from PlayerData at scene load | no | Reads local flags only (blackThreadWorld, spinnerDefeated, mapperAway). | | | scenes/Belltown.fsm.txt:1930, scenes/Belltown.objects.txt:98 |
| Belltown.6 | Mapper Rest NPC | FSM [Dialogue] 3b047a04e6 | Act 3 mapper: talk, shop, white flower line | maybe | Shop stock and the convo flags are per save, so limited stock bought by one player stays for sale in the other game. | Nothing, unless limited stock should be shared: then the purchase. | Shop stock shared, or each wallet and stock separate. | scenes/Belltown.fsm.txt:1976 |
| Belltown.7 | Music Control | FSM [Control] c277857616 | Chooses Belltown music cue | no | Music only, reads spinnerDefeated. | | | scenes/Belltown.fsm.txt:3602 |
| Belltown.8 | Belltown Greeter Act3 | FSM [Animation Control] de7e1b651c; FSM [Dialogue] 47a1811fc5 | Act 3 greeter: faces hero, sings, talks | no | Facing and singing read the local hero and needolin, and dialogue only reads flags. | | | scenes/Belltown.fsm.txt:4469, scenes/Belltown.fsm.txt:4566 |
| Belltown.9 | Belltown Greeter NPC | FSM [Dialogue] 16029548a7 | Greeter: wave, sing, talk, house lines | no | Only the conversation counter BelltownGreeterConvo is written. | | | scenes/Belltown.fsm.txt:4610 |
| Belltown.10 | Belltown Shop NPC | FSM [Dialogue] 79cd4de870 | Shopkeeper: shop, furnishings, paint, courier lines | maybe | Furnishing and paint purchases change BelltownHouseState and stock for the buyer's save only, and the shop is paid from the buyer's wallet. | The furnishing or paint purchase, if the house should look the same for both. | House and shop shared, or per player. | scenes/Belltown.fsm.txt:4840 |
| Belltown.11 | Bagpipers Leader, Short, Tall | FSM [Animation] 3cb06c1306 | Bagpiper idle, talk, sing animations | no | Animation only. | | | scenes/Belltown.fsm.txt:5049 |
| Belltown.12 | Bagpipers Leader | FSM [Dialogue] 48fb0416ca | Bagpiper quest offer and turn-in | maybe | Quest accept and completion (TryEndQuest, consumes currency, pays reward) belong to the hero who talked. | Nothing, unless quests are shared. | Same quest-sharing choice as Belltown.1. | scenes/Belltown.fsm.txt:5089 |
| Belltown.13 | Sherma Bellhart NPC | FSM [Conversation Control] c0acfd57bb | Sherma wakes, talks, reacts to needolin | no | Only sets metSherma and plays dialogue. | | | scenes/Belltown.fsm.txt:5351 |
| Belltown.14 | Silk Strung Up Audio | FSM [Control] 073dc4a8c3 | Random rotation and audio of strung-up silk | no | Ambience. | | | scenes/Belltown.fsm.txt:5528 |
| Belltown.15 | Webbed Hero | FSM [Web Control] b5776251ed | Hero starts stuck in a web, mashes free | maybe | Only when this is the first scene the player loads: takes control, blocks inventory, blurs the camera, then returns control after the web breaks. | Nothing required; optionally mirror the stuck pose for the partner. | If the partner is already here, leave them free. | scenes/Belltown.fsm.txt:5541 |
| Belltown.16 | Cutscene Loader | C# ExtraSettings | Entry flags (relinquish control, skip normal entry) | maybe | Applies only to the hero entering the scene and pairs with the arrival cutscene, see Belltown_cutscene.1. | | | scenes/Belltown.objects.txt:287 |
| Belltown.17 | hornet_bell_door | C# TransitionPointDoorAnim | Door open and close animation at transition | no | Animation of the local hero's door. | | | scenes/Belltown.objects.txt:816 |
| Belltown.18 | gramaphone | C# CollectionGramaphone; C# PersistentAudioInstance | Plays the chosen relic music | no | Audio, stores the playing relic in the player's own save. | | | scenes/Belltown.objects.txt:855, scenes/Belltown.objects.txt:856 |
| Belltown.19 | Extender Plat Platform(Clone) ×2 | C# ExtenderPlatPlatform | Platforms on unfolding link chains | maybe | Follows ExtenderPlatsController: the chain unfolds only in the game where its lever was hit, so the other player has no platforms. | The lever hit (see notes/Lever.md and notes/ExtenderPlatsController.md). | | scenes/Belltown.objects.txt:2194 |
| Belltown.20 | Fisher_Act3 (crowd) | C# AnimatorLookAnimNPC | Crowd NPC head turns toward hero | no | Cosmetic look at the local hero. | | | scenes/Belltown.objects.txt:2985 |

## Belltown_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_04.1 | Bell Fly ×3 | FSM [Behaviour] c422cead71; FSM [Tween] ba75a6b0b7 | Hovering bell flyer, faces hero, bobs, sings | maybe | HP 30 with contact damage, but passive: it only turns toward the local hero and a kill is seen in one game only. | The kill, if dead flyers should stay dead for both. | | scenes/Belltown_04.fsm.txt:28, scenes/Belltown_04.fsm.txt:63 |
| Belltown_04.2 | Fling/Bell (Corpse Bell Fly) ×3 | FSM [Head Control] efdc9598e4 | Flung bell head of the corpse | no | Corpse physics, random angles, harmless. | | | scenes/Belltown_04.fsm.txt:144 |
| Belltown_04.3 | Bell Wall Flea / Flea Rescue Generic | FSM [Call Out] 1652d9b7cd | Caged flea: rescue gives a quest item | sure | Rescue writes the flea's scene bool and adds the quest target to the rescuer, so the flea stays caged for the partner and can be freed twice. | The rescue (RESCUE or BREAK) by object id; each game then frees the flea itself. | Does the freed flea count for both players or only the rescuer. | scenes/Belltown_04.fsm.txt:2216 |
| Belltown_04.4 | Drop Bell ×10 | FSM [Control] fdfe7ae84d | Hanging bell falls on hero, hurts, persists | sure | Fall is triggered by the local hero's alert range or a tink, damages only the local hero, and the dropped state is saved. | The drop trigger by object id; fall and landing then play in both games. | | scenes/Belltown_04.fsm.txt:2962 |
| Belltown_04.5 | Quest Bell Dropper ×14 | FSM [Control] f81ab32335 | Drops quest bells above hero | sure | Drop type (standard or silver, weighted random) and x position are rolled per game near the local hero, so the silver bell appears in different places or not at all. | The drop roll (type and x) and the silver pickup. | Quest progress for the silver bell shared or per player. | scenes/Belltown_04.fsm.txt:3482 |
| Belltown_04.6 | rosary_cache_bell_ground/flung_bell ×6 | FSM [Tink Fling] 26b128d0f5; C# RosaryCache | Hit bell flings rosaries | maybe | Each game flings its own random rosaries, so loot differs but nothing else breaks. | The hit by object id with the loot outcome, only if loot must match. | Loot per game or shared roll. | scenes/Belltown_04.fsm.txt:4308, scenes/Belltown_04.objects.txt:2622 |
| Belltown_04.7 | Belltown_floor_main ×164 | C# JitterSelfSimple | Floor tiles jitter | no | Visual. | | | scenes/Belltown_04.objects.txt:53 |

## Belltown_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_06.1 | Lift Delay | FSM [FSM] 1cb9acdd51 | After OPEN, unlocks the Mines Lift button | sure | Unlocks the persistent Mines Lift only in the game that received OPEN, so the partner's lift stays locked. | The OPEN source (the lever or switch that sends it, see notes/LiftControl.md); the unlock then follows. | | scenes/Belltown_06.fsm.txt:193 |

## Belltown_07

no new elements

## Belltown_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_08.1 | Audio Tension | FSM [Control] ab0e39067d | Tension music on boss gates close | no | Music, follows Belltown_08.4. | | | scenes/Belltown_08.fsm.txt:4 |
| Belltown_08.2 | Battle Gate Swamp ×5 | FSM [BG Control] fca54f9373 | Arena gates close and open | sure | Gates close only where the fight began, so the partner can walk out of or into the arena freely (follows Belltown_08.4). | Nothing separate: BG CLOSE and BG OPEN come from the boss start and end. | Partner locked in the arena too, or free. | scenes/Belltown_08.fsm.txt:19 |
| Belltown_08.3 | Boss Scene/Darkness | FSM [FSM] b8584f680f | Lights come up when the fight starts | no | Visual, follows Belltown_08.4. | | | scenes/Belltown_08.fsm.txt:76 |
| Belltown_08.4 | Wisp Pyre Effigy | FSM [Summon Control] b4f32f187a | Lantern boss: summons wisps and flame waves | sure | Fight starts when the local hero stays in the gate range, roars to stun that hero, picks summon spots 8 to 20 units from it, rolls flame waves with RNG, and on its end sets defeatedWispPyreEffigy. | Fight start (gate range entered), the host's summon and flame-wave rolls, and the final break. | Pull the partner in and stun them, lock them out, or let them join late. Who rolls the RNG. Boss reward to both or the finisher. | scenes/Belltown_08.fsm.txt:82 |
| Belltown_08.5 | Wisp Pyre Effigy | FSM [Wobble] ce53fdb26d | Wobble after hit | no | Visual, follows Belltown_08.6. | | | scenes/Belltown_08.fsm.txt:535 |
| Belltown_08.6 | Wisp Pyre Effigy (core) | FSM [Take Damage] 01c2189ffd | Core takes nail hits, breaks after 30 | sure | Hit count and HP live in this game's FSM, so a hit by the partner never lowers it. | Each hit on the core with its damage; FINAL BREAK follows locally. | | scenes/Belltown_08.fsm.txt:552 |
| Belltown_08.7 | Brazier Arm BL, BR, TL, TR | FSM [wisp_brazier_arm] 05904c6c82 | Lantern arm takes hits, breaks, drops shell | sure | Each arm has its own HP and break; the Total Times Hit count decides the break and drives the phase split in Belltown_08.4. | Each hit on an arm (which arm, damage); break and recoil then follow. | | scenes/Belltown_08.fsm.txt:596 |

## Belltown_basement

no new elements

## Belltown_basement_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_basement_03.1 | Collapser Small_Bell_Type ×3 | FSM [Control] 8c8fd5ade0 | Floor collapses when hero lands on it | sure | Collapse starts on a collision with the local hero and is saved, so the partner stands on an intact floor and the path differs. | The collapse trigger by object id; antic and break play in both games. | | scenes/Belltown_basement_03.fsm.txt:3252 |
| Belltown_basement_03.2 | Bell Hermit | FSM [Dialogue] 27c38c8730 | Hermit talks, sings back to needolin | no | Only conversation counters and the local needolin. | | | scenes/Belltown_basement_03.fsm.txt:3384 |
| Belltown_basement_03.3 | One Way Wall (1), (4) / Activate On Break | FSM [FSM] 79823d7cd2 | Wall shatters when broken, camera shakes | sure | The wall is a persistent barrier, so breaking it in one game leaves the way shut in the other. | The wall break by object id; effects play in both games. | | scenes/Belltown_basement_03.fsm.txt:3665 |

## Belltown_cutscene

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_cutscene.1 | Cinematic Player | FSM [Cutscene Control] d2713e8546 | First arrival video, then starts the main quest | sure | Takes the entering hero's control, blocks pause, plays a video, sets SeenBelltownCutscene and visitedBellhart, begins the Threadspun Town quest and saves. | Whether the cutscene ran (the flags it sets). | Partner watches too, is frozen while it plays, or stays free; skip allowed for whom. | scenes/Belltown_cutscene.fsm.txt:1 |

## Belltown_Room_doctor

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_Room_doctor.1 | Death Respawn Marker | FSM [FSM] 44f52e838f | Reacts when hero respawns here | no | Respawn is per player. | | | scenes/Belltown_Room_doctor.fsm.txt:75 |
| Belltown_Room_doctor.2 | Doctor Fly | FSM [Dialogue] ce2207e6df; C# NPCFlyAround | Hidden doctor, forced talk, curse cure cutscene, crest | sure | A trigger captures the local hero for a forced talk, then the cure removes the maggot state, unlocks and equips a crest, plays a cinematic and saves. | Cure accepted (BelltownDoctorCuredCurse) and the crest unlock, if shared. | Pull the partner into the forced talk, freeze them, or leave free. Cure and crest shared or per player. | scenes/Belltown_Room_doctor.fsm.txt:82, scenes/Belltown_Room_doctor.objects.txt:89 |
| Belltown_Room_doctor.3 | Doctor Fly, Audio Loop Buzz | FSM [Fly Audio] f28a8fb3a5; FSM [Control] 2f9151b53f | Buzz audio | no | Audio. | | | scenes/Belltown_Room_doctor.fsm.txt:507, scenes/Belltown_Room_doctor.fsm.txt:514 |

## Belltown_Room_pinsmith

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_Room_pinsmith.1 | Plinney Inside | FSM [Dialogue] 12abc1270f | Nail upgrade with cinematic, quest offer | sure | Fades to a cinematic and grants a Needle Upgrade (nailUpgrades) to the buyer only, and nail level changes damage dealt in that game. | The upgrade grant, if nail level is shared. | Nail level linked between players or separate; partner frozen during the cinematic or not. | scenes/Belltown_Room_pinsmith.fsm.txt:9 |

## Belltown_Room_Relic

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_Room_Relic.1 | Relic Board | FSM [Inventory Proxy] ab2aff2fe6; C# CollectableRelicBoard; C# InventoryPaneStandalone; C# InventoryPaneInput; C# InventoryItemGrid; C# InventoryCursor; C# CollectableRelicBoardItem; C# TextMeshProLineAnchor; C# AnimateSpriteIndex | Relic board UI | no | Local menu over the player's own relics. | | | scenes/Belltown_Room_Relic.fsm.txt:9, scenes/Belltown_Room_Relic.objects.txt:109, scenes/Belltown_Room_Relic.objects.txt:110, scenes/Belltown_Room_Relic.objects.txt:123, scenes/Belltown_Room_Relic.objects.txt:131, scenes/Belltown_Room_Relic.objects.txt:134, scenes/Belltown_Room_Relic.objects.txt:146 |
| Belltown_Room_Relic.2 | Relic Dealer NPC | FSM [Dialogue] af024a13bc; C# RelicBoardOwner | Takes relics from inventory, plays them | maybe | Depositing marks relics as deposited in the depositor's save only, so the partner's relics are not on the board. | Nothing, unless relics are shared: then the deposit. | Relic collection shared or per player. | scenes/Belltown_Room_Relic.fsm.txt:191, scenes/Belltown_Room_Relic.objects.txt:2 |
| Belltown_Room_Relic.3 | Librarian Tentacle ×13 | FSM [Animation] e3aff4ab15 | Tentacle animations | no | Animation. | | | scenes/Belltown_Room_Relic.fsm.txt:494 |

## Belltown_Room_shellwood

no new elements

## Belltown_Room_Spare

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_Room_Spare.1 | Farsight Cinematic Player | FSM [Sequence] 73d718cb6b | Farsight ability cinematic and item message | maybe | On CONSTRUCT FARSIGHT it takes the hero's control, plays a video and shows the Farsight get-item message. | The ability unlock, if abilities are shared. | Ability granted to both or only the owner; partner frozen during the video or not. | scenes/Belltown_Room_Spare.fsm.txt:276 |
| Belltown_Room_Spare.2 | furnishings/bed/hornet | FSM [Bed Sleep] 6332c58445 | Bed lying animation during bench rest | no | Visual, follows the bench rest. | | | scenes/Belltown_Room_Spare.fsm.txt:309 |
| Belltown_Room_Spare.3 | furnishings/Gramaphone | FSM [Behaviour] 6a791e996a; FSM [FSM] d5688cf85e (Hit Response) | Gramaphone jitter, hit distortion | no | Audio and animation. | | | scenes/Belltown_Room_Spare.fsm.txt:1178, scenes/Belltown_Room_Spare.fsm.txt:1295 |
| Belltown_Room_Spare.4 | Spa Region | FSM [Heal HP] 55cfb4b450; FSM [Spa Region] 3bf29faa4e | Healing bath: slow heal, silk, removes maggot | no | Heals only the local hero standing in the tub, and each game handles its own hero. | | | scenes/Belltown_Room_Spare.fsm.txt:1438, scenes/Belltown_Room_Spare.fsm.txt:1479 |
| Belltown_Room_Spare.5 | Spa Glows, Spa Glows Front ×10 | FSM [Destroy on Level Load] d964b8e7d4 | Cleanup of bath glow effect | no | Visual cleanup. | | | scenes/Belltown_Room_Spare.fsm.txt:1572 |
| Belltown_Room_Spare.6 | left1 | FSM [Award Achievement] 89ce48c086 | Awards the house achievement when full | no | Achievement, per save. | | | scenes/Belltown_Room_Spare.fsm.txt:1857 |
| Belltown_Room_Spare.7 | UI Msg Get Item(Clone) ×5 | FSM [Msg Control] 31a5bc072a, a3e001590d | Item-get popup | no | UI. | | | scenes/Belltown_Room_Spare.fsm.txt:2298, scenes/Belltown_Room_Spare.fsm.txt:2700 |
| Belltown_Room_Spare.8 | Collection desks and board | C# CollectionViewerDesk; C# CollectionViewBoard; C# CollectionViewBoardItem; C# ScrollView; C# TextMeshProClipRect | Inspect trophies and mementos | no | Local viewer, only unlock bools in the player's save. | | | scenes/Belltown_Room_Spare.objects.txt:27, scenes/Belltown_Room_Spare.objects.txt:391, scenes/Belltown_Room_Spare.objects.txt:408, scenes/Belltown_Room_Spare.objects.txt:416, scenes/Belltown_Room_Spare.objects.txt:417 |
| Belltown_Room_Spare.9 | Hunter Heart, Clover Heart, Coral Heart ×4 | C# InventoryItemHeart; C# InventoryAudioFadeOut | Heart trophy sprites with heartbeat audio | no | Display and audio. | | | scenes/Belltown_Room_Spare.objects.txt:166, scenes/Belltown_Room_Spare.objects.txt:167 |
| Belltown_Room_Spare.10 | Gramaphone audio helpers | C# VolumeBlendController; C# FadeAudioOnPause; C# NeedolinSyncedAudioPlayer | Music loop blend, needolin sync clip | no | Audio. | | | scenes/Belltown_Room_Spare.objects.txt:181, scenes/Belltown_Room_Spare.objects.txt:200 |

## Belltown_Shrine

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Belltown_Shrine.1 | Boss Scene | FSM [Control] c05ca6d5ba | Runs the Spinner fight and aftermath | sure | Checks spinnerDefeated and encounteredSpinner, starts the fight, and at the end sets spinnerDefeated (changes Belltown for the whole save), ends the main quest, opens gates and awards the achievement. | Fight start (SPOKEN or challenge), the end (SPINNER SEQUENCE END) and the spinnerDefeated flag. | Pull the partner into the arena, lock them out, or let them spectate. | scenes/Belltown_Shrine.fsm.txt:4 |
| Belltown_Shrine.2 | Audio Loop Tension | FSM [FSM] 956639e0c4 | Tension music | no | Audio. | | | scenes/Belltown_Shrine.fsm.txt:106 |
| Belltown_Shrine.3 | Bench Appear Sequence | FSM [Bench Appear] cd6143047d | Post-fight bell bench appears, quest update | sure | Runs only when the hero enters the alert range after the defeat, and it emits SPINNER SEQUENCE END plus the Grand Gate Bellshrines quest update for that hero. | The bench appearing (BELLBENCH APPEARED), once. | Quest update to both or only the finisher. | scenes/Belltown_Shrine.fsm.txt:943 |
| Belltown_Shrine.4 | Hanging Bell | FSM [Break Threads] 74b22f0c47 | Hitting the bell snaps its threads in steps | sure | Thread break steps are saved in a state sequence and only advance in the game where the bell was hit. | The bell hit (THREAD BELL HIT) or the step index. | | scenes/Belltown_Shrine.fsm.txt:1014 |
| Belltown_Shrine.5 | Challenge Region | FSM [Challenge] b604b67718 | Hero's challenge skill starts the fight without talking | sure | Reads and writes the local hero's Silk Specials state and starts the fight from that action (CHALLENGE START). | The fight start, same as Belltown_Shrine.1. | | scenes/Belltown_Shrine.fsm.txt:1247 |
| Belltown_Shrine.6 | Battle Gate (2) | FSM [FSM] 1a374f9261 | Quick-closes gate when defeated but shrine not used | maybe | Evaluated at scene load from the local flags spinnerDefeated and bellShrineBellhart. | | | scenes/Belltown_Shrine.fsm.txt:1420 |
| Belltown_Shrine.7 | Ground Bells Holder 1..4 | FSM [FSM] c8cc1d0bde | Ground bell hazards during fight | no | Hazard visual, follows Belltown_Shrine.9. | | | scenes/Belltown_Shrine.fsm.txt:1489 |
| Belltown_Shrine.8 | HarpShots, Spinner BellShot 1, 2 | FSM [Control] b1a61418be; FSM [Control] 30134edbde | Bell projectile volleys aimed at hero's x | sure | Pattern is a weighted random and aim shifts are rolled per game, so volleys land on different spots (follows Belltown_Shrine.9). | The pattern choice and fire events from the host. | | scenes/Belltown_Shrine.fsm.txt:1557, scenes/Belltown_Shrine.fsm.txt:1581 |
| Belltown_Shrine.9 | Speak Region | FSM [Dialogue] 43a3348e1b | Intro talk with the Spinner, ends in fight | sure | Interact by one hero locks that hero in dialogue and sends SPOKEN, which starts the fight. | The interact and fight start. | Partner frozen during the intro talk, pulled to the arena, or left free. | scenes/Belltown_Shrine.fsm.txt:1741 |
| Belltown_Shrine.10 | Spinner Boss | FSM [Control] d1cf758950 | Spinner boss, HP 360, teleports, slams, strums, rages | sure | Teleport target, attack choice and evade all read the local hero, attacks are weighted random, and the intro roar stuns the local hero. | Fight start, the host's attack and teleport choices, hits from either player, and the phase and death steps. | Which hero the boss targets: nearest, first aggro, or each game its own copy. Boss HP shared. | scenes/Belltown_Shrine.fsm.txt:1907 |
| Belltown_Shrine.11 | Spinner Boss | FSM [Fake Death] a390ca2480 | At the P3 HP threshold sends FAKE DEATH | sure | Reads HP from the local copy, so the mid-fight death scene fires at different times if HP differs (follows Belltown_Shrine.10). | | | scenes/Belltown_Shrine.fsm.txt:3111 |
| Belltown_Shrine.12 | glint audio player ×2 | C# AudioSourcePitchRandomizerV3 | Random pitch for attack glint sounds | no | Audio. | | | scenes/Belltown_Shrine.objects.txt:1583 |

## Peak_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_01.1 | Peaks Drifter ×27 | FSM [Control] e696b372a8 | Flying critter, HP 4, respawns after death | maybe | Passive drift with contact damage that comes back after a kill, so the worst case is a drifter alive in one game and dead in the other. | Nothing needed. | | scenes/Peak_01.fsm.txt:2369 |
| Peak_01.2 | Atmos Region ×3 | C# AtmosRegion | Sets area ambience | no | Audio and environment. | | | scenes/Peak_01.objects.txt:151 |
| Peak_01.3 | Hero Box Breaker ×29 | C# BreakableTriggerBreak | Breaks white ball props when hero enters | no | Decorative break from the local hero's collider. | | | scenes/Peak_01.objects.txt:2815 |
| Peak_01.4 | Camera Wind Region ×17 | C# WindCameraRegion; C# WindRegion | Windy area: camera sway, hero in-wind flag | no | Affects only the local hero and camera. | | | scenes/Peak_01.objects.txt:2882 |
| Peak_01.5 | Umbrella Wind Region ×24 | C# UmbrellaWindRegion | Wind that pushes a floating brolly | no | Applies to the local hero only. | | | scenes/Peak_01.objects.txt:4048 |
| Peak_01.6 | peak_blizzard_below_hero | C# HeroFallParticle | Particles when hero falls fast | no | Visual. | | | scenes/Peak_01.objects.txt:4642 |

## Peak_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_02.1 | Crystal Drifter ×25 (normal and Black Thread variants) | FSM [Control] d81884536b | Bursts into 8 spikes near hero | sure | The burst fires when the local hero enters its attack range (or it is hit), so spikes hurt only in that game and the partner's drifter stays intact. | The burst trigger by object id; spikes and reform follow. | | scenes/Peak_02.fsm.txt:4 |
| Peak_02.2 | Float Crystal (12), (13) ×14 | FSM [Destroy] 3d7cf93015; FSM [Control] b16a651136 | Crystal platform bobs, shatters when hit, reforms | maybe | A hit in one game removes the platform there only, so a partner standing on it elsewhere keeps or loses footing differently. | The hit by object id. | | scenes/Peak_02.fsm.txt:3072, scenes/Peak_02.fsm.txt:3116 |
| Peak_02.3 | Snowflake Spawner, Drifting Snowflake ×10 | FSM [Control] 202e051db7, 621a599bb2; FSM [Set Terrain Collider] 59dec41fe6 | Falling snowflakes, random drift | no | Decoration. | | | scenes/Peak_02.fsm.txt:5279, scenes/Peak_02.fsm.txt:5301, scenes/Peak_02.fsm.txt:5386 |
| Peak_02.4 | Weaver Servitor Large | FSM [FSM] 28d0c2d36b; FSM [Control] 4bdbbdefc6; FSM [Control] c82ae18d3a; FSM [Control] 2fd240d131 | Walking sniper, beam and roll, HP 70 | sure | Wake, walk distance, beam aim and roll direction all read the local hero, and its kill is saved, so two copies fight two heroes and one stays alive. | Wake, the beam shot with its aim, and death. | Which hero a shared servitor targets: nearest, first aggro, or each game its own copy. | scenes/Peak_02.fsm.txt:6410, scenes/Peak_02.fsm.txt:6420, scenes/Peak_02.fsm.txt:6988, scenes/Peak_02.fsm.txt:7040 |
| Peak_02.5 | Corpse Weaver Servitor Large chunks ×3 | FSM [Control] a079c72480 | Corpse chunk physics | no | Corpse visuals. | | | scenes/Peak_02.fsm.txt:6772 |
| Peak_02.6 | RestBench/Light | C# iTweenFSMEvents | Bench light tween events | no | Visual. | | | scenes/Peak_02.objects.txt:10 |
| Peak_02.7 | Tinker (coal lantern string) ×6 | C# TinkRigidbodyForce | Tinked string cap swings | no | Visual physics reaction to a tink. | | | scenes/Peak_02.objects.txt:3992 |

## Peak_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_04.1 | drop pillar_chunk (1), (2) ×3 | FSM [Control] 3d8114f015; FSM [bobbing_platform] 574e6f2337 (floating chunk) | Pillar falls, becomes bobbing platform | sure | Persistent: the fall (touch or damage) happens only where the local hero hit it, with a damaging fall, and the platform appears only there. | The touch or hit trigger by object id; the fall and the platform then follow. | | scenes/Peak_04.fsm.txt:739, scenes/Peak_04.fsm.txt:800 |

## Peak_04c

no new elements

## Peak_04d

no new elements

## Peak_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_05.1 | Slide Surface ×14 | C# SlideSurface | Hero grabs and slides down a slope | no | Moves only the local hero, no saved state. | | | scenes/Peak_05.objects.txt:1130 |

## Peak_05c

no new elements

## Peak_05d

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_05d.1 | Audio Loop Maskmaker Voice | FSM [FSM] 54eb04ae0d | Mask maker voice loop | no | Audio. | | | scenes/Peak_05d.fsm.txt:4 |

## Peak_05e

no new elements

## Peak_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_06.1 | Crystal Drifter Giant ×11 | FSM [Control] 806d1958a8 | Giant flyer bursts into 15 to 20 spikes | sure | Same as Peak_02.1 with a larger burst: it fires when the local hero is in attack range and a 1 s cooldown is up, and the partner's copy does nothing. | The burst trigger by object id. | | scenes/Peak_06.fsm.txt:214 |

## Peak_06b

no new elements

## Peak_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_07.1 | Pinstress Control | FSM [Control] a5eb4958b5 | Picks Pinstress scene or empty scene | maybe | The choice reads the local quest state, blackThreadWorld and visitedIceCore, so one player may see her and the other not. | | The partner meets her only if their own quest allows it, or the scene follows whoever is on the quest. | scenes/Peak_07.fsm.txt:1631 |
| Peak_07.2 | Brolly Blocker, Brolly Blocker (1) | FSM [FSM] 238ed8e6d9 | Pushes Pinstress away when she enters it | no | Boss helper, follows Peak_07.3. | | | scenes/Peak_07.fsm.txt:1811 |
| Peak_07.3 | Pinstress Boss | FSM [Control] 7f1dd53119 | Sparring boss: darts, dashes, cross slash, teleports | sure | Targets, facing, teleport side, attack choice and the fall-back reset all read the local hero, attacks are random, and "Hornet dead" ends the fight. | Fight start, the host's attack and teleport choices, hits from either player, and the end. | Which hero she targets. Whether the partner joins the spar, is locked out, or is pulled in. Defeat reward for both or the finisher. | scenes/Peak_07.fsm.txt:1922 |
| Peak_07.4 | Pinstress Boss | FSM [Battle Range Check] a6bb803c79 | Hero out of range resets fight | sure | Measures only the local hero, so a partner who leaves, or dies, ends or keeps the fight differently in each game; on range-out the boss heals fully. | The range-out result, as one fight end. | Range counts the nearest player, any player, or all players. | scenes/Peak_07.fsm.txt:3133 |
| Peak_07.5 | Pinstress Boss/Charge Effect | FSM [Effect Control] ae8198f225 | Charge flash and shake | no | Visual. | | | scenes/Peak_07.fsm.txt:3162 |
| Peak_07.6 | Pinstress Boss/NPC | FSM [NPC Control] adcdcea7d1 | Sleeping Pinstress: wake, talk, offer spar | sure | Waking uses hits on a sleep HP, and the spar is accepted by a yes or no prompt for one hero, setting PinstressPeakBattleAccepted and starting the fight. | The accept (fight start) and the wake. | Who may accept for the pair. Partner pulled into the dialogue and prompt, frozen, or left free. | scenes/Peak_07.fsm.txt:3185 |
| Peak_07.7 | Conveyor Speed Zone, Conveyor Zone ×17 | C# ConveyorSpeedZone; C# ConveyorZone | Windy edge conveyors push hero | no | Moves only the local hero. | | | scenes/Peak_07.objects.txt:1463, scenes/Peak_07.objects.txt:1470 |
| Peak_07.8 | Double Jump Peak Glower ×2 | C# ActivateOnDoubleJump | Glow turns on after a double jump | no | Visual, reacts to the local hero's jump. | | | scenes/Peak_07.objects.txt:2847 |

## Peak_08

no new elements

## Peak_08b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_08b.1 | DJ Get Sequence | FSM [DJ Get Sequence] f1745a97eb | Needolin at tuning fork grants Double Jump | sure | Playing the needolin in range locks the hero, plays a cutscene, sets hasDoubleJump, saves and sets the death respawn marker, for that hero only. | The ability grant (hasDoubleJump), if abilities are shared. | Double Jump to both players or the player who did it. Partner pulled in, frozen, or free. | scenes/Peak_08b.fsm.txt:4 |
| Peak_08b.2 | Audio Loop TuningFork | FSM [Fade Audio In] e5c22642a3 | Tuning fork audio | no | Audio. | | | scenes/Peak_08b.fsm.txt:246 |
| Peak_08b.3 | Fayforn Ground Sit NPC | FSM [Sing Reaction] 11b4750db5 | NPC sings back to needolin | no | Cosmetic and tied to the local needolin. | | | scenes/Peak_08b.fsm.txt:254 |
| Peak_08b.4 | Tuning Fork, Needolin Audio, Needolin Harp Loop | C# VibrationPlayerAdvanced; C# SyncNeedolinLoop | Controller vibration and audio sync | no | Audio and rumble. | | | scenes/Peak_08b.objects.txt:688, scenes/Peak_08b.objects.txt:716 |

## Peak_10

no new elements

## Peak_12

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_12.1 | Webshot Scene | FSM [Repair webshot] 8925bb91b5 | Repairs Broken SilkShot into WebShot tool | maybe | Consumes the item and grants the tool to the repairing hero only; the Broken SilkShot is checked in the local inventory. | The repair result, if tools are shared. | Tool granted to both players or only the repairer. | scenes/Peak_12.fsm.txt:828 |
| Peak_12.2 | Glow Response Object gg ×4 | FSM [Glow Noise] 6f42a0485f | Glow pulses | no | Visual. | | | scenes/Peak_12.fsm.txt:891 |

## Peak_Mask_Maker

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Peak_Mask_Maker.1 | Peak Mask Maker | FSM [Dialogue] e109de6d9a; C# EnemyHitEffectsBasic | Talks, delivery quest, heavy hit unmasks | maybe | A heavy hit throws the mask off and the naked state is saved in the NPC's persistent bool, so the NPC looks different per game; dialogue and delivery reward go to the talker. | The heavy hit (BIG BONK) by object id. | Delivery quest and reward shared or per player. | scenes/Peak_Mask_Maker.fsm.txt:21, scenes/Peak_Mask_Maker.objects.txt:100 |
| Peak_Mask_Maker.2 | Bonk Audio | FSM [Control] 8d8f18df3e | Bonk sound by mask state | no | Audio. | | | scenes/Peak_Mask_Maker.fsm.txt:268 |

## Summary

Events that should pull the partner to the player who triggered them, or at least be an explicit decision:

1. Belltown_08.4 Wisp Pyre Effigy: arena gates close for one hero only and the opening roar stuns that hero. Decide whether the partner is pulled in and stunned, locked in too, or left free.
2. Belltown_Shrine.9 and Belltown_Shrine.10 Spinner (Widow): intro talk locks one hero, the fight reads that hero for teleports and attacks, and the end sets spinnerDefeated, which flips Belltown from cursed to saved for the whole save. Treat the fight start and its end as shared, and decide how the partner arrives.
3. Peak_07.6, Peak_07.3, Peak_07.4 Pinstress spar: accepted by a prompt for one hero, and the fight resets if that hero leaves the range. Needs a rule for who may accept and which hero the range check uses.
4. Belltown_cutscene.1 arrival video: pull in, freeze or leave free, and who may skip.
5. Belltown_Room_doctor.2 forced talk, curse cure cinematic and crest unlock: the trigger captures the local hero; decide freeze or leave free and whether the cure is shared.
6. Peak_08b.1 Double Jump grant, Belltown_Room_pinsmith.1 nail upgrade, Belltown_Room_Spare.1 Farsight: each locks one hero in a cutscene and grants a permanent ability. Decide shared or per player, and whether the partner is held.

Cross-cutting choices:

7. Quests, shops and rewards (Belltown.1, 4, 6, 10, 12; Belltown_Room_Relic.2; Peak_12.1; Peak_Mask_Maker.1): everything is written to the interacting hero's save. One rule for the whole game, shared quest log and stock or per player, would settle all of them.
8. World objects that change for good and are triggered by a hero's presence or hit are the clearest pre-codable set, because without a shared trigger the second player meets the old state: Drop Bell (Belltown_04.4), Quest Bell Dropper (Belltown_04.5), flea cage (Belltown_04.3), Mines Lift unlock (Belltown_06.1), Collapser floors (Belltown_basement_03.1), One Way Wall (Belltown_basement_03.3), Hanging Bell (Belltown_Shrine.4), pillar chunks (Peak_04.1). Share the trigger by object id and let each game replay the rest.
9. Hero-proximity enemies in Peak (Crystal Drifter and Giant: Peak_02.1, Peak_06.1; Weaver Servitor Large: Peak_02.4): they attack only when the local hero is near, so a partner never triggers the other game's copy. Share the burst, wake and shot triggers, and decide the targeting rule.
10. Gaps in the catalog I could not fully pin down: Doctor Fly's forced-talk trigger and Webbed Hero's exact start condition (first scene a player loads) were read from the FSM only, not from a running scene. Bell Fly and Peaks Drifter were rated maybe and could move to no if dead flyers need not match.
