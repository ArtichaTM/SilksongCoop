# Audit 2.13: Song (City_Lace_cutscene, Song_01 .. Song_Tower_Destroyed)

Slice: catalog/Song.md (Choral Chambers / Citadel). Verdict means: does coop need to handle it. Enemy rows are `sure` when alert, chase, target or attack choice reads only the local hero or uses a random roll; follower FSMs (audio, particles, jitter) are `no`. Where a row says "one game decides", the other game should replay enemy state, HP and death. Scripted scenes (lock the hero, set PlayerData, start a quest) are `sure` and carry a design choice. Shared notes live in `audit/notes/` (new: BattleSceneEnemy, JoinedLevers, SilkRationObject, SilkRationMachine, TrapBridgeFlip, DialDoorBridge, MetronomePlat, BouncePodSequence, NPCEncounterStateController, QuestRewardHolder, BlackThreadImpacter, SpecialQuestItemVariant, CogMultiHitter, HeroBouncer).

## City_Lace_cutscene

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| City_Lace_cutscene.1 | Cinematic Player | FSM Door Enter Play c82f41dc15 | Door plays cinematic, sets visitedCitadel, unlocks menu style | sure | Only the hero who touched the door sees the cinematic and fades; the partner stays in the old scene with no idea. | The DOOR TOUCHED start and the CINEMATIC END. | Pull the partner into the cinematic scene, hold them until it ends, or let them skip. | scenes/City_Lace_cutscene.fsm.txt:1 |

## Song_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_01.1 | Song Reed (Enemy Group) ×N | FSM Control f0ee983e8f | Flying reed: patrols, alerts, chases, multi-stabs | sure | Alert, chase, evade and stab read only the local hero; sings along to local needolin. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_01.fsm.txt:301 |
| Song_01.2 | Song Pilgrim 03 | FSM Attack 742cf9eb27 | Ground pilgrim: alerts, hops, slashes, whips, jump-spins | sure | Alert, attack choice and hop direction target the local hero and use random picks; sends HANDMAIDEN CALL on alert. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_01.fsm.txt:1012 |
| Song_01.3 | Pilgrim 01 Song ×3, Pilgrim 02 Song ×2, Song Pilgrim 01 ×2 (Citadel Woken Scene) | FSM Attack 30a925acf2, 0a99a42f69, 21240b0f1c | March and ambush pilgrims: roll, charge, slash combos | sure | Wake, target hero and combo choice are local and random; zero-HP death is local. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_01.fsm.txt:2969, scenes/Song_01.fsm.txt:3634, scenes/Song_01.fsm.txt:4257 |
| Song_01.4 | Song Pilgrim 01 (2), (3) | FSM FSM 73a375417a | Rolls chance to call a handmaiden when alerted | sure | Each game rolls its own percent and checks its own marionettes flags, so one game gets a handmaiden and the other does not. | The CALL decision (HANDMAIDEN CALLED) from the game that rolled. | | scenes/Song_01.fsm.txt:4226 |
| Song_01.5 | Pilgrim 04 Song (1) | FSM Control 79df51c64f | Flying pilgrim: patrols, startles, dash-charges at hero | sure | Alert range, charge direction and chase target read the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_01.fsm.txt:7416 |
| Song_01.6 | Breakable Floor_basic Song_01 | FSM Control 6f0253c752 | Floor breaks after nail hits; sets PD bool | sure | The hits and the break happen only in the game where the nail landed; the other game keeps a solid floor. | The break (final hit) of this floor. | | scenes/Song_01.fsm.txt:2240 |
| Song_01.7 | Jitter Bits | FSM FSM 3ca7f48025 | Floor jitters while hero walks on it | no | Cosmetic reaction to the local hero velocity. | | | scenes/Song_01.fsm.txt:2319 |
| Song_01.8 | Song Pilgrim 01 (3), Song Pilgrim 03 | C# BattleSceneEnemy | Enables enemy FSMs when the battle starts | sure | The enemy stays frozen until the local battle scene starts, so it wakes only in the game whose hero triggered the arena. | The battle-start event of the arena. | When a battle starts, pull the partner into the arena or leave them outside. | scenes/Song_01.objects.txt:84 |
| Song_01.9 | Lever Cog ×2 (Pipe Vent Hatch) | C# JoinedLevers | Hit on either lever opens the vent hatch | sure | The lever pair and hatch state (persistent bool) change only in the hitting game. | The lever hit; the hatch then opens in both games. | | scenes/Song_01.objects.txt:3674 |

## Song_01b

No new elements.

## Song_01c

No new elements.

## Song_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_02.1 | Pilgrim 03 Song ×N | FSM Control 72bd8dc1e1 | Thrower pilgrim: alerts, throws, evades | sure | Alert, throw direction and evade read the local hero and use random picks. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_02.fsm.txt:541 |
| Song_02.2 | Pilgrim Stomper Song ×N | FSM Attack 0a180855fe | Chases hero, leaps and stomps | sure | Chase and stomp timing read the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_02.fsm.txt:900 |
| Song_02.3 | Heavy Landing Msg | FSM FSM 993f742f2a | Trigger entry sends HEAVY LANDING event | no | Helper that fires with the stomper landing. | | | scenes/Song_02.fsm.txt:1093 |
| Song_02.4 | Clear Out Group | FSM FSM a50a97bfb1 | Marks Song Knight absent-variant enemies dead in save | maybe | Whether the knight is present comes from each player's own PD flags, so two players can see different enemy sets. | The songChevalierActive flag of this scene so both games pick the same variant. | | scenes/Song_02.fsm.txt:2293 |
| Song_02.5 | Song Knight BattleEncounter | FSM Control 7f164cac8d | Song Knight wakes, bows, duels or allies, parries, dives | sure | Boss target is the local hero; parry rolls, move choice and stun are random and local; sets encountered PD flags. | One game runs the knight; share its state, HP, stun and death plus the start of the duel. | Duel only the player who talked to it, the nearest player, or both players. | scenes/Song_02.fsm.txt:3160 |
| Song_02.6 | Target Cursor | FSM Control 9f424334d2 | Picks which enemy the allied knight aims at | maybe | Only used in ally mode; follows the knight decision. | Covered by the knight row Song_02.5. | | scenes/Song_02.fsm.txt:4246 |
| Song_02.7 | Mite Heavy, Mite Heavy (1) | FSM Control 5323603073 | Heavy mite: runs, leaps, multislashes, hides | sure | Attack or evade choice, leap and multislash read the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_02.fsm.txt:8411 |
| Song_02.8 | Attack Damager ×2 (Mite Heavy) | FSM multi wounder cde230c3f2 | Damage collider; reacts to parry, tink, bell bind | maybe | Parry and bell bind stagger the mite only in the game where they happened. | Follows the mite state sync of Song_02.7. | | scenes/Song_02.fsm.txt:8583 |
| Song_02.9 | RapidSlash Collider | C# ComponentToggleTimer | Pulses the knight slash collider on and off | no | Timer-driven visual of the knight attack; follows its state. | | | scenes/Song_02.objects.txt:2872 |

## Song_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_03.1 | Grindle Chest Thief | FSM Control 5f3d5002d8 | Thief rummages chest, flees when hero is near | sure | Approach and noise checks read the local hero, so the thief flees only in one game and the chest stays guarded in the other. | The approach trigger (thief startled, leaves, chest QUICK OPEN). | | scenes/Song_03.fsm.txt:80 |
| Song_03.2 | Silk Ration Dropper | FSM Control 5e2e73990f | Rolls full or empty silk ration on corpse | maybe | Each game rolls its own percent, so the corpse can drop a full ration in one game and an empty one in the other. | The drop result of one game, or only the game that killed the enemy rolls. | | scenes/Song_03.fsm.txt:1569 |
| Song_03.3 | Chunk ×12 | C# SpecialQuestItemVariant | Corpse chunk shows quest item if quest active | maybe | Depends on the local quest and a roll, so only one player may see the item. | | | scenes/Song_03.objects.txt:1466 |
| Song_03.4 | Silk Ration Citadel Large | C# SilkRationObject | Pickup gives silk to the hero who touches it | maybe | The pickup vanishes only in the game where it was touched, so both players can take it. | The pickup (which hero took it) so it disappears in both games. | Silk to the toucher only, or to both players. | scenes/Song_03.objects.txt:1502 |
| Song_03.5 | Silk Ration Citadel Large Core | C# DropRecycle | Recycles dropped core after it falls | no | Pure object-pool timing. | | | scenes/Song_03.objects.txt:1511 |

## Song_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_04.1 | Song Pilgrim 03 (Song Knight Present) | FSM Attack a890c31162 | Ground pilgrim: alerts, slashes, whips, jumps | sure | Targets and attack picks are local and random. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_04.fsm.txt:2006 |
| Song_04.2 | Song Reed (Song Knight Present) | FSM Control a34e22be3a | Flying reed: patrols, alerts, chases, stabs | sure | Alert, chase and stab read only the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_04.fsm.txt:3238 |
| Song_04.3 | Wake Scene | FSM Control e3aab965d2 | Trigger roars hero, pulls threads, starts battle | sure | Local hero entering the trigger gets the roar lock and starts the battle wave and cam locks; the partner is not locked and sees no wake. | The wake trigger (battle start) so both games start the waves. | Pull the partner into the arena, lock them too, or leave them free. | scenes/Song_04.fsm.txt:5055 |
| Song_04.4 | silk_strand_impact_parent ×45 | FSM Control 3538a68ef3 | Random-delay silk strand impact visuals | no | Visuals driven by the Wake Scene event. | | | scenes/Song_04.fsm.txt:5101 |
| Song_04.5 | Song Handmaiden (Wave 2, Wave 5) | FSM Silk Suck 67677f94a1 | Drains hero silk while in suck range | sure | TakeSilk and range checks hit only the local hero's silk; the partner in range loses nothing. | The silk tick (or suck target) for each player in range. | Drain only the targeted player, or every player in range. | scenes/Song_04.fsm.txt:8010 |
| Song_04.6 | Song Handmaiden (Wave 2, Wave 5) | FSM Control 9e5c5ebf0f | Flies, slashes, spin-sucks silk, teleports | sure | Target selection, teleport and slash read the local hero; handmaiden calls come from random rolls. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_04.fsm.txt:8097 |
| Song_04.7 | Song Handmaiden | FSM Lock Control f3b22879a6 | Short lock so two handmaidens do not suck together | no | Local timer, follows the suck state. | | | scenes/Song_04.fsm.txt:8592 |
| Song_04.8 | Rest Pivot | FSM Control d8726c025b | Random sway of the resting handmaiden | no | Random idle rotation. | | | scenes/Song_04.fsm.txt:8606 |
| Song_04.9 | Spear | FSM Control 968d355a97 | Handmaiden spear homes on hero then explodes | sure | Aim and homing target the local hero; explosion damages local hero. | Follows the handmaiden target decision. | | scenes/Song_04.fsm.txt:8643 |
| Song_04.10 | CamLock Control | FSM Control 4744919c4e | Toggles camera lock while hero is in range | no | Camera is local anyway. | | | scenes/Song_04.fsm.txt:12631 |
| Song_04.11 | innerworks_trap_bridge | C# TrapBridgeFlip | Trap bridge flips open or closed | sure | Flip direction depends on which side the local hero stands on, and it opens only in the game where it was triggered. | The bridge Open and Close events. | | scenes/Song_04.objects.txt:1741 |

## Song_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_05.1 | Lace NPC Citadel Meet | FSM Control d947b599d3 | Lace meeting cutscene; hero lifted; starts quest | sure | Locks and walks only the local hero, plays dialogue, sets laceMeetCitadel, citadelWoken, visitedCitadel and begins quest Citadel Ascent. | The start of the cutscene and its end flags (citadelWoken, laceMeetCitadel, quest start). | Pull the partner in and freeze them, freeze them where they stand, or leave them free. | scenes/Song_05.fsm.txt:2674 |

## Song_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_07.1 | Wave 1 | FSM Start Battle via Enemy Alert 58294aa74e | Starts battle when any enemy is alerted | sure | Alert comes from the local hero, so the battle starts only in one game. | The ENEMY ALERTED event. | Pull the partner into the arena or leave them outside. | scenes/Song_07.fsm.txt:271 |
| Song_07.2 | City Merchant Meet | FSM Meet Control fe0808298d | Merchant hides in battle, rescue dialogue ends quest | sure | Rescue dialogue, EndQuest Save City Merchant and the keep-active flag happen only for the talking hero. | Battle start and end, and the saved state of the merchant. | Quest reward to the talker only, or both. | scenes/Song_07.fsm.txt:1841 |
| Song_07.3 | Song Knight Resting NPC | FSM Control 8ea439832f | Resting knight: seen flags, wake, dialogue, leaves | sure | Writes songChevalierSeenIn, EncounterCooldown, Encounters counter and Met flags locally, so players diverge on knight appearances. | The seen and met flags plus the leave. | | scenes/Song_07.fsm.txt:2105 |

## Song_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_08.1 | Barrel | FSM Shake 60d3d4c251 | Barrel shakes on SHAKE event | no | Cosmetic. | | | scenes/Song_08.fsm.txt:4 |
| Song_08.2 | Grindle Citadel Barrel Meet | FSM Meet Control 0f83c1fdcc | Hidden Grindle emerges after merchant is saved | sure | Sets savedGrindleInCitadel and grindleInSong_08 for the local game only, and the emerge plays only there. | The saved event (follows the merchant rescue of Song_07.2). | | scenes/Song_08.fsm.txt:10 |
| Song_08.3 | Thief NPC Rummage | FSM Conversation 498476029d | Grindle rummages, talks, enclave convo flags | maybe | Dialogue and PD flags are per player; only the flags matter. | | | scenes/Song_08.fsm.txt:890 |

## Song_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_09.1 | Citadel Bat ×N | FSM Control 837b201d27 | Bat roosts, wakes, flies, charges at hero | sure | Wake, chase and charge read only the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_09.fsm.txt:11 |
| Song_09.2 | Citadel Bat ×N | FSM Flap ec5ff8f6d9, Flapping Voice 05fa8735ce | Flap animation and voice loop | no | Cosmetic followers. | | | scenes/Song_09.fsm.txt:300, scenes/Song_09.fsm.txt:334 |
| Song_09.3 | Waker ×3 | FSM Wake 53b8eea839 | Trigger wakes the bat roost | maybe | Trigger fires from the local hero only; the other game's bat stays asleep unless enemy state is synced. | Covered by the bat state sync of Song_09.1. | | scenes/Song_09.fsm.txt:345 |

## Song_09b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_09b.1 | Great Gourmand Scene | FSM Curtain Jiggle 027e57a22e | Random curtain jiggle | no | Ambience. | | | scenes/Song_09b.fsm.txt:618 |
| Song_09b.2 | Gourmand Servant | FSM Dialogue 81aa8afb74 | Quest giver: offers, takes dishes, Gourmand scene | sure | Quest offer, dish hand-in and reward scene run for the talking hero; locks inventory, save-quit and pause in that game only. | Quest accept, complete and the Gourmand scene start. | Run the scene for the talker only, or pull the partner in. | scenes/Song_09b.fsm.txt:628 |
| Song_09b.3 | pivot (pale_oil_holder) | FSM Hit Animate 05e9aca2fa | Tink wobble of the oil holder | no | Cosmetic. | | | scenes/Song_09b.fsm.txt:985 |
| Song_09b.4 | pale_oil_holder, Generic Pickup | C# QuestRewardHolder, GenericPickup | Reward pickup appears after quest; sets PD bool | sure | Pickup gives the reward to the interacting hero only; the other game still shows it. | The pickup (taken flag). | Reward to the taker only, or to both. | scenes/Song_09b.objects.txt:157, scenes/Song_09b.objects.txt:166 |

## Song_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_10.1 | Pipes BG Water Audio | FSM Control e6bc7d6cbd | Background water audio | no | Ambience. | | | scenes/Song_10.fsm.txt:784 |
| Song_10.2 | Chain Platform Breaker, Vine | FSM Rope Plat a31dfa796e, Vine Cut 28b56f0485 | Nail cut of vine drops chain platform, breaks | sure | The cut and the fall happen only in the cutting game (persistent bool). | The vine cut (NAIL HIT leads to CUT). | | scenes/Song_10.fsm.txt:798, scenes/Song_10.fsm.txt:960 |
| Song_10.3 | Hero Land, Landing Box | FSM Land 06792e24df, Land Detector e05b3d5e7c | Hero landing audio, hard or acid land events | no | Local landing feedback. | | | scenes/Song_10.fsm.txt:875, scenes/Song_10.fsm.txt:899 |
| Song_10.4 | Collapser Small | FSM Control c21fcf74e6 | Collapses on BREAK, rock debris | maybe | Breaks with the chain platform event; persistent bool differs if only one game broke it. | Follows Song_10.2. | | scenes/Song_10.fsm.txt:1004 |
| Song_10.5 | Sherma Citadel Spa NPC | FSM Conversation Control 8b1adbbee9 | Sherma spa dialogue; sets metSherma flags | maybe | Dialogue and flags are per player. | | | scenes/Song_10.fsm.txt:1850 |
| Song_10.6 | Sherma Citadel Spa NPC | FSM Sprite Control ac54c4c43d, Splash Audio 8c10cccd5a | Sherma sprite and splash audio | no | Cosmetic. | | | scenes/Song_10.fsm.txt:1893, scenes/Song_10.fsm.txt:2031 |
| Song_10.7 | Audio Loop Piped Water ×2 | FSM Control 2db8bdb6a3 | Shower water audio loop | no | Ambience. | | | scenes/Song_10.fsm.txt:2040 |
| Song_10.8 | Shower Lever ×2 | FSM Control 109d2ba06c | Nail-hit lever toggles shower on and off | maybe | Persistent int stores the toggle, but it is decorative. | The lever hit. | | scenes/Song_10.fsm.txt:2052 |
| Song_10.9 | Hero Spatter ×2 | FSM Control b49d7ac35c | Water spatter on hero in shower; flame imbue check | no | Cosmetic effect on the local hero. | | | scenes/Song_10.fsm.txt:2133 |
| Song_10.10 | Steam Column ×6, Steam Column Timer | FSM Control b7e1793040, Control 7cfc7fa213 | Timed steam jets that damage hero | maybe | Timer starts when the local hero is in range, so each game's jets run at their own phase; damage is local. | Optional: the STEAM ON and OFF phase of the timer. | | scenes/Song_10.fsm.txt:2658, scenes/Song_10.fsm.txt:2988 |

## Song_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_11.1 | Plug_Gate | FSM Animation 1aea55f270 | Big plug gate breaks open on OPEN | maybe | Opens only in the game that received OPEN; persistent bool. | Follows the propeller activation of Song_11.2. | | scenes/Song_11.fsm.txt:4527 |
| Song_11.2 | propeller understore propeller ×3 | FSM Control 913e5669ff | Staged lever hits spin propeller; final starts updraft | sure | Stage counter and the final activation (persistent) advance only where the nail landed. | The LEVER HIT; each game replays staging and updraft. | | scenes/Song_11.fsm.txt:4551 |
| Song_11.3 | Control Lever ×3 | FSM Detect Hit e9d87421f2 | Nail hit sends LEVER HIT to propeller | sure | The only source of progress for Song_11.2. | The lever hit (same as Song_11.2). | | scenes/Song_11.fsm.txt:4694 |
| Song_11.4 | metronome_plat ×33 | C# MetronomePlat | Platform extends and retracts on a timer | maybe | Each game runs its own clock, so a platform the partner stands on can be gone in the other game. | The tick phase (start time) of the platform ticker. | | scenes/Song_11.objects.txt:2040 |

## Song_12

No new elements.

## Song_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_13.1 | Song Pilgrim 03 (1) | FSM call_handmaiden 106501932c | Rolls chance to call a handmaiden | sure | Same local roll and marionettes flag check as Song_01.4. | The CALL decision (HANDMAIDEN CALLED). | | scenes/Song_13.fsm.txt:1895 |

## Song_14

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_14.1 | Flea Rescue CitadelCage | FSM Control 932d7b7510 | Flea cage: nail hits topple it, break frees flea | sure | Hits and break (quest scene bool) happen only in the hitting game, so the flea can be rescued twice or only once. | The cage break. | Flea credit to the breaker only, or to both. | scenes/Song_14.fsm.txt:363 |

## Song_15

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_15.1 | Song Pilgrim Scene | FSM Control 2047bc9899 | Trigger wakes the sleeping pilgrim group | sure | Local trigger sets Activated and sends WAKE only in its game. | The WAKE event. | | scenes/Song_15.fsm.txt:301 |

## Song_17

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_17.1 | March Group Control | FSM Control fc539fb644 | Pilgrim march direction by hero side; sets clearedOut | sure | Direction is chosen from the local hero side, so two players can see opposite marches; song_17_clearedOut is written locally. | The chosen direction (MARCH L or R) and the clearedOut flag. | | scenes/Song_17.fsm.txt:3183 |
| Song_17.2 | black_thread_impacter ×3 | C# BlackThreadImpacter | Black thread strand visual on trigger entry | no | Visual with a persistent bool. | | | scenes/Song_17.objects.txt:2114 |

## Song_18

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_18.1 | steam_vent (1), (2) | FSM Control 4ac0278e4c | Timed steam vent: antic, blast, damage | maybe | Timer runs per game, so vent phase differs; damage is local. | Optional: vent phase. | | scenes/Song_18.fsm.txt:1677 |
| Song_18.2 | steam_vent (1), (2) | FSM Screen Shake f22fb3e7d7 | Camera shake while vent blasts | no | Local camera. | | | scenes/Song_18.fsm.txt:1732 |

## Song_19_entrance

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_19_entrance.1 | Scales Plat Arm ×4 | FSM Control c66a9ee9a2 | Balance arm tilts left or right, locks | sure | Tilt follows where the local hero lands; a partner standing on a pan does not tilt it in the other game. | The tilt of each arm (TILT L or R) and the lock. | | scenes/Song_19_entrance.fsm.txt:271 |
| Song_19_entrance.2 | Plat L, Plat R ×16 | FSM Detect Landing 48b96cf7c2 | Landing on pan tilts the scale by hero X | sure | Reads local hero position and landing collider. | Follows Song_19_entrance.1. | | scenes/Song_19_entrance.fsm.txt:370 |
| Song_19_entrance.3 | Song_lever_side | FSM Do Lock 3b26926a42 | Lever locks scale plats, roar and shake | sure | Persistent lever hit locks plats only in the hitting game and sends the roar event. | The lever hit; locking then plays in both games. | Roar effect on the hitter only, or on both players. | scenes/Song_19_entrance.fsm.txt:2537 |

## Song_20

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_20.1 | right_cutsceneEntry | FSM Cutscene Control bc0c5ce296 | Entry from Bellway: hero locked, look up, camera pan | maybe | Locks only the entering hero for a few seconds; sets act2Started for that game. | The act2Started flag. | Hold the partner while the entry plays, or leave them free. | scenes/Song_20.fsm.txt:9785 |

## Song_20b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_20b.1 | Dial Door Bridge | C# DialDoorBridge | Lever hit rotates the door bridge 90 degrees | sure | Rotation and doors change only in the hitting game; colliders differ if states disagree. | The lever hit (and direction). | | scenes/Song_20b.objects.txt:964 |
| Song_20b.2 | Hero Bouncer ×3 | C# HeroBouncer | Pushes the hero away from the door edge | no | Local movement push. | | | scenes/Song_20b.objects.txt:1074 |

## Song_24

No new elements.

## Song_25

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_25.1 | Weakness Scene Act3 Final | FSM Control bb71727d22 | Act 3 weakened-hornet scripted walk and fall | sure | Takes control of the local hero, makes them invincible and plays a long forced sequence; the partner is not part of it. | Start of the scene and its completion flag. | Pull the partner in and freeze or mirror them, or leave them free. | scenes/Song_25.fsm.txt:102 |
| Song_25.2 | Hatch ×2 | FSM Control 2e7ecd7ef9 | Grate breaks when the hero collides with it | sure | Breaks only in the colliding game. | The grate break. | | scenes/Song_25.fsm.txt:335 |
| Song_25.3 | Fall Grate | FSM Bounce Away 2d6c5b6dee | Grate debris bounces away | no | Cosmetic. | | | scenes/Song_25.fsm.txt:370 |
| Song_25.4 | Song Reed Grand ×5 | FSM Control 023ee06670 | Big reed: wakes, slashes, jump attacks, casts, blocks | sure | Wake, target and attack choice read the local hero and are random. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_25.fsm.txt:409 |
| Song_25.5 | Clear Out Group | FSM FSM 7db8388ac0 | Marks absent-variant Song Reed Grand dead in save | maybe | Same present or absent knight mismatch as Song_02.4. | The songChevalierActive flag. | | scenes/Song_25.fsm.txt:2411 |
| Song_25.6 | Song Reed Grand SK | FSM Control 2f73bc0056 | Knight-present variant of the big reed | sure | Same local targeting and random attacks as Song_25.4. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_25.fsm.txt:2420 |
| Song_25.7 | Song Reed Grand Cobweb ×2 | FSM Control e747f832d2 | Cobweb wakes on WAKE event | no | Cosmetic follower. | | | scenes/Song_25.fsm.txt:4215 |

## Song_26

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_26.1 | Music Box Sequence - EDITED ×3 | C# BouncePodSequence | Bell bounce-order puzzle; opens reward | sure | Plate press and pod hits come from the local hero only; progress (persistent int) advances in one game. | The plate press and each pod hit, or the sequence progress. | | scenes/Song_26.objects.txt:27 |
| Song_26.2 | music_box_support ×157 | C# AnimatorActivatingStates | Bell holder appear and disappear animations | no | Visual follower of the sequence. | | | scenes/Song_26.objects.txt:36 |

## Song_27

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_27.1 | Audio Cog Atmos ×2 | FSM follow_camera_bound e5d01769a2 | Audio follows camera bounds | no | Ambience. | | | scenes/Song_27.fsm.txt:1012 |
| Song_27.2 | terrain collider (puzzle_scaff_plat) ×10 | FSM tilt_plat ac2a0b4ed1 | Scaffold plat dips on landing | maybe | Tilts on the local hero landing only; the partner on it does not dip it. | The tilt. | | scenes/Song_27.fsm.txt:1060 |
| Song_27.3 | Pipe_Vent_Hatch_BG Open | FSM BG Control 2634e31938 | Background reacts to BG OPEN or CLOSE | no | Cosmetic. | | | scenes/Song_27.fsm.txt:2278 |
| Song_27.4 | Song Automaton Fly ×3 | FSM Control e51aaeee9f | Worker fly: works at spots, startles, swipes | sure | Alert, taunt and swipe react to the local hero; the battler variant attacks. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_27.fsm.txt:2288 |
| Song_27.5 | Song Automaton Fly ×3 | FSM detect_taunt 9e42a42dcd | Hears hero taunt in range and alerts fly | maybe | DID TAUNT comes from the local hero only. | The DID TAUNT event from either player. | | scenes/Song_27.fsm.txt:2531 |
| Song_27.6 | Song Automaton Fly Spot ×5 | FSM Control 4d6bb0808a | Work spot marker, no logic | no | Inert sprite. | | | scenes/Song_27.fsm.txt:3377 |
| Song_27.7 | March Groups | FSM Control aaa626ddc3 | Trigger picks march direction from hero side | sure | Direction uses the local hero side, so the games can march opposite ways. | The chosen direction (MARCH L or R). | | scenes/Song_27.fsm.txt:3508 |
| Song_27.8 | Clear Out Group | FSM FSM 74e10e288b | Marks absent-variant pilgrims dead in save | maybe | Same present or absent knight mismatch as Song_02.4. | The songChevalierActive flag. | | scenes/Song_27.fsm.txt:5978 |
| Song_27.9 | Pilgrim 04 Song | FSM Control 42a22c1b42 | Flying pilgrim: patrols, charges hero | sure | Alert and charge read the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_27.fsm.txt:6580 |
| Song_27.10 | Song Pilgrim 01 (2) | FSM Attack 7851e94961 | Ground pilgrim: slashes, charges, hops | sure | Targets and attack picks are local and random. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Song_27.fsm.txt:6849 |
| Song_27.11 | Cog Damager ×290 | C# CogMultiHitter | Spike cog grinds and hurts the hero | no | Hurts only the local hero; hazard state is deterministic. | | | scenes/Song_27.objects.txt:2034 |

## Song_28

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_28.1 | Toll_machine_silk_ration | FSM Behaviour f07d9861ec, C# SilkRationMachine | Pay rosaries; machine drops a silk ration | sure | Stock count (persistent int), jam roll and rosary payment are local, so both players can buy the last ration. | The purchase (stock decrement and jam). | Whose rosaries pay, and who gets the ration. | scenes/Song_28.fsm.txt:4, scenes/Song_28.objects.txt:444 |

## Song_29

No new elements.

## Song_Enclave

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_Enclave.1 | Atmos Control, Audio Player Wind, Enclave Music Control, Chime Sfx | FSM Control 734b01db2b, FSM f245a1f65f, Control eac65a4b1d, Chime Sfx e63815440e | Ambient audio, music cue by enclave PD state | no | Local audio. | | | scenes/Song_Enclave.fsm.txt:4, scenes/Song_Enclave.fsm.txt:18, scenes/Song_Enclave.fsm.txt:204, scenes/Song_Enclave.fsm.txt:420 |
| Song_Enclave.2 | City Merchant Enclave ×2 | FSM Dialogue d43d535187 | Merchant dialogue, legend convo, then shop | maybe | Writes MetCityMerchantEnclave and bridge remeet flags and quest checks per player; shop stock is a separate system. | The Met and remeet PD flags. | | scenes/Song_Enclave.fsm.txt:25 |
| Song_Enclave.3 | Seen Range ×2 | FSM FSM 7d7af64447 | Sets merchant seen or left library flags | maybe | Flags set by the local hero entering. | The cityMerchantRecentlySeenInEnclave flag. | | scenes/Song_Enclave.fsm.txt:127 |
| Song_Enclave.4 | Enclave Simple NPC Standard ×4 | FSM Met State ce864742b6, Rest Animation a33972382c | Scared pilgrim sets met flag; resting idle anim | no | Flag follows the dialogue; anim is cosmetic. | | | scenes/Song_Enclave.fsm.txt:145, scenes/Song_Enclave.fsm.txt:161 |
| Song_Enclave.5 | Sherma Caretaker | FSM Control 74964430b1 | Act 3 Sherma: convo flags, white flower final convo | maybe | Conversation flags per player; final convo depends on HasWhiteFlower. | The shermaCaretakerConvo flags. | | scenes/Song_Enclave.fsm.txt:261 |
| Song_Enclave.6 | Enclave Simple NPC Large ×4, Enclave Pilgrim Small | FSM Rest Animation 3e1e7ba7ca, aa9ab1f23f | Resting NPC idle animations | no | Cosmetic. | | | scenes/Song_Enclave.fsm.txt:471, scenes/Song_Enclave.fsm.txt:2310 |
| Song_Enclave.7 | door1 | FSM Re-enable Door after caretaker meet ed81975a19 | Door re-enables after caretaker meeting | maybe | Gated by the local caretaker meeting. | Follows Song_Enclave.9 (metCaretaker). | | scenes/Song_Enclave.fsm.txt:1469 |
| Song_Enclave.8 | Enclave States | FSM State Setup ffef587dcf | Picks enclave NPC set from enclaveLevel and bellshrine | maybe | Players with different enclaveLevel load different NPC sets. | Keep enclaveLevel and bellShrineEnclave equal. | | scenes/Song_Enclave.fsm.txt:1476 |
| Song_Enclave.9 | Enclave Caretaker FirstMeet | FSM Dialogue 13b08bccad | Caretaker first meet: calls out, tent, sets metCaretaker | sure | Scripted first meeting with dialogue runs once for the local hero and enables the door. | The first-meet start and the metCaretaker flag. | Run for the first arrival only, for both, or hold the partner outside. | scenes/Song_Enclave.fsm.txt:1582 |
| Song_Enclave.10 | Enclave Caretaker PreBellshrine | FSM Sing React b064ff318b | Caretaker peeks out when hero sings | no | Reacts to local needolin only; ambience. | | | scenes/Song_Enclave.fsm.txt:1725 |
| Song_Enclave.11 | Enclave Caretaker (Level 1) | FSM Dialogue 793dda5bc4 | Caretaker dialogue ladder, snare offers | maybe | Sets caretakerConvoLv1-3 and other convo flags for the talker. | The caretakerConvo flags. | | scenes/Song_Enclave.fsm.txt:1914 |
| Song_Enclave.12 | Enclave Simple NPC Tall, Short Horned | FSM State Control b73a0ae195, Met State bbaf01cd04 | Show or hide NPCs by defeatedLaceTower and melodies | maybe | Presence depends on local PD flags. | Keep defeatedLaceTower and HasMelody flags equal. | | scenes/Song_Enclave.fsm.txt:2373, scenes/Song_Enclave.fsm.txt:4423 |
| Song_Enclave.13 | Sherma Enclave NPC | FSM Conversation Control 83254bc443 | Sherma enclave talk, healing convo flags | maybe | Sets metShermaEnclave and healing convo flags per player. | The metSherma flags. | | scenes/Song_Enclave.fsm.txt:4708 |
| Song_Enclave.14 | door_act3_wakeUp | FSM Cutscene Control 8a7af6b32e | Act 3 wake-up: prostrate, Sherma talk, quest begin | sure | Forced long cutscene on the local hero; sets act3_enclaveWakeSceneCompleted, act3MapUpdated and begins quest Black Thread Pt1 Shamans. | Start of the scene and its end flags. | Pull the partner in and freeze them, or leave them free. | scenes/Song_Enclave.fsm.txt:4917 |
| Song_Enclave.15 | sc_drape_set_passage (3) ×34 | C# MaterialPropertyRegion | Fades drape material by position | no | Visual. | | | scenes/Song_Enclave.objects.txt:493 |
| Song_Enclave.16 | Enclave Simple NPC Tall, Pilgrim Small, Short Horned ×5 | C# NPCEncounterStateController, BasicNPCRepeatDialogueLeaveCondition | NPC stays, leaves after repeat dialogue (random) | maybe | Leave state is saved in PD and can roll leave on load, so an NPC can be gone in one game only. | The NPC encounter state (met, left). | | scenes/Song_Enclave.objects.txt:1294 |
| Song_Enclave.17 | Bell Shrine Silk | C# PersistentIntItemResponder | Reacts to bell shrine sequence int | maybe | Follows the bell shrine sequence step of the local game. | Follows the bell shrine sequence sync. | | scenes/Song_Enclave.objects.txt:1822 |

## Song_Enclave_Tube

No new elements.

## Song_Tower_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_Tower_01.1 | Battle Particles | FSM End On Lace Defeat f6cc1c9bb1 | Stops particles on Lace death | no | Cosmetic. | | | scenes/Song_Tower_01.fsm.txt:4 |
| Song_Tower_01.2 | Flower Effect Hornet | FSM Control c061534825 | Flower effect when hero is in area | no | Cosmetic. | | | scenes/Song_Tower_01.fsm.txt:10 |
| Song_Tower_01.3 | Lace Boss2 New | FSM Control 281259e033 | Lace boss: hops, counters, combos, charges, phases | sure | Boss target, attack choice and teleport read the local hero and use random picks; arena, music and boss title start locally. | One game runs Lace; share her state, HP, phase and death. | Lock in both players, pull the partner into the arena, or leave them out; target nearest player or the starter. | scenes/Song_Tower_01.fsm.txt:25 |
| Song_Tower_01.4 | Lace Boss2 New | FSM Multicircle 100599d63c | Spinning multi-slash circle attack | sure | Part of the boss attack set. | Follows Song_Tower_01.3. | | scenes/Song_Tower_01.fsm.txt:1339 |
| Song_Tower_01.5 | Cross Slash, lace collider | FSM Multihit 66dafc54a0, Multihitter a7ed9376a1 | Cross slash pins hero in place and hides them | sure | Moves and hides only the local hero (position, mesh, velocity); the partner is not grabbed. | The grab (victim) and release. | Grab only the targeted player, or whoever is hit first. | scenes/Song_Tower_01.fsm.txt:2218, scenes/Song_Tower_01.fsm.txt:2292 |
| Song_Tower_01.6 | Corpse Lace2(Clone), NPC | FSM Control b882fa179f, Control 518176df45 | Defeated Lace lands, post-fight talk, collapses | sure | Post-fight dialogue and hero control run only for the talking hero; saves corpse position in PD and triggers SILK HEART APPEAR. | The corpse position and the end of the talk. | Let the partner join the talk, freeze them, or leave them free. | scenes/Song_Tower_01.fsm.txt:2051, scenes/Song_Tower_01.fsm.txt:2158 |
| Song_Tower_01.7 | Lace Return Corpse | FSM Position e0418067ae | Places the Lace corpse from saved position | maybe | Uses the local saved laceCorpsePosX. | The corpse position PD floats. | | scenes/Song_Tower_01.fsm.txt:2522 |
| Song_Tower_01.8 | Audio Player Steam | FSM Fade Control d6d64eb968 | Steam hazard audio fade | no | Audio. | | | scenes/Song_Tower_01.fsm.txt:3084 |
| Song_Tower_01.9 | door_cutsceneEndLaceTower | FSM Travel Control 28d6d6d0c5 | Lift arrival, first cinematic, then starts Lace fight | sure | Locks the local hero, sets encounteredLaceTower, and sends BATTLE START FIRST or REFIGHT only in that game. | The boss start (first or refight). | Start the fight when the first player arrives, or wait for both. | scenes/Song_Tower_01.fsm.txt:3203 |
| Song_Tower_01.10 | Left Wall Hero Saver | FSM Control f4fdf720cc | Pushes hero back if past left wall | no | Local safety clamp. | | | scenes/Song_Tower_01.fsm.txt:3376 |
| Song_Tower_01.11 | song_tower_right_gate | FSM Control dc706879af | Gate opens after Lace defeated; sets door opened | maybe | Opens when the local hero stays in the trigger; laceTowerDoorOpened is local. | The laceTowerDoorOpened flag. | | scenes/Song_Tower_01.fsm.txt:3389 |

## Song_Tower_Destroyed

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Song_Tower_Destroyed.1 | Act3 Intro Cutscene | FSM Control df0f15efb0 | Act 3 intro: hero prostrate, rise, first bind tutorial | sure | Forces the local hero prone and locks pause and camera; sets act3_wokeUp and bindCutscenePlayed. | Start of the scene and its end flags. | Pull the partner in and freeze them, or leave them free. | scenes/Song_Tower_Destroyed.fsm.txt:4 |
| Song_Tower_Destroyed.2 | Audio Rock Dropper ×6 | FSM Control ce3288308f | Random falling rock audio | no | Ambience. | | | scenes/Song_Tower_Destroyed.fsm.txt:152 |
| Song_Tower_Destroyed.3 | Flower Effect Hornet | FSM Control 4154350d15 | Flower effect when hero is in area | no | Cosmetic. | | | scenes/Song_Tower_Destroyed.fsm.txt:237 |
| Song_Tower_Destroyed.4 | pivot (song_tower_right_gate) | FSM Control c330a79ecf | Gate pivot dips on landing or tink | no | Visual reaction to local collisions. | | | scenes/Song_Tower_Destroyed.fsm.txt:261 |

## Summary

1. Events that should pull the partner to the player who triggered them (or hold them), each a forced cutscene or boss with hero lock:
   1.1. Song_05.1 Lace meets Hornet at the Citadel: locks the hero, walks them, lifts them with silk threads, begins quest Citadel Ascent. Without it the partner has citadelWoken and the quest start only if flags are shared.
   1.2. Song_Tower_01.9 and Song_Tower_01.3 Lace boss: lift arrival cinematic, BATTLE START and arena. Lace targets one hero only; cross slash (Song_Tower_01.5) pins one hero.
   1.3. Song_Tower_01.6 post-fight Lace talk and silk heart: dialogue locks only the talker.
   1.4. Song_02.5 Song Knight duel: dialogue then boss fight targeting the local hero.
   1.5. Song_04.3 Wake Scene and its waves with Handmaidens, and Song_07.1 merchant battle: arena start by the local hero only, others not locked in.
   1.6. Song_25.1, Song_Enclave.14, Song_Tower_Destroyed.1: the three Act 3 opening cutscenes (weakened walk, enclave wake-up, bind tutorial) lock and force the hero; they also set act3 flags and quests.
   1.7. City_Lace_cutscene.1: door cinematic that switches scene for one player only.
   1.8. Song_09b.2 Gourmand scene and Song_Enclave.9 caretaker first meet: scripted scenes locking inventory, pause or dialogue.

2. Cross-cutting findings:
   2.1. Handmaiden call (Song_01.4, Song_13.1, Song_04.6): every game rolls its own percent and sets marionettesMet, so one game gets a Handmaiden and the other none. Share the CALL decision.
   2.2. Silk Suck (Song_04.5) and spear (Song_04.9): take silk and homing from the local hero only. Design choice: drain the targeted player, or all players in range.
   2.3. Song Knight present or absent: three Clear Out Group FSMs (Song_02.4, Song_25.5, Song_27.8) and Resting NPC (Song_07.3) switch enemy sets from per-player PD flags (songChevalierActiveIn, SeenIn, Encounters). Both games must agree or the scene holds different enemies.
   2.4. March groups (Song_17.1, Song_27.7): direction picked from local hero side, so two games can march opposite ways. Share the chosen direction.
   2.5. Timed platforms and hazards (MetronomePlat Song_11.4, steam vents Song_10.10 and Song_18.1, tilt plats Song_27.2, scale pans Song_19_entrance.1-2): each game has its own clock, and tilt follows only the local hero landing. A partner standing on a platform that is gone in the other game is the visible failure.
   2.6. Lever and break pieces to share by one hit event each: Song_01.6 floor, Song_01.9 vent lever pair, Song_10.2 vine cut, Song_11.3 propeller lever, Song_19_entrance.3 scale lock lever, Song_20b.1 bridge door lever, Song_25.2 hatch, Song_14.1 flea cage.
   2.7. Silk ration loot and machine (Song_03.2-4, Song_28.1): per-game roll and stock, so both players can take the same ration or buy the last one; decide whose rosaries pay and who gets the silk.
   2.8. Needolin reactions (enemy sing states such as Song_01.1 and Song_01.2, caretaker Song_Enclave.10) read the local hero only; the partner playing needolin is ignored in the other game. Decide whether partner needolin should count.
   2.9. Gaps: the Song_04 battle (waves, handmaiden respawn, battle end PD bool) and Song_07 merchant battle need the arena open and gates closed in both games; this slice does not show arena exits, only starts.
