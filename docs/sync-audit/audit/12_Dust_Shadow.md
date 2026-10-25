# Audit 2.12: Dust and Shadow (Dust_01 .. Dust_Shack, Shadow_01 .. Shadow_Weavehome)

Slice: catalog/Dust.md and catalog/Shadow.md. Verdict means: does coop need to handle it. Dust is Sinner's Road plus the Mist maze; Shadow is Bilewater. Enemy rows are `sure` when target choice, attack choice or RNG reads the local hero only; plain follower FSMs (audio, particles, jitter) are `no`. Where a row says "one game decides", the other game should replay enemy state, HP and death.

## Dust_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_01.1 | Roachfeeder Tall ×N | FSM Control a80a39ea7f | Flying roach: patrols, alerts, chases, multislashes | sure | Alert, chase and slash target the local hero, so two games chase two heroes and raise their own alarm. | One game decides target and attack choice; share enemy state, HP and death. | Target nearest player, or only the player who woke it. | scenes/Dust_01.fsm.txt:4 |
| Dust_01.2 | Simple Battle | FSM Alarm Start 0bb0b24249 | On RAISE ALARM wakes every roach | sure | The alarm is raised by a roach noticing one hero, so only that game wakes its roaches. | The RAISE ALARM event. | | scenes/Dust_01.fsm.txt:435 |
| Dust_01.3 | Dustroach ×3 | FSM Control ccb9148685 | Ground roach: sleeps, wakes, chases, leaps to grab | sure | Wake range, chase and leap read only the local hero and use random picks. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Dust_01.fsm.txt:500 |
| Dust_01.4 | Dustroach ×3 | FSM Detect Grab 2bd4b40a67 | Grab collider asks if hero can be grabbed | maybe | Only the local hero collider can be grabbed, so the partner is never grabbed in the other game. | Follows the target decision of Dust_01.3: grab applies to the targeted player. | | scenes/Dust_01.fsm.txt:457 |
| Dust_01.5 | Dustroach ×3 | FSM Run Msg 55b409d455, Run Vox 438fddab8b, Above Timer 97d8a5a4a0 | Relay run direction, voice loop, range timer | no | Pure helpers that follow the enemy state. | | | scenes/Dust_01.fsm.txt:442, scenes/Dust_01.fsm.txt:471, scenes/Dust_01.fsm.txt:1243 |
| Dust_01.6 | Dustroach (1) | FSM Cage Stick f378737104 | Sticks to cage on WAKING | no | Cosmetic follower of the wake event. | | | scenes/Dust_01.fsm.txt:1254 |
| Dust_01.7 | cage plat colliders ×117 | FSM Enemy Rattle 065db1e6e6 | Jitters cage when enemy lands on it | no | Visual reaction to local collisions. | | | scenes/Dust_01.fsm.txt:2891 |
| Dust_01.8 | Dustroach FG ×10 | FSM Control 622af8b899 | Background roach wakes, barks, runs off | no | Ambience; the noise it makes only wakes local enemies. | | | scenes/Dust_01.fsm.txt:3186 |
| Dust_01.9 | Cage Scene | FSM Control 0869c3d705 | On BREAK swaps sleeper for awake Dustroach | maybe | Cage opens only in the game where the wall broke unless wall break itself is shared. | Nothing extra if the Breakable wall that sends BREAK is shared. | | scenes/Dust_01.fsm.txt:3435 |
| Dust_01.10 | Dustroach Caged ×11 | FSM Control d79e191b97 | Caged roach sleeps, barks, shakes, can be killed | maybe | Wake reads the local hero and needolin; death of a caged roach differs per game. | Death like any enemy; the rest is ambience. | | scenes/Dust_01.fsm.txt:4445 |
| Dust_01.11 | Dustroach Caged ×11 | FSM harpoon hack cbe3f03688 | Toggles collider during harpoon throw | no | Local tool-use workaround. | | | scenes/Dust_01.fsm.txt:4557 |
| Dust_01.12 | Lace Encounter Control | FSM Control 95f7d2df35 | Starts Lace meeting cutscene if never met | sure | Takes hero control, camera lock, dialogue, writes encounteredLaceBlastedBridge and laceLeftDocks, saves; the partner would see Lace appear and vanish, or get a second meeting. | The moment the cutscene starts (whose PD flags are written); the partner's flag must match. | Pull partner and freeze both, run it for the starter only and mark partner as met, or replay it for the partner on arrival. | scenes/Dust_01.fsm.txt:6379 |
| Dust_01.13 | Lace Encounter Control | FSM Control e9cb66d9e5 | Music snapshot reset after Lace leaves | no | Audio only. | | | scenes/Dust_01.fsm.txt:6929 |
| Dust_01.14 | Message Controller ×2 | FSM Control 101f10c4bb, ef18caa67d | Swaps hint set when hero crosses trigger | no | Local UI hint tied to the local hero position. | | | scenes/Dust_01.fsm.txt:6940, scenes/Dust_01.fsm.txt:6957 |

## Dust_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_02.1 | cage_mid_small_sway (1) | FSM Extra Chain e1572ce759 | Rotates extra chain links to anchor | no | Visual. | | | scenes/Dust_02.fsm.txt:1138 |
| Dust_02.2 | Spike Pendulum Hanging ×29 | FSM Control 3a25acc970 | Random swing, recoils when hit | no | Damage hits only the local hero; swing phase is cosmetic. | | | scenes/Dust_02.fsm.txt:2286 |
| Dust_02.3 | Spike Pendulum Hanging Spike ×30 | FSM Tink Effects 1d406e83c7 | Dust and jitter when nail tinks | no | Visual. | | | scenes/Dust_02.fsm.txt:2346 |
| Dust_02.4 | Green Prince Sit | FSM Extra Animation a7dde89b1e | Rest and look animations for caged prince | no | Visual. | | | scenes/Dust_02.fsm.txt:4095 |
| Dust_02.5 | Prince Cell Door | FSM Cell Door 4abd937dfc | Unlock breaks cell, writes UnlockedDustCage, prince talks | sure | One-time world change plus NPC dialogue; the partner's door stays shut and a second UNLOCK could replay. | The UNLOCK event (key use); each game then plays the break and sets the PD flag. | Dialogue per player or only the unlocker. | scenes/Dust_02.fsm.txt:4146 |
| Dust_02.6 | cage_mid_small_longer_move_EDITED | C# MovePlatform | Platform moves to active position on unlock | sure | Both players ride it; it moves only in the game that got the unlock (a lever sits nearby). | The unlock trigger (lever hit); see note MovePlatform. | | scenes/Dust_02.objects.txt:2500 |

## Dust_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_03.1 | Battle Scene / Simple Battle | FSM Alarm Start d475f36384, a44312d8fc | On RAISE ALARM starts wave battle, wakes roaches | sure | The alarm comes from a roach death or wake near one hero, so only that game closes gates and spawns waves. | The RAISE ALARM (battle start) and the battle end. | Lock both players in the arena, or only the one who raised the alarm. | scenes/Dust_03.fsm.txt:4, scenes/Dust_03.fsm.txt:8322 |
| Dust_03.2 | Battle Gate Dustpens ×2 | FSM BG Control 1961b7cd25 | Closes and opens arena gate on BG events | sure | Gate closes only where battle started and opens only where it ended; the partner is trapped or walks through. | Battle start and end; each game closes and opens its gates. | Same as Dust_03.1. | scenes/Dust_03.fsm.txt:10 |
| Dust_03.3 | Dustroach Runner ×3 | FSM Control ac483490fa | Runs across arena at wave start, then vanishes | no | Ambient runner; only its direction reads the local hero X. | | | scenes/Dust_03.fsm.txt:1168 |
| Dust_03.4 | Return Scene | FSM Alarm d24f8cdcd2 | On RAISE ALARM sends BATTLE START to return wave | sure | Return-wave roaches fly in only in the game that raised the alarm. | Same RAISE ALARM as Dust_03.1. | | scenes/Dust_03.fsm.txt:3344 |
| Dust_03.5 | Message Controller ×6 | FSM Control 6fd7d25008 | Swaps hint set when hero crosses trigger | no | Local UI hint. | | | scenes/Dust_03.fsm.txt:14486 |

## Dust_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_04.1 | Courier Scene | FSM Save Courier 8d99ce714a | Counts guard deaths, sends COURIER SAVED | sure | Courier is freed only in the game whose count hits zero; kills by the partner are not counted. | Guard deaths (as normal enemy deaths) or the COURIER SAVED event. | | scenes/Dust_04.fsm.txt:1641 |
| Dust_04.2 | Courier Tall | FSM Cower ca1c47cff0 | Cowers, then thanks hero, ends quest, flies away | sure | Quest end, dialogue and fly-away are one-time and write quest state; the partner could still find the courier cowering. | The COURIER SAVED event and quest completion. | Quest completes for both, or per player with a separate thank-you. | scenes/Dust_04.fsm.txt:1659 |
| Dust_04.3 | Spike Pendulum Upright ×10 | FSM Control ceae44e333, Tink Effects 5b1aaeb728 | Swinging spike, reacts to nail tinks | no | Damage only hits the local hero; swing is cosmetic. | | | scenes/Dust_04.fsm.txt:4668, scenes/Dust_04.fsm.txt:4730 |

## Dust_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_05.1 | Roachkeeper - Garmond Target ×4 | FSM Control 95b4161c32 | Miniboss: walks, whips, rolls, punches (hp 100) | sure | Attack choice and range checks use the local hero; "Garmond Not Here" variant is chosen by PlayerData. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Dust_05.fsm.txt:5858 |
| Dust_05.2 | Roachkeeper ×4 | FSM Protect From Below 68bf8d5a6d | Invulnerable while hero is below | maybe | Immunity flips by the local hero position, so it differs if the enemy targets the partner. | Follows the target decision of Dust_05.1. | | scenes/Dust_05.fsm.txt:6202 |
| Dust_05.3 | Music Fader | FSM FSM 5877734e58 | Fades music silent on trigger | no | Audio. | | | scenes/Dust_05.fsm.txt:7103 |
| Dust_05.4 | Spike Cog Audio ×10 | FSM Control 3470ac72ad | Plays cog sound by camera distance | no | Audio. | | | scenes/Dust_05.fsm.txt:9682 |
| Dust_05.5 | Spike Pendulum Controller (1) | C# TimedTicker | Sends TICK events every N seconds | no | Timing is per game; damage is local (see note TickedPendulum). | | | scenes/Dust_05.objects.txt:1270 |
| Dust_05.6 | Spike Pendulum ×44 | C# TickedPendulum | Swings spike pendulum on each tick | no | Swing phase differs between games but damage is local. | | | scenes/Dust_05.objects.txt:1477 |
| Dust_05.7 | left1 ×2 | C# RecordDoorEntry | Writes previous-scene name to PlayerData on exit | maybe | Written only for the hero that leaves; matters only where code reads the stored scene. | Nothing unless the stored scene drives shared logic. | | scenes/Dust_05.objects.txt:1885 |

## Dust_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_06.1 | greymoor_flip_bridge (3) | FSM Start Open If Key Collected f9bc325f93 | Opens bridge at load if cage key collected | maybe | Checked once at load, so the bridge opens for the partner only after they reload the room. | CollectedDustCageKey flag, plus a live OPEN when the key is picked up. | | scenes/Dust_06.fsm.txt:1528 |
| Dust_06.2 | Roachkeeper Key Control | FSM Control 66ad720aa7 | Keykeeper defeat spawns the cage key pickup | sure | One persistent pickup that sets CollectedDustCageKey; spawned only where the keeper died. | The keeper death (KEYKEEPER DEFEATED) and the pickup being taken. | Key goes to the first picker, to both players, or into shared inventory. | scenes/Dust_06.fsm.txt:3031 |

## Dust_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_09.1 | _SceneManager | FSM FSM 72eeeddfe9 | Picks music layer from defeatedPhantom | no | Audio. | | | scenes/Dust_09.fsm.txt:4 |
| Dust_09.2 | BG Exhaust Effect ×8 | FSM Control 5988d70fc9 | Looping steam audio and effect | no | Ambience. | | | scenes/Dust_09.fsm.txt:65 |
| Dust_09.3 | Dust Maze Controller | C# MazeController | Rewires maze doors and counts progress in PlayerData | sure | Random door links and scene seed are rolled per game and counters are per save, so the players diverge or never leave together. | Scene seed, door links and the progress counters (see note MazeController). | One shared maze state with the partner following the first player's door, or a fully independent maze per player. | scenes/Dust_09.objects.txt:1629 |
| Dust_09.4 | Dust Maze Controller | C# MazeCorpseSpawner | Spawns hero corpse in maze if death was in maze | maybe | Reads only the local save death scene; a partner death is not shown. | HeroCorpseScene if corpses are shared. | | scenes/Dust_09.objects.txt:1629 |

## Dust_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_10.1 | bell_bench_underside | FSM Control 269b0c4b26 | BREAK repairs cogs, sets FixedDustBellBench | sure | One-time persistent repair that unlocks the bell bench; the partner's cogs stay broken until reload. | The BREAK event (cog hit); each game plays the repair. | | scenes/Dust_10.fsm.txt:1106 |
| Dust_10.2 | Spike Pendulum (16), (17) | FSM Control 96df0b4307 | Swings on TICK | no | Damage is local; phase cosmetic. | | | scenes/Dust_10.fsm.txt:4226 |
| Dust_10.3 | Spike Pendulum Controller | FSM Tick af02cd73c9 | Sends TICK and TICK HALF repeatedly | no | Local clock. | | | scenes/Dust_10.fsm.txt:4262 |
| Dust_10.4 | Support Plat terrain collider ×3 | FSM FSM 841d5f6c3a | Dust and sway when hero lands | no | Visual reaction to local landing. | | | scenes/Dust_10.fsm.txt:4344 |
| Dust_10.5 | bell_bench Bell parts ×7 | C# JitterFixPosition | Fixes jitter of bell parts | no | Visual. | | | scenes/Dust_10.objects.txt:590 |

## Dust_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_11.1 | Grub Farmer NPC | FSM Tending Anim Control ebf4c922e6 | Tending animation loop | no | Visual. | | | scenes/Dust_11.fsm.txt:4936 |
| Dust_11.2 | Grub Farmer NPC | FSM Control e601758445 | Meeting, silk-farm growth, delivery quest, farm battle | sure | Writes metGrubFarmer, grubFarmLevel, grubFarmerEmerged, quest end and reward, and ends a farm battle; per-save progress differs per player. | Meeting and quest-step events (PD writes), battle start and end. | Farm progress and quest shared, or per player with a first-meeting rule. | scenes/Dust_11.fsm.txt:4959 |
| Dust_11.3 | Large Cocoon ×3 | FSM Jitter d7c4be6c6d | Shakes cocoon on JITTER | no | Visual. | | | scenes/Dust_11.fsm.txt:5452 |
| Dust_11.4 | Little Cocoon ×3 Break Effects | FSM Broadcast 7b5facf4dd | Broadcasts event when little cocoon breaks | maybe | Breaking one before it ripens startles the farmer; only the local game reacts. | The cocoon break event. | | scenes/Dust_11.fsm.txt:5476 |
| Dust_11.5 | silk_farm_centre, silk_farm_lvl2 | FSM Shake Control 3c0b12c836 | Broadcasts shake to farm parts | no | Visual. | | | scenes/Dust_11.fsm.txt:6288 |
| Dust_11.6 | Grub Farmer Mimic | FSM Dialogue 09d7181bca | Steel Soul: swaps a Silk Grub for rosaries | sure | Takes a Silk Grub, adds rosaries and GrubFarmerSilkGrubsSold, sets MetGrubFarmerMimic; each player has their own grub stock. | The trade (item taken and rosaries paid). | Grubs and rosaries shared or per player. | scenes/Dust_11.fsm.txt:6676 |
| Dust_11.7 | Event Register Region ×6 | C# SendEventToRegisterOnTriggerExit | Sends a named event when a collider leaves region | maybe | Fires from the local hero collider only; the partner leaving never triggers it. | The enter and exit events if they drive shared NPC state. | | scenes/Dust_11.objects.txt:3002 |

## Dust_12

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_12.1 | Break Wall Dustpen Cage | FSM Wake Roaches a7d9572e76 | On wake raises alarm to wake roaches | sure | Breaking the wall raises the alarm only in the breaker's game; the partner's roaches stay asleep. | The wall break (persistent Breakable); each game then raises its own alarm. | | scenes/Dust_12.fsm.txt:19 |

## Dust_Barb

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Barb.1 | pontoon ×3 | FSM bobbing_platform f20051d8b9 | Drifts randomly, bobs when landed on | maybe | Drift direction is random and bobbing reacts to local landings, so the platform sits at different spots. | Nothing, or its position if players ride it together. | | scenes/Dust_Barb.fsm.txt:835 |

## Dust_Chef

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Chef.1 | Battle Floor | FSM Control 746a02367c | Floor dips on FLOOR SMASH | no | Visual follower of the boss. | | | scenes/Dust_Chef.fsm.txt:4 |
| Dust_Chef.2 | Hero Stopper | FSM Control b61f46dc1a | Enables blocking collider on BG CLOSE | sure | Keeps the hero in the arena only in the game where the battle started. | Battle start; each game closes its gate. | Lock both players in. | scenes/Dust_Chef.fsm.txt:24 |
| Dust_Chef.3 | Roachkeeper Chef Tiny ×5 | FSM Control f59347ce41 | Kitchen minions: work, launch, fly around hero | sure | Fly distance and aggro target the local hero. | One game decides target; share enemy state, HP and death. | Target nearest player. | scenes/Dust_Chef.fsm.txt:33 |
| Dust_Chef.4 | Wave 1 | FSM Bg guy fly away e4570dae24 | On WAVE START sends BATTLE START | sure | Wave start is local to the battle owner. | Part of the battle start event. | | scenes/Dust_Chef.fsm.txt:382 |
| Dust_Chef.5 | Chef Stopper | FSM FSM 2a86369be6 | Destroys itself on BATTLE START | no | One-shot cleanup after battle start. | | | scenes/Dust_Chef.fsm.txt:1086 |
| Dust_Chef.6 | Roachkeeper Chef (1) | FSM Control 21f889f847 | Boss: swipe, stomp, butt charge, spawns blobs | sure | Attack choice is random and aimed at the local hero; stomps and blobs spawn per game; stun and phase changes differ. | One game decides target and attack; share boss state, HP and death. | Target nearest player, or the player who struck the gong. | scenes/Dust_Chef.fsm.txt:1092 |
| Dust_Chef.7 | Corpse Roachkeeper Chef | FSM Death 962ec59293 | Boss death: explosion, fanfare, falls into cauldron | sure | Boss defeat and fanfare happen only where the boss died; the partner's boss lives on. | Boss death (defeatedRoachkeeperChef). | | scenes/Dust_Chef.fsm.txt:1679 |
| Dust_Chef.8 | Kitchen Pipe Gong | FSM Gong Hit Reaction a887cf48df | Hero hit with hero inside starts boss battle | sure | Starts the battle (roar, lock, hatch) only in the striker's game and reads defeatedRoachkeeperChef. | The gong hit that starts the battle. | Pull partner into the arena when it starts. | scenes/Dust_Chef.fsm.txt:2443 |
| Dust_Chef.9 | kitchen_gong | FSM Tink Hit Force 94a61d9354 | Kicks gong on tink, sends GONG HIT | sure | GONG HIT is what starts the battle; sent only in the striker's game. | Same gong hit as Dust_Chef.8. | | scenes/Dust_Chef.fsm.txt:2591 |
| Dust_Chef.10 | Hero Detector Close ×10 | C# ColliderExpandTo | Expands detector collider at load | no | One-time geometry setup. | | | scenes/Dust_Chef.objects.txt:1939 |

## Dust_Maze_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Maze_01.1 | Mist Maze Controller ×10 | FSM mist_maze_controller e7cbc989e1 | Spawns silkflies, traps, wraiths by maze level | sure | Picks children with the per-game scene seed and reads CorrectMazeDoorsEntered, so players see different traps and wraiths. | Scene seed and maze counters (see note MazeController). | Same as Dust_09.3. | scenes/Dust_Maze_01.fsm.txt:4 |
| Dust_Maze_01.2 | cage_mid_small_long_sway drop ×19 | FSM Detect Landing 9b1cc57706 | Cage platform falls and breaks when landed on | sure | Players share the platform; it falls only in the game of whoever landed. | The landing (fall trigger); each game plays the fall and remnant. | | scenes/Dust_Maze_01.fsm.txt:554 |
| Dust_Maze_01.3 | cage drop ×7 | C# DestroyCallback | Callback when the cage object is destroyed | no | Follows Dust_Maze_01.2. | | | scenes/Dust_Maze_01.objects.txt:1632 |
| Dust_Maze_01.4 | Dust Trap Spike Dropper ×28 | FSM Control 8aaa455869 | Falls when hero in alert range, becomes terrain | maybe | Alert range sees only the local hero; the fallen spike becomes a platform in one game only. | Drop trigger if the fallen spike matters as terrain. | | scenes/Dust_Maze_01.fsm.txt:3402 |
| Dust_Maze_01.5 | Dust Trap Spike Plate ×46 | FSM Control dcfd034290 | Spikes rise when hero steps on plate | maybe | Plate and damage react to the local hero only; the partner standing on it does nothing. | Step trigger if both should be hurt. | | scenes/Dust_Maze_01.fsm.txt:3437 |
| Dust_Maze_01.6 | Wraith | FSM Control 68964f74c8 | Chases hero, charges, grabs, drains silk | sure | Alert, chase, grab and silk drain target the local hero, with random heights. | One game decides target; share enemy state, HP, death and the grab. | Target nearest player. | scenes/Dust_Maze_01.fsm.txt:4858 |
| Dust_Maze_01.7 | Wraith | FSM Particle Control e629b56026 | Toggles particles when off camera | no | Visual. | | | scenes/Dust_Maze_01.fsm.txt:5198 |
| Dust_Maze_01.8 | Wraith Collider Charge | FSM Detect Grab 126583c5cc | Grab check on touch, notifies parent | maybe | Same as Dust_01.4: only the local hero collider can be grabbed. | Follows Dust_Maze_01.6. | | scenes/Dust_Maze_01.fsm.txt:5207 |
| Dust_Maze_01.9 | Wraith | C# SpriteFlashDistanceSilhouette | Tints silhouette by distance to hero | no | Visual. | | | scenes/Dust_Maze_01.objects.txt:2413 |
| Dust_Maze_01.10 | Dust Maze Mist Zone ×16 | C# MazeMistZone | Touching mist resets maze and sends hero to entrance | sure | Moves only the hero that touches it while shared counters are already reset. | The reset trip; the partner either follows or the counters stay per player. | Pull partner back to the entrance, or let each player fail alone. | scenes/Dust_Maze_01.objects.txt:2697 |

## Dust_Maze_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Maze_02.1 | Spike Pendulum Upright ×4 | FSM Control 434e00a77e | Random-direction swing, reacts to tinks | no | Damage local; swing cosmetic. | | | scenes/Dust_Maze_02.fsm.txt:1775 |
| Dust_Maze_02.2 | Spike Pendulum Upright Spike ×3 | FSM Tink Effects 94857bf252, a5924de590 | Dust and jitter on tink | no | Visual. | | | scenes/Dust_Maze_02.fsm.txt:1828, scenes/Dust_Maze_02.fsm.txt:1946 |
| Dust_Maze_02.3 | Spike Pendulum Controller (1) | FSM Tick 1343a53df5 | Sends TICK repeatedly | no | Local clock. | | | scenes/Dust_Maze_02.fsm.txt:5268 |
| Dust_Maze_02.4 | Wraith Summoner (1) | FSM Control 3c82e3b403 | On WRAITH CALL spawns 2 to 3 wraiths around | sure | Random count and side per game; summoned wraiths exist only where a wraith called. | Summon event with count and positions, or let the enemy owner spawn them. | | scenes/Dust_Maze_02.fsm.txt:5376 |

## Dust_Maze_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Maze_03.1 | Spike Pendulum Upright Spike ×3 | FSM Tink Effects 5e7357dfde | Dust and jitter on tink | no | Visual. | | | scenes/Dust_Maze_03.fsm.txt:3447 |

## Dust_Maze_04

no new elements

## Dust_Maze_05

no new elements

## Dust_Maze_06

no new elements

## Dust_Maze_07

no new elements

## Dust_Maze_08

no new elements

## Dust_Maze_08_completed

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Maze_08_completed.1 | cam shaker | C# CameraShake | Shakes camera when cage smashes | no | Local camera effect. | | | scenes/Dust_Maze_08_completed.objects.txt:701 |

## Dust_Maze_09_entrance

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Maze_09_entrance.1 | Music Control | FSM Control 1c3d74b72e | Maze tension music by entry gate | no | Audio. | | | scenes/Dust_Maze_09_entrance.fsm.txt:4 |
| Dust_Maze_09_entrance.2 | Silkfly Control | FSM FSM bb14031bd2 | Hides silkflies unless entered from right | no | Ambience chosen by the local entry gate. | | | scenes/Dust_Maze_09_entrance.fsm.txt:854 |
| Dust_Maze_09_entrance.3 | right1 | C# PlayerDataStringResponse | Reacts to a PlayerData string on exit door | maybe | Reads a per-save maze string, so the partner's door can behave differently. | The maze entrance string if maze state is shared. | | scenes/Dust_Maze_09_entrance.objects.txt:333 |

## Dust_Maze_crossing

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Maze_crossing.1 | Silkflies Control | FSM Control 42656c8988 | Shows silkflies by entry side | no | Ambience chosen by the local entry gate. | | | scenes/Dust_Maze_crossing.fsm.txt:128 |

## Dust_Maze_Last_Hall

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Maze_Last_Hall.1 | Directional Activation Control | FSM Control d00d39a926 | Picks silkflies side by entry gate and PreviousMazeScene | no | Ambience chosen by local entry and maze state. | | | scenes/Dust_Maze_Last_Hall.fsm.txt:4 |
| Dust_Maze_Last_Hall.2 | fog_sets | FSM Control aa3352ee60 | Picks fog side by entry gate | no | Visual. | | | scenes/Dust_Maze_Last_Hall.fsm.txt:884 |

## Dust_Shack

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Dust_Shack.1 | Dust Traders | FSM Shared Anims 6dce7f467e | Camera lock areas swap while talking | no | Local camera. | | | scenes/Dust_Shack.fsm.txt:9 |
| Dust_Shack.2 | Dust Traders | FSM Dialogue 0dfec26a2f | Roach Killing quest, Doctor Curse Cure pins sale | sure | Offers, completes and pays quests, sets DustTradersOfferedQuest and DustTradersOfferedPins, sells Extractor Machine Pins for rosaries; quest log and currency differ per player. | Quest accept and complete, and the pins purchase. | Quests and purchase shared, or per player. | scenes/Dust_Shack.fsm.txt:51 |
| Dust_Shack.3 | Dust Trader Large | FSM Rest Animation cd1dbd2eae, Sing Reacton 56498b9c3f | Rest and needolin-song animations | no | Visual. | | | scenes/Dust_Shack.fsm.txt:169, scenes/Dust_Shack.fsm.txt:207 |
| Dust_Shack.4 | Dust Trader Small | FSM Sing Reacton dd675b997f | Needolin-song animation | no | Visual. | | | scenes/Dust_Shack.fsm.txt:252 |

## Shadow_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_01.1 | Bloat Roach ×8 | FSM Control 21e1c5b29f | Hovers, evades, belches gas stream at hero | sure | Attack range, evade side and belch aim read the local hero; roach has a persistent flag and hp 80. | One game decides target and attack; share enemy state, HP and death. | Target nearest player. | scenes/Shadow_01.fsm.txt:28 |
| Shadow_01.2 | Corpse Bloat Roach ×15 | FSM Rotation 541412a218 | Spins corpse | no | Visual. | | | scenes/Shadow_01.fsm.txt:174 |
| Shadow_01.3 | Swamp Bounce Pod ×44 | FSM Twitch 676d08e21e | Random twitch animation | no | Visual. | | | scenes/Shadow_01.fsm.txt:768 |
| Shadow_01.4 | Swamp Bounce Pod Antic Sprite ×44 | FSM Do Explosion d23e52d69b | Pod explodes after antic, then disables | maybe | The explosion hazard and pod removal happen only in the game where the pod was triggered. | The pod trigger (bounce). | | scenes/Shadow_01.fsm.txt:784 |

## Shadow_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_02.1 | Swamp Goomba ×3 | FSM Control 36f6ac6e47 | Crawls, wakes on range, extracted to kill | sure | Wake range is the local hero; EXTRACT kill sets hasKilled and records the journal kill; hp 30. | Enemy state, HP and death; the extract kill as a normal death. | | scenes/Shadow_02.fsm.txt:28 |
| Shadow_02.2 | Swamp Goomba ×3 | FSM Drown 51dce3a5e0 | Dies when entering water | maybe | Drowning is physics; a drifting goomba could die only in one game. | Death (normal enemy death sync). | | scenes/Shadow_02.fsm.txt:90 |
| Shadow_02.3 | Corpse Swamp Goomba ×2 | FSM Do Explosion 2779c9b358, 0cb71928f4 | Corpse swells and explodes into a bounce pod blast | maybe | Blast is a hazard and bounce source in one game only. | Follows the goomba death. | | scenes/Shadow_02.fsm.txt:1486, scenes/Shadow_02.fsm.txt:1516 |

## Shadow_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_03.1 | stake_trap_swing ×10 | FSM Control 1c3b84a2e8 | On TRAP swings stake ball, persists when sprung | sure | A sprung trap stays swinging only in the game where the wire was tripped. | The TRAP event (wire trip); see Shadow_03.8. | | scenes/Shadow_03.fsm.txt:440 |
| Shadow_03.2 | stake_trap_swing Spike Ball ×10 | FSM Tink Hit 93fb1ece6b | Ball shoved by nail tinks | no | Local physics reaction. | | | scenes/Shadow_03.fsm.txt:495 |
| Shadow_03.3 | Enemy Recoil Blocker ×11 | FSM Control acb52ae927 | Solid unless an enemy is recoiling into it | no | Local collision helper. | | | scenes/Shadow_03.fsm.txt:515 |
| Shadow_03.4 | Swamp Muckman ×3 | FSM Control 65892fbc5f | Ambusher: digs out, hops, shoots darts, dives | sure | Ambush range, hop direction and dart aim use the local hero; sets metSwampMuckmen. | One game decides target and attack; share enemy state, HP and death. | Target nearest player. | scenes/Shadow_03.fsm.txt:1102 |
| Shadow_03.5 | Swamp Muckman ×3 | FSM Detect Moss Plat c57eb27e0d | Sends HIT MOSS PLAT on collision | no | Local collision helper. | | | scenes/Shadow_03.fsm.txt:1672 |
| Shadow_03.6 | Swamp Muckman Caller Audio ×4 | FSM Control 246a0410e9 | Random call sounds | no | Audio. | | | scenes/Shadow_03.fsm.txt:2839 |
| Shadow_03.7 | Swamp Muckman FG ×8 | FSM Control c0cd70c5ca | Foreground ambusher hops away when hero near | no | Ambience. | | | scenes/Shadow_03.fsm.txt:2866 |
| Shadow_03.8 | Swamp Trap Wire (1) ×17 | FSM Control 24d07efe6b | Hero touch or nail cut sets Activated, sends TRAP | sure | A persistent trap fires only in the game where the wire was touched or cut. | The wire trip (TRAP and Activated), then each game plays swing or shot. | | scenes/Shadow_03.fsm.txt:2911 |

## Shadow_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_04.1 | Swamp Mosquito ×2 | FSM Control 5657a117f5 | Hovers, chases, stabs hero, returns to node | sure | Alert, chase, jukes and stab aim read the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player. | scenes/Shadow_04.fsm.txt:906 |
| Shadow_04.2 | Swamp Mosquito Nodes | FSM node_control 2ac82b883f | Hands random home node to a mosquito | no | Follows the enemy position. | | | scenes/Shadow_04.fsm.txt:1676 |

## Shadow_04b

no new elements

## Shadow_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_05.1 | Area Title Controller ×2 | FSM Music 076c0430eb, fce03685c8 | Sets music snapshot | no | Audio. | | | scenes/Shadow_05.fsm.txt:4, scenes/Shadow_05.fsm.txt:12 |
| Shadow_05.2 | cage_mid_small_long_sway drop (2), (3) | FSM Persistence b7cec5fa07 | Empty persistence holder of fallen cage platform | sure | Fall is persistent (PersistentBoolItem); falling in one game leaves the partner's platform standing. | The landing, as in Dust_Maze_01.2. | | scenes/Shadow_05.fsm.txt:646 |

## Shadow_08

no new elements

## Shadow_09

no new elements

## Shadow_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_10.1 | Swamp Stake Shooter ×32 | FSM Control a561813251 | On TRAP fires three stake projectiles | maybe | Fires only in the game where the wire was tripped; damage is local. | Nothing extra beyond the wire trip (Shadow_03.8). | | scenes/Shadow_10.fsm.txt:4511 |

## Shadow_11

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_11.1 | stake_trap_spikeball_loose Scene | FSM FSM 688d11681a | On ROLL tells spike balls to roll | sure | Persistent rolling hazard starts in one game only (sender of ROLL is outside the dump). | The ROLL start event. | | scenes/Shadow_11.fsm.txt:4 |
| Shadow_11.2 | stake_trap_spikeball_loose ×5 | FSM Control f3f01460f3 | Rolls, falls, smashes; damages hero and enemies | sure | Physics roll can differ per game and the ball is destroyed persistently. | Roll start; each game plays the roll and smash. | | scenes/Shadow_11.fsm.txt:10 |
| Shadow_11.3 | Swamp Drifter ×2 | FSM Control 78470a7916 | Floating hazard enemy, wakes near hero, darts | sure | Wake and dart use the local hero and random drift; hp 1 with DamageHero. | One game decides wake; share enemy state and death. | Target nearest player. | scenes/Shadow_11.fsm.txt:402 |

## Shadow_12

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_12.1 | Swamp Muckman Tall ×2 | FSM Control 950c0e29e0 | Tall ambusher: digs out, hops, shoots, dives | sure | Same ambush and aim logic as Shadow_03.4, local hero only. | One game decides target and attack; share enemy state, HP and death. | Target nearest player. | scenes/Shadow_12.fsm.txt:2724 |

## Shadow_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_13.1 | Dustroach Pollywog ×10 | FSM Land Control e75b496fb0 | Land collider solid or not by range | no | Local collision helper. | | | scenes/Shadow_13.fsm.txt:133 |
| Shadow_13.2 | Dustroach Pollywog ×10 | FSM Control b475532f4f | Leaps from water, flounders, grabs hero | sure | Leap, flounder and grab use the local hero and random angles; hp 22. | One game decides; share enemy state, HP, death and the grab. | Target nearest player. | scenes/Shadow_13.fsm.txt:142 |
| Shadow_13.3 | Dustroach Pollywog Spawner ×3 | FSM Control 8ce5b310ab | Picks random pollywog to spawn near hero | sure | Spawn side and pick use the local hero and RNG, so wogs appear in different places. | The spawn (which wog, side, velocity), or one game spawns. | | scenes/Shadow_13.fsm.txt:2559 |
| Shadow_13.4 | Solid Land Range ×6 | FSM Control 19e01a6f26 | Sends SOLID LAND when enemy in range | no | Local collision helper. | | | scenes/Shadow_13.fsm.txt:2835 |

## Shadow_14

no new elements

## Shadow_15

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_15.1 | Fake Bench Collapser | FSM Control 741b96d8e4 | Sitting on fake bench collapses floor, drops hero | sure | Persistent trap and forced fall: floor breaks and the sitter falls with the bench; the partner's floor stays. | The FAKE BENCH SIT trigger; each game collapses its floor. | Drop the partner too if standing on it, or only the sitter. | scenes/Shadow_15.fsm.txt:197 |
| Shadow_15.2 | Fall Bench | FSM Splash 807d178b83 | Splash effect when bench falls to water | no | Visual. | | | scenes/Shadow_15.fsm.txt:259 |
| Shadow_15.3 | RestBench | C# RestBenchTilt | Holds tilt value for bench | no | Visual. | | | scenes/Shadow_15.objects.txt:215 |

## Shadow_16

no new elements

## Shadow_18

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_18.1 | Boss Summon Crew | FSM Control c57d3f3e39 | On SUMMON picks random muckmen and spawn X | sure | Random type and positions per game, so adds differ. | One game decides the summon (type, X) and share the adds. | | scenes/Shadow_18.fsm.txt:50 |
| Shadow_18.2 | stake_trap_swing_repeater | FSM Control 4d460df2b9 | Boss-wave spike ball swings across arena | sure | Swing timing and damage depend on when each game sends TRAP. | The TRAP and TRAP DOUBLE events from the wave. | | scenes/Shadow_18.fsm.txt:2487 |
| Shadow_18.3 | Wave 1 | FSM FSM 015f8df7b0 | Enables bell camera lock on wave queued | sure | Camera lock holds only the hero in the game where the wave started. | Wave queued (battle start). | Lock both players' cameras. | scenes/Shadow_18.fsm.txt:2577 |
| Shadow_18.4 | Wave 3, Wave 5 | FSM Do Trap 9d51e60434 | Repeatedly sends TRAP during wave | sure | Same as Shadow_18.2; timing local. | Part of the wave state. | | scenes/Shadow_18.fsm.txt:4755 |
| Shadow_18.5 | Corpse Swamp Mosquito ×5 | FSM Do Explosion 59faa2010d | Corpse swells and explodes | maybe | Blast hazard appears in one game only; follows the death. | Follows mosquito death. | | scenes/Shadow_18.fsm.txt:5536 |
| Shadow_18.6 | Swamp Shaman | FSM Control 893c190f76 | Boss: spit, suck, dive, fireball, bomb swallow | sure | Attack choice is random and aimed at the local hero; roar stuns the hero; sets DefeatedSwampShaman. | One game decides target and attack; share boss state, HP and death. | Target nearest player or the one who triggered the bell; roar stun on both or one. | scenes/Shadow_18.fsm.txt:8450 |
| Shadow_18.7 | Swamp Shaman Suck Capture Collider | FSM Detect Grab 03bd2105ad | Grabs hero or swallows bombs in the suck zone | sure | Grab and bomb swallow read the local hero collider. | Follows the boss decision (grab applies to the targeted player). | | scenes/Shadow_18.fsm.txt:9386 |
| Shadow_18.8 | Swamp Shaman Suck Zone Object | FSM Suck Objects 16fec2eadf | Pulls suckable rigidbodies toward boss mouth | maybe | Pulls local projectiles and bombs; the hero is pulled by a separate conveyor zone. | Nothing beyond the boss attack event. | | scenes/Shadow_18.fsm.txt:9427 |
| Shadow_18.9 | Bellshrine Sequence Swamp | FSM Bell State d22e1910c5 | After battle raises lever plat and bench, saves, opens gates | sure | Reward platform and bench appear only where the battle ended; also writes a save. | Battle end (completed bool); each game plays the appear sequence. | | scenes/Shadow_18.fsm.txt:9441 |

## Shadow_19

no new elements

## Shadow_20

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_20.1 | Crying Audio Control | FSM Control 64d1bc001d, Lowpass Control e75d2f0bec | Mandrake crying audio by range | no | Audio. | | | scenes/Shadow_20.fsm.txt:234, scenes/Shadow_20.fsm.txt:268 |
| Shadow_20.2 | Exhaust Column Gloop Audio | FSM Control 4b0e72824c | Steam gloop sound | no | Audio. | | | scenes/Shadow_20.fsm.txt:356 |
| Shadow_20.3 | Exhaust Column New ×10 | FSM Control 199941c672 | Steam damager on and off per event | maybe | Damage window starts per game clock; the partner may be scalded when none is shown. | Timer phase, or let each player's own game decide. | | scenes/Shadow_20.fsm.txt:368 |
| Shadow_20.4 | Exhaust Column Damager ×65 | FSM steam_slash_effect c796671776 | Steam cut effect when damaged | no | Visual. | | | scenes/Shadow_20.fsm.txt:573 |
| Shadow_20.5 | Exhaust Column Timer | FSM Control a1166b0eeb | Cycles steam events once hero in range; stops after Phantom | maybe | Cycle starts when the local hero enters range, so phases differ and reads defeatedPhantom. | Cycle start time if steam must match. | | scenes/Shadow_20.fsm.txt:627 |
| Shadow_20.6 | Shake Region Down ×6 | FSM Shaker 1e7170f283 | Camera shake while hero in region | no | Local camera. | | | scenes/Shadow_20.fsm.txt:679 |
| Shadow_20.7 | Air Blow Region ×76 | C# AirZone | Random upward force on particles and corpses | no | Visual and corpse physics. | | | scenes/Shadow_20.objects.txt:3605 |

## Shadow_21

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_21.1 | Cage chunk ×2 | FSM Tink Move 998eca167e | Cage chunk shoved by nail tinks | no | Local physics reaction. | | | scenes/Shadow_21.fsm.txt:1522 |

## Shadow_22

no new elements

## Shadow_23

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_23.1 | Corpse Rosary Pilgrim New | C# CorpseItems | Rolls random loot on corpse and spawns pickup | sure | Loot is rolled per game, so the two games can show different or no pickups for the same kill. | The rolled loot (or which game rolls) and the pickup taken. | Loot to first picker, both players, or only the killer. | scenes/Shadow_23.objects.txt:376 |

## Shadow_24

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_24.1 | Mapper Master NPC | FSM Dialogue c53dc48bca | Needolin duet quest, reward item, saves, NPC leaves | sure | Needs the hero to play the needolin, ends quest, grants item, sets shakraAidForumBattle and disablePause, then the NPC disappears. | Quest completion and the NPC leaving; each game plays the dismissal. | Reward to the performer or both; quest shared or per player. | scenes/Shadow_24.fsm.txt:117 |

## Shadow_25

no new elements

## Shadow_26

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_26.1 | gloom_lift_destroy | FSM FSM cde449d330 | On PLAT DESTROY removes lift and boulder, persists | sure | One-time persistent break of a lift that players ride; breaks only where triggered. | The PLAT DESTROY trigger; each game plays the break. | | scenes/Shadow_26.fsm.txt:4 |

## Shadow_27

no new elements

## Shadow_28

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_28.1 | Flea Rescue Scared | FSM Control b8751a3b06 | After battle, hero near flea frees it, grants flea | sure | Flea is rescued once per scene bool; the partner could rescue it again or miss it. | The rescue (flea freed and item granted once). | Flea credited to the rescuer, both, or shared count. | scenes/Shadow_28.fsm.txt:1504 |

## Shadow_Bilehaven_Room

no new elements

## Shadow_Weavehome

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Shadow_Weavehome.1 | Auto Slide Gate (1) | FSM Control e4d5e9cbb0 | Slides open when an unlocked hero stands in front | maybe | Opens from the local hero detector only; the persistent unlock must be seen by the partner. | The persistent unlock bool. | | scenes/Shadow_Weavehome.fsm.txt:28 |
| Shadow_Weavehome.2 | Swamp Zone ×2 | C# SwampZone | Marks swamp area for area tint | no | Visual tint only. | | | scenes/Shadow_Weavehome.objects.txt:841 |

## Summary

7.1. Events that should pull the partner to the player who triggered them (forced control, arena locks, one-time cutscenes):

- Dust_01.12 Lace meeting (Dust_01): takes hero control, locks the camera, runs dialogue, writes laceLeftDocks and encounteredLaceBlastedBridge and saves; the other player would see an NPC appear and vanish, or get a second meeting.
- Dust_Chef.8 and .9 gong, Dust_Chef.6 Roachkeeper Chef boss: the gong strike with the hero inside starts the battle, gate and camera lock; the partner must be inside the arena.
- Dust_03.1 and .2 Dustpens alarm battle: the arena gates close only where the alarm was raised.
- Shadow_18.3, .6, .9 Swamp Shaman bell battle: camera lock, roar stun and gates are local; the boss reads one hero.
- Shadow_15.1 Fake bench collapse: sitting drops the sitter down the shaft and breaks the floor for good; the partner on the floor needs the same fall.
- Dust_Maze_01.10 and Dust_09.3 Mist maze: mist touch sends one hero to the entrance and resets shared counters.

7.2. Maze state is the largest single risk in Dust:

- MazeController (Dust_09.3, Dust_Maze_01.1) rolls door links with UnityEngine.Random and the trap, wraith and silkfly layout with a per-scene seed, and keeps progress in PlayerData counters. Two games diverge in door targets and counters; if the players take different doors they end up in different scenes. Needed: one shared seed and counters, and a rule for who picks the door (the other follows).
- MazeMistZone resets those counters when one player touches mist.

7.3. RNG and hero targeting that diverge unless one game decides:

- Enemy and boss target and attack choice: Dust_01.1, .3, Dust_05.1, Dust_Chef.3, .6, Dust_Maze_01.6, Dust_Maze_02.4, Shadow_01.1, Shadow_03.4, Shadow_04.1, Shadow_11.3, Shadow_12.1, Shadow_13.2, .3, Shadow_18.6, .7.
- Random scene content: Shadow_18.1 (boss crew), Shadow_13.3 (pollywog spawns), Shadow_23.1 (corpse loot).
- Grab effects (Dust_01.4, Dust_Maze_01.8, Shadow_18.7, Shadow_13.2): only the local hero collider can be grabbed, so a grab by the other game's enemy needs to land on the targeted player.

7.4. Shared-world events to keep in step (one trigger, then each game replays):

- Wire trips: Shadow_03.8 (TRAP) drives Shadow_03.1, Shadow_10.1 and in the same family Shadow_18.2/.4.
- Platform and break triggers: Dust_02.6 (lever platform), Dust_Maze_01.2 and Shadow_05.2 (cage drop), Shadow_26.1 (lift break), Dust_10.1 (bell bench repair), Dust_02.5 (prince cell door), Dust_12.1 (wall break raises alarm), Shadow_11.1 (spike ball roll).
- Alarm: Dust_01.2, Dust_03.1 and .4 share the RAISE ALARM event; replaying it in both games wakes both sets of roaches.

7.5. Per-save progression where the user must pick shared vs per-player:

- Quests and shops: Dust_04.2 courier, Dust_Shack.2 traders (Roach Killing, pins), Dust_11.2 grub farmer, Dust_11.6 mimic trade, Shadow_24.1 mapper, Shadow_28.1 flea.
- Items: Dust_06.2 cage key, Shadow_23.1 corpse loot.
- Act 3: Dust_11.2, Dust_11 farm and Dust_01 Lace both read blackThreadWorld; the flag must match on both sides.

7.6. Gaps and cautions:

- Dust_01.12 and the Lace NPC FSM are only partially in this catalog (the NPC FSM itself is catalogued elsewhere); I read it from the dump to judge the cutscene.
- Dust_01.9 and Shadow_11.1 receive BREAK and ROLL from senders I could not locate in the dump; verdicts assume the sender is shared or a persistent object.
- Dust_06.1 only checks the key at scene load; the live-open case needs a decision.
- Dust_02.6 MovePlatform: which lever drives it was inferred from a nearby Lever, not read from the link.
- New notes written in audit/notes/: MazeController, MazeMistZone, MovePlatform, TickedPendulum, CorpseItems, AirZone, RecordDoorEntry.
