# Audit 2.15: Under and Aqueduct (Under_01 .. Under_27, Aqueduct_01 .. Aqueduct_08)

Slice: catalog/Under.md (Underworks) and catalog/Aqueduct.md (Putrified Ducts). Verdict means: does coop need to handle it. Enemy rows are `sure` when alert, chase, target or attack choice reads only the local hero or uses a random roll; where a row says "one game decides", the other game replays enemy state, HP and death. Timed hazards (grinders, steam vents, lava, grind platforms) run their own loop in each game and only hurt the local hero: they are `maybe` or `no` unless they kill enemies or move a platform the partner stands on. Components of the Fleatopia flea games are `maybe`: they only matter if the partner is meant to watch a running game. New shared notes in `audit/notes/`: HeroTreadmill, RecoilEnemiesToRadius, CorpseSplatter, SpikeCogFall, TagDamager, LechPusherWall, OverrideExtraRestZone.

## Under_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_01.1 | Understore_lift | FSM Control 42a9ae8555 | Boarding lift, lever-driven ride, breaks, drops hero | sure | An enter trigger on the local hero parents him to the lift, then three lever hits end in a blackout, critical damage, teleport to the room below, SaveGame and PD understoreLiftBroke; the other player is untouched and the lift state differs. | The boarding (who rides) and the final understoreLiftBroke state; the ride, shakes and wreckage then play natively in each game. | Pull the partner onto the lift and drop both together, let each ride alone (second rider gets the same blackout), or skip the ride for a hero who arrives after the break. | scenes/Under_01.fsm.txt:1503 |
| Under_01.2 | lever | FSM Control ad802fcf70 | Nail-hit lever, random side, breaks on third use | sure | Only the local nail hit counts; the random side and the break happen in one game, so the partner's lift never advances. | The lever hit (side included) when both players ride; the lift then reacts in both games. | Whose lever hit counts: either player, or only the rider. | scenes/Under_01.fsm.txt:1680 |
| Under_01.3 | Understore_lift | FSM Camera Control ef30a93101 | Free camera follows lift during drop | no | Camera of the local hero only, driven by lift events. | | | scenes/Under_01.fsm.txt:1465 |
| Under_01.4 | Understore Lift Grate | FSM Open 91b710b540 | Slides two grates open at the lift top | no | Pure visual started by the lift ride (Under_01.1). | | | scenes/Under_01.fsm.txt:1445 |

## Under_01b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_01b.1 | Understore Toll Bench ×2 | FSM Behaviour (special) e34e96bfb8 | Pay toll to flip into a timed bench | sure | The toll payment, the flip animation and the timer back to toll run only in the paying game; the partner cannot rest on a bench that stays a toll box in their game. | The paid toll (bench becomes usable) and the flip back; resting itself stays per player. | Payer only pays and partner uses it free, partner pays too, or each pays own toll. | scenes/Under_01b.fsm.txt:4 |

## Under_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_02.1 | Barrel Plat Lift DROPPER | FSM Control 357e928d8e | Lift rises, then falls and slams, crusher hurts | sure | Started by ACTIVATE or OPEN from a lever and persisted; the slam crusher hurts hero and enemies, and the lift sits at a different height in each game. | The trigger that starts the lift; the move and slam then replay natively. | | scenes/Under_02.fsm.txt:521 |
| Under_02.2 | Barrel Plat Lift DROPPER | FSM Floor breaker da4d9b0a71 | At a height, breaks chain and sends FLOOR BREAK | sure | Starts the dock floor smash; follows Under_02.1 so no extra event. | Nothing extra; follows the lift start. | | scenes/Under_02.fsm.txt:508 |
| Under_02.3 | Dock Smash Plats | FSM Floor Break ea934f048e | Swaps intact plats for smashed ones | sure | Removes floor the partner may stand on if only one game ran FLOOR BREAK. | Nothing extra; follows Under_02.1. | | scenes/Under_02.fsm.txt:1309 |
| Under_02.4 | Collapser Small Understore Grate (1) ×4 | FSM Control dd082e4b89 | Grate collapses when hero lands on it | sure | Breaks 0.8 s after a collision with the local hero and is persisted; the partner walks on a grate that is already gone in the other game. | The collapse start (touch by either player); the break plays natively. | | scenes/Under_02.fsm.txt:1273 |
| Under_02.5 | lift_top_cog_brace ×4 | FSM Control 1c0261e722 | Spins cogs while lift moves | no | Visual follower of lift movement. | | | scenes/Under_02.fsm.txt:1326 |

## Under_03

no new elements

## Under_03b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_03b.1 | Grind Plat Control (1) | FSM Control 06f1383acd | Drops grind plats, side chosen by hero position | maybe | Which plats drop first depends on the local hero's side and the loop is on its own timer, so the two games drift. | Nothing if the plats only hurt the local hero; otherwise the cycle start. | One game drives the timing for both, or each game keeps its own. | scenes/Under_03b.fsm.txt:4 |
| Under_03b.2 | Understore Grind Plat ×3 | FSM Control beba5a5f11 | Plat drops, grinds, becomes solid then sinks | maybe | Random sprite choice and a damager that turns into a standing platform; desync puts the partner's avatar on air or on a damager. | Covered by Under_03b.1 if one game drives timing. | | scenes/Under_03b.fsm.txt:31 |
| Under_03b.3 | Pilgrim 03 Understore (1) ×9 | FSM Attack b13c3d5ea5 | Wakes near hero, leaping skid attack | sure | Wake range, facing and attack start read the local hero only. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Under_03b.fsm.txt:181 |

## Under_03c

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_03c.1 | Understore_Grinder ×15 | FSM Control 08f0579688 | Crusher moves through a fixed direction loop | maybe | Deterministic loop on its own clock, so grinders sit at different places in each game and drag-kill enemies at different times; damages only the local hero. | Nothing if only the hero is hurt; otherwise a shared start time per grinder. | Driven by one game for both, or independent. | scenes/Under_03c.fsm.txt:327 |
| Under_03c.2 | Understore_Grinder (3) and (5) | FSM Set 5c06175740, 6bc44c6aee | Sets Min Y and Max Y limits | no | Static configuration. | | | scenes/Under_03c.fsm.txt:323, scenes/Under_03c.fsm.txt:526 |
| Under_03c.3 | Understore_Grinder ×15 | FSM Region Volume Control 2afb13098d | Switches grinder audio volume | no | Audio only. | | | scenes/Under_03c.fsm.txt:495 |
| Under_03c.4 | Hero Damager ×15 | FSM Multihitter 05e6ec22db | Hurts hero in grinder, repeated hits | no | Damages only the local hero, correct per game; the partner avatar takes no damage. | | | scenes/Under_03c.fsm.txt:505 |
| Under_03c.5 | Enemy Sucker ×15 | C# RecoilEnemiesToRadius | Pulls enemies into the grinder | maybe | Moves enemies by physics in each game; an enemy dragged and killed in one game only desyncs enemy HP and death. | Covered by enemy state sync. | | scenes/Under_03c.objects.txt:914 |
| Under_03c.6 | Corpse Splatter ×15 | C# CorpseSplatter | Removes corpses touching grinder | no | Cosmetic corpse cleanup. | | | scenes/Under_03c.objects.txt:915 |

## Under_03d

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_03d.1 | Hero Treadmill | C# HeroTreadmill, ConveyorBelt | Running hero drives belt and needle gauge | sure | The belt captures only the local hero, so the needle only rises for the runner; the payout counter (PersistentIntItem) is shared state. | The payout (see Under_03d.2); belt and needle stay local. | Only the runner collects, or both can run for one pool. | scenes/Under_03d.objects.txt:896, scenes/Under_03d.objects.txt:910 |
| Under_03d.2 | needle behaviour | FSM FSM 8ed7f0ac0e | Needle high: fair-random spit of a rosary | sure | Random fair roll plus a persisted paid-out count; both games can spit the same capped rosaries twice. | The spit event with the paid-out count; the rosary pickup then drops in both. | Rosary goes to the runner only or is shared. | scenes/Under_03d.fsm.txt:376 |
| Under_03d.3 | Rosary Limiter | C# CurrencyObjectLimitRegion | Caps rosary object count on low graphics | no | Graphics tier limit, no gameplay. | | | scenes/Under_03d.objects.txt:909 |
| Under_03d.4 | Understore Large Worker | FSM Behaviour 0b7b2c8a31 | Works, rests, sings, talks (dialogue counter) | sure | NPC dialogue progress is a persisted int (PersistentIntItem) read at the talking hero's interact; singing reacts to the local needolin. | The dialogue counter after a talk; the partner's NPC then continues from the same line. | | scenes/Under_03d.fsm.txt:462 |
| Under_03d.5 | Understore Large Worker | FSM Fade Work Loop on Speech 5091687c61 | Fades work audio during speech | no | Audio only. | | | scenes/Under_03d.fsm.txt:582 |
| Under_03d.6 | Audio Loop Voice | FSM Position Control 4f99c1bc04 | Moves voice source by hero range | no | Audio only. | | | scenes/Under_03d.fsm.txt:591 |

## Under_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_04.1 | dock_metal_grate_floor_set (1) | FSM Custom Shake 448991fc6d | Shakes grate when something stands on it | no | Visual jitter. | | | scenes/Under_04.fsm.txt:270 |
| Under_04.2 | Pilgrim Staff Understore ×6 | FSM Attack 096cd50e39 | Random stab or leap attack facing hero | sure | Attack choice is random and faces the local hero. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Under_04.fsm.txt:1124 |
| Under_04.3 | Pilgrim Staff Understore | FSM Shift 165aab750f | Moves enemy if the room was entered by gate top1 | maybe | Start position depends on the entry gate of the local hero, so the staff stands in different places per game. | Covered by enemy state sync. | | scenes/Under_04.fsm.txt:1111 |
| Under_04.4 | Staff ×6 | C# PlayAnimationInAir | Plays animation while corpse is airborne | no | Cosmetic corpse effect. | | | scenes/Under_04.objects.txt:1072 |
| Under_04.5 | Grinder Detector ×29 | C# AnimatorSpeedInRange | Speeds junk animators near grinder | no | Visual. | | | scenes/Under_04.objects.txt:1137 |

## Under_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_05.1 | cog_05_shortcut | FSM Cog Shortcut Break aa8d73f44a | Cogs fall and open a shortcut, persisted | sure | The break is persisted but replays only in the game that triggered it; the partner's shortcut stays blocked until reload. | The break trigger; fall, shakes and opened shortcut then play natively. | | scenes/Under_05.fsm.txt:1047 |
| Under_05.2 | Spike Cog ×15 | C# SpikeCogFall | Falling spike cogs, random bounce | no | Hurts the local hero only; random bounce is cosmetic after the break event. | | | scenes/Under_05.objects.txt:1763 |

## Under_06

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_06.1 | Grind Plat Control | FSM Control 0e3d4ae8ec | Drop pattern R or L by hero side | maybe | Pattern picked by the local hero's side range; timings differ per game, so the partner's plat may be missing. | Nothing if plats only hurt the local hero; otherwise the pattern start. | One game drives it or each its own. | scenes/Under_06.fsm.txt:52 |
| Under_06.2 | junk_chute_ manual ×6 | FSM Control 4771bafe85 | Timed spike flow in junk chute | maybe | Own timer per game; spike collider position differs. | Covered by Under_06.1 timing. | | scenes/Under_06.fsm.txt:94 |
| Under_06.3 | Spike Collider ×6 | FSM Shift Hero fcda0a590f | Pushes hero down 1.75 when hit | no | Moves only the local hero. | | | scenes/Under_06.fsm.txt:117 |
| Under_06.4 | Attack Detector ×6 | FSM Control ab35ca3856 | Shakes junk pipe when something enters | no | Visual shake. | | | scenes/Under_06.fsm.txt:1166 |

## Under_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_07.1 | steam_vent_short (1) and (2) | FSM Control 91a8f36b1f | Battle-gate steam vents: close and open with arena | maybe | Timed hazard that starts closed and follows BG CLOSE or OPEN, so it needs the arena start and end to reach the partner; damager hurts only the local hero. | Covered by the battle scene start and end. | | scenes/Under_07.fsm.txt:19 |
| Under_07.2 | steam_vent_short ×3 | FSM Control 5362bd4344 | Timed steam blast with damager and air push | no | Own timer per game, hurts and pushes only the local hero. | | | scenes/Under_07.fsm.txt:3624 |

## Under_07b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_07b.1 | Soft Waterfall Region ×2 | FSM Cut Effect 418e7a9b31, Hero Effects d94efa64dc | Water cut and splash effects | no | Visual only. | | | scenes/Under_07b.fsm.txt:163, scenes/Under_07b.fsm.txt:180 |

## Under_07c

no new elements

## Under_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_08.1 | Confessional NPC | FSM Dialogue 0dc8dc9961 | Pay toll, Hornet silence, dialogue, may break | sure | Toll payment through the shared template, then a persisted Broke flag changes the dialogue; one game pays and breaks it while the other still shows it intact. | The toll payment and the broken flag. | Payer pays alone, or the toll is shared. | scenes/Under_08.fsm.txt:210 |
| Under_08.2 | Disable Sound Trigger | FSM FSM 950df37d72 | Marks secret sound region as heard | no | Ambient sound flag only. | | | scenes/Under_08.fsm.txt:252 |
| Under_08.3 | glow_bugs | C# SpeedRampAnimator | Ramps lamp bug animation speed | no | Visual. | | | scenes/Under_08.objects.txt:1748 |

## Under_10

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_10.1 | Battle Floor | FSM Break 75e7cf73d1 | Wave-7 floor breaks and falls | sure | Part of the battle: removes the floor in the game that ran wave 7 only. | Nothing extra if the arena waves are shared; otherwise the FLOOR BREAKER step. | | scenes/Under_10.fsm.txt:4 |
| Under_10.2 | Pipe_Vent_Hatch_BG | FSM BG Control abb1bb5040 | Locks hatch levers while arena is closed | sure | Arena gate: locks and unlocks joined levers on BG CLOSE and OPEN, so the partner's hatch stays locked or unlocked wrongly. | Covered by arena start and end. | See Summary: pull the partner into the arena or not. | scenes/Under_10.fsm.txt:104 |

## Under_11

no new elements

## Under_12

no new elements

## Under_13

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_13.1 | CameraLockArea Bridge Pass | FSM Appearance a7f0f3c879 | Disables camera lock for some entry gates | no | Local camera only. | | | scenes/Under_13.fsm.txt:25 |
| Under_13.2 | Understore Automaton ×17 | FSM Control 523613e077 | Flier: idles, slashes toward hero, sings, respawns | sure | Attack range, fire-at-hero and sing read the local hero; some respawn after death. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Under_13.fsm.txt:2052 |
| Under_13.3 | Understore Automaton EX ×2 | FSM Control 82dfe008cb | Chases hero then explodes, ambush or battle | sure | Alert, chase target and explosion read the local hero; also started by BATTLE or AMBUSH. | One game decides target; share enemy state, HP and death (explosion is replayed). | Target nearest player, or only the one who woke it. | scenes/Under_13.fsm.txt:2972 |
| Under_13.4 | Understore Automaton EX ×2 | FSM Recoil Bounce 151fa30b8b | Clamps velocity after being hit | no | Local knockback detail, covered by enemy state. | | | scenes/Under_13.fsm.txt:3152 |
| Under_13.5 | Bridge Backings | C# AnimatorPlayDelaySequence | Plays bridge backing animators in sequence | no | Visual. | | | scenes/Under_13.objects.txt:907 |

## Under_14

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_14.1 | Grind Plat Control New | FSM Control f499c84bb4 | Drops three grind plats in a fixed loop | maybe | Fixed 3 s loop with no hero check, but independent clocks per game. | Nothing if plats only hurt the local hero. | One game drives it or each its own. | scenes/Under_14.fsm.txt:242 |

## Under_16

no new elements

## Under_17

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_17.1 | Architect NPC | FSM Behaviour 65bec87c3d | Chair NPC: meet, shop, quest and tool dialogue | sure | Sets many PD flags (MetArchitect, ArchitectWillLeave, melody and crest flags) and reads quest and item state of the talking hero; random top or bottom start; shop spends rosaries; camlock on shop open. | The first meeting and PD flag changes; the shop as per-player stock and purchases. | Shop session private to the buyer or shared stock; shop cam lock only for the buyer. | scenes/Under_17.fsm.txt:96 |
| Under_17.2 | Architect Shrine Door | FSM FSM c795bf18fb | Locks door after Toolmaster crest, SaveGame | sure | Door locks permanently and saves once the crest is owned; the state follows the crest of the local hero. | The lock state when either player takes the crest. | Crest per player or shared. | scenes/Under_17.fsm.txt:401 |
| Under_17.3 | hangar_golem_head ×2 | FSM Needolin Reaction 0a79337971 | Golem glows and sings to needolin | no | Visual and audio reaction to the local needolin. | | | scenes/Under_17.fsm.txt:3457 |

## Under_18

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_18.1 | Shrine Weaver HarpoonDash | FSM Inspection 0e31bb8cce | Clawline shrine: lock hero, dialogue, bind, grant ability | sure | Cutscene with input locks and camlocks, sets PD hasHarpoonDash and starts the automaton ambush and hides lava plats; only the interacting hero gets the ability. | The ability grant (PD hasHarpoonDash) and the shrine used state. | Pull the partner in and freeze them, leave them free, or grant the ability to both. | scenes/Under_18.fsm.txt:981 |
| Under_18.2 | Battle Scene | FSM Start Control 2de6406103 | Starts battle on OPEN | sure | Started when the local hero pulls the ring switch; gates and camlocks lock only that hero. | The battle start; waves, gates and end replay natively. | Pull the partner in, or let them stay outside. | scenes/Under_18.fsm.txt:1278 |
| Under_18.3 | Battle Start Sequence | FSM Start Control 4a27ccaf92 | Closes gates, lights lamp, begins battle | sure | Same arena start as Under_18.2 with gate close and darkness region. | Covered by Under_18.2. | | scenes/Under_18.fsm.txt:1290 |
| Under_18.4 | Battle Gate Understore | FSM BG Control eda1ffa8c6 | Closes and opens the arena gate | sure | Locks the local hero in; the partner must end up inside or the gate must not block them. | Covered by Under_18.2 and the battle end. | | scenes/Under_18.fsm.txt:1324 |
| Under_18.5 | Pipe_Vent_Hatch (1) | FSM Battle Gate Control bd1dc9914a | Opens trapdoor at battle end | sure | Exit door of the arena; stays shut in the game that did not finish. | Covered by the battle end. | | scenes/Under_18.fsm.txt:1394 |
| Under_18.6 | Item Dropper (no longer used), Trapdoor Light | FSM Battle Gate Control 35d58f9760, 2ed0d1e974 | Unused dropper and fading light | no | Unused or visual. | | | scenes/Under_18.fsm.txt:1383, scenes/Under_18.fsm.txt:1403 |

## Under_19

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_19.1 | Lava_Waterfall Set (3) to (7) ×8 | FSM Control 7f50ac0db8, f0fa6fbf54, 6e9da41b79 | Timed lava hazards when hero in range | no | Started by the local hero's range; hazards hurt only the local hero. | | | scenes/Under_19.fsm.txt:1214, scenes/Under_19.fsm.txt:1246, scenes/Under_19.fsm.txt:1330 |

## Under_19b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_19b.1 | Lava_Waterfall Set (4) to (6) | FSM Control a63a6a34be, df826f4bb2, 4b52d40b46 | Timed lava hazards when hero in range | no | Same as Under_19.1. | | | scenes/Under_19b.fsm.txt:477, scenes/Under_19b.fsm.txt:505, scenes/Under_19b.fsm.txt:537 |

## Under_19c

no new elements

## Under_20

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_20.1 | Crest_Head | FSM Jitter Message 61b38d3e8b | Jitters shrine head on animation event | no | Visual. | | | scenes/Under_20.fsm.txt:609 |
| Under_20.2 | Needolin Reaction | FSM Pulse Reaction 67f1068d77 | Shrine glows while needolin plays | no | Visual reaction to the local needolin. | | | scenes/Under_20.fsm.txt:616 |
| Under_20.3 | Death Respawn Marker | FSM FSM d5d6f91d3c | Clears shrine particles when hero respawns here | no | Cosmetic; the respawn point itself is per player. | | | scenes/Under_20.fsm.txt:693 |

## Under_21

no new elements

## Under_22

no new elements

## Under_23

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_23.1 | Understore_Grinder A Boss and 8 more | FSM set move accel b4f7922520 | Sets grinder move acceleration | maybe | Config of the boss-room grinders (same loop as Under_03c.1); independent clocks per game drag and kill the room's enemies at different times. | Covered by Under_03c.1. | | scenes/Under_23.fsm.txt:813 |

## Under_27

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Under_27.1 | Fall_platform_barrel | FSM Rope Plat 7d4743187e | Cut rope drops platform, sets a PD bool | sure | Persisted and falls only in the cutting game; the partner's platform stays up and its PD bool arrives late. | The vine cut; the fall, landing and acid splash then play natively. | | scenes/Under_27.fsm.txt:468 |
| Under_27.2 | Vine | FSM Vine Cut 0a85bb6a7c | Nail-hit vine from above, sends CUT | sure | Only the local nail hit cuts it. | Same event as Under_27.1 (the vine cut). | | scenes/Under_27.fsm.txt:673 |
| Under_27.3 | Gate SFX | FSM FSM cfb1d03424 | On twig wall vines cut, sends SHORTCUT OPENED | sure | The Seth shortcut opens only in the game where the vines were cut. | The twig wall cut; the shortcut then opens natively. | | scenes/Under_27.fsm.txt:2074 |
| Under_27.4 | right2 | FSM Activation be4676e0f1 | Enables gate collider if sethShortcut PD is true | maybe | Reads the PD flag only at load, so a shortcut opened live by the partner stays closed here. | Covered by Under_27.3. | | scenes/Under_27.fsm.txt:1969 |
| Under_27.5 | Remasker ×2 | FSM unmasker 01174c55db | Hides or reveals mask when hero enters | no | Visual reveal for the local hero. | | | scenes/Under_27.fsm.txt:2178 |

## Aqueduct_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Aqueduct_01.1 | Swamp Barnacle ×22 | FSM Control 7de1dae096 | Tendril grabs hero or items, chews, eats | sure | Grab is triggered by the local hero (or tagged items like bombs and mosquitoes); it takes hero control, pulls the camera, chews for damage, can kill and sets PD hasKilled. | The grab and release of a captured player and the barnacle state; the held victim then replays in the other game. | Partner can free the victim by hitting the tendril, or not. | scenes/Aqueduct_01.fsm.txt:426 |
| Aqueduct_01.2 | Tendril ×22 | FSM Detect Grab 6de05c860c | Checks hero or item entering tendril | sure | Reads only the local hero (CanBeBarnacleGrabbed); the partner avatar is never captured. | Same as Aqueduct_01.1. | | scenes/Aqueduct_01.fsm.txt:1168 |
| Aqueduct_01.3 | Attack Detector ×22 | FSM Detect Attack 6d48693d16 | Nail hit on tendril makes barnacle frightened | maybe | Only the local hero's attack frightens the barnacle and so releases the grab; a partner hit is not seen. | The tendril hit, if rescue is allowed. | See Aqueduct_01.1. | scenes/Aqueduct_01.fsm.txt:1274 |
| Aqueduct_01.4 | Tendril ×22 | FSM Set Colliders e0f09056e4 | Sizes tendril colliders at start | no | Static setup. | | | scenes/Aqueduct_01.fsm.txt:1258 |
| Aqueduct_01.5 | Tendril ×22 | C# CustomPlayMakerTriggerStay2D | Relays trigger-stay events to FSM | no | Plumbing for Aqueduct_01.2. | | | scenes/Aqueduct_01.objects.txt:1689 |
| Aqueduct_01.6 | Dust Damager ×22 | C# TagDamager | Adds dust damage tag to enemies in trigger | maybe | Damages enemy HP over time after a dust-bomb chomp; only matters if enemy HP is local. | Covered by enemy HP sync. | | scenes/Aqueduct_01.objects.txt:1736 |
| Aqueduct_01.7 | Tendril Target ×12 | C# CustomTag | Marks mosquito as edible by barnacle | maybe | A barnacle eating a mosquito in one game only removes it there. | Covered by enemy death sync. | | scenes/Aqueduct_01.objects.txt:2196 |
| Aqueduct_01.8 | Swamp Ductsucker ×3 | FSM Control 355e166b35 | Patrols, roars, charges, jumps, digs away | sure | Alert, facing, charge and jump read the local hero; random side, charge count and jump roll; persisted kill. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Aqueduct_01.fsm.txt:4831 |
| Aqueduct_01.9 | Swamp Ductsucker ×3 | FSM Detect Water Entry 32513ebce1 | Sends SPLASH IN on water contact | no | Splash visual trigger. | | | scenes/Aqueduct_01.fsm.txt:4821 |
| Aqueduct_01.10 | Swamp Mosquito Skinny ×12 | FSM Control 653b1b2b9f | Flies, clings to walls, wakes on hero | sure | Alert range and startle read the local hero; random dart direction and speed. | One game decides target and attack; share enemy state, HP and death. | Target nearest player, or only the one who woke it. | scenes/Aqueduct_01.fsm.txt:5862 |
| Aqueduct_01.11 | Swamp Mosquito Skinny ×12 | FSM Shoot 9b1285076a | Fires three aimed spits at hero | sure | Aim angle targets the local hero with random spread and speed. | Covered by Aqueduct_01.10 (the shot is replayed from the owning game). | | scenes/Aqueduct_01.fsm.txt:5817 |
| Aqueduct_01.12 | Ankle Drip Region ×11 | FSM Control 7246dc73c8 | Drip effect when leaving region | no | Visual. | | | scenes/Aqueduct_01.fsm.txt:4 |

## Aqueduct_02

no new elements

## Aqueduct_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Aqueduct_03.1 | Breakable Wall | C# PlayMakerArrayListProxy | PlayMaker list holder on a hit-count wall | maybe | The wall (persisted, breaks after several hits) is a path blocker; a break in one game leaves it standing in the other. | The wall break; masks, effects and interactible then play natively. | | scenes/Aqueduct_03.objects.txt:951 |

## Aqueduct_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Aqueduct_04.1 | Swamp Barnacle Slab Fly | FSM FSM 17391d760c | Plays hit effect and animation when damaged | no | Decorative reaction. | | | scenes/Aqueduct_04.fsm.txt:3656 |

## Aqueduct_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Aqueduct_05.1 | Caravan_States | FSM Conditions ee0adbf353 | Picks pre, Fleatopia or festival set from PD | sure | Chooses the whole scene content from PD (CaravanTroupeLocation, FleaGamesStarted, FleaGamesCanStart); a player who enters before the partner's PD arrives sees another state. | The PD flags before either player enters; the festival intro then runs natively. | | scenes/Aqueduct_05.fsm.txt:129 |
| Aqueduct_05.2 | Mr Mushroom Tablet | FSM Inspection 72f4f21d97 | Riddle tablet, hero anim, asks to begin quest | sure | Starts the Mr Mushroom quest and locks the reading hero; quest state is per save. | The quest acceptance. | Quest per player or shared. | scenes/Aqueduct_05.fsm.txt:313 |
| Aqueduct_05.3 | Lit Parent, Weaver Prompt BG | FSM Tablet Control b7c5613ff1, Control 8768271c10 | Glow and prompt fades by hero distance | no | Visual by local hero distance. | | | scenes/Aqueduct_05.fsm.txt:372, scenes/Aqueduct_05.fsm.txt:399 |
| Aqueduct_05.4 | Fungus Mushroom 03 ×7 | C# TouchShake | Shakes sprite when hero touches | no | Visual. | | | scenes/Aqueduct_05.objects.txt:301 |
| Aqueduct_05.5 | Lech Wall | C# LechPusherWall | Pushes rigidbodies colliding with the wall | no | Physics nudge for the lech enemy, no persistence. | | | scenes/Aqueduct_05.objects.txt:1505 |

## Aqueduct_05_caravan

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Aqueduct_05_caravan.1 | Caravan Troupe Leader Fleatopia NPC | FSM Dialogue 3a0087667b | Meet, flea rescue and courier delivery reward | sure | Reads quest, flea and item state of the talking hero; turns in the delivery quest (ConsumeCurrency) and gives a reward. | The quest turn in and the reward; PD meet flags. | Quest and reward to the turn-in player or both. | scenes/Aqueduct_05_caravan.fsm.txt:784 |

## Aqueduct_05_festival

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Aqueduct_05_festival.1 | Caravan_States | FSM Conditions db1099580c | Festival intro, begin, locks hero, saves state | sure | The intro takes control of the local hero, looks up, locks camera and area title; the begin is gated on PD FleaGamesStarted. | The festival begin (PD FleaGamesStarted); the intro effects replay natively. | Pull the partner into the intro, freeze them, or leave them free. | scenes/Aqueduct_05_festival.fsm.txt:1 |
| Aqueduct_05_festival.2 | Caravan Troupe Leader Festival Intro | FSM Dialogue 69d889b7ce | Offers Flea Games quest, starts festival | sure | Quest offer (Flea Games Pre / Flea Games) and the start of the festival depend on the talking hero. | The quest acceptance and the festival start. | Quest per player or shared. | scenes/Aqueduct_05_festival.fsm.txt:21121 |
| Aqueduct_05_festival.3 | Flea Games Host NPC ×3 | FSM Control ac31a066d5 | Intro, yes or no, starts game, high scores, outro | sure | Writes PD (Played, Champ, FleaGamesEnded, FleaGamesOutroReady), disablePause, saves, and runs the outro with relinquished control. | Game start, final score and champion flag; the outro as a cutscene event. | One player plays at a time with the partner as spectator, or both may play separately. | scenes/Aqueduct_05_festival.fsm.txt:4841 |
| Aqueduct_05_festival.4 | Bouncing, Dodging, Juggling | FSM flea_game_master_control cd4fde9ba4 | Takes hero, resets silk, runs game, saves high score | sure | Teleports the playing hero to the start, sets silk, pauses off, and saves the highscore PD; the partner is not moved. | The game start and end with the score (high score PD). | Freeze or pull the partner, or leave them free. | scenes/Aqueduct_05_festival.fsm.txt:1623 |
| Aqueduct_05_festival.5 | Flea Game - Bouncing, Dodging, Juggling | FSM Game Specific Control 7dde015a80, c200b3ce3d, 0eacc24ae8 | Random flea spawns, difficulty, score | maybe | Random flea pattern drawn per game, so the partner sees a different game unless spectating is off. | A shared random seed with the start, only if spectating is wanted. | Spectate or not. | scenes/Aqueduct_05_festival.fsm.txt:1482, scenes/Aqueduct_05_festival.fsm.txt:6028, scenes/Aqueduct_05_festival.fsm.txt:11642 |
| Aqueduct_05_festival.6 | Bellflea Bouncer ×12 | FSM Control 80f7ba96b0, Wave Control d62ca64b7d | Bouncing fleas fly in waves | maybe | Game object of the local game; random heights. | Covered by Aqueduct_05_festival.5. | | scenes/Aqueduct_05_festival.fsm.txt:1913, scenes/Aqueduct_05_festival.fsm.txt:2120 |
| Aqueduct_05_festival.7 | Tink Collider ×36 | FSM Harpoon Tink c9a8ca790f, ee39f05cdf, 647c3b0210 | Reacts to harpoon hit on fleas | maybe | Reaction to the local hero's harpoon only. | Covered by Aqueduct_05_festival.5. | | scenes/Aqueduct_05_festival.fsm.txt:2141, scenes/Aqueduct_05_festival.fsm.txt:5983, scenes/Aqueduct_05_festival.fsm.txt:12392 |
| Aqueduct_05_festival.8 | Game Start Fleas ×21, Kratt BounceGame | FSM Control b7702da568, acd3ff35fa | Intro fleas fly in and leave; Kratt enters | no | Staging visuals before the game. | | | scenes/Aqueduct_05_festival.fsm.txt:5172, scenes/Aqueduct_05_festival.fsm.txt:5907 |
| Aqueduct_05_festival.9 | Dodging game | FSM Start Timer 406698c946, Fatigue Control c4ec584b9d | Start delay; counts bonks for fatigue | maybe | Reads hero bonks of the local game. | Covered by Aqueduct_05_festival.5. | | scenes/Aqueduct_05_festival.fsm.txt:5995, scenes/Aqueduct_05_festival.fsm.txt:6006 |
| Aqueduct_05_festival.10 | Dodge Plat L and R, terrain collider ×4, Pos ×32 | FSM Control c17ccb2cc7, Detect Slam 45e2246ad6, FSM 26429414dc | Tilting dodge plats, slam, flea positions | maybe | Platform tilt follows the local hero standing and slamming; the partner avatar on the plat is not weighed. | Covered by Aqueduct_05_festival.5. | | scenes/Aqueduct_05_festival.fsm.txt:6435, scenes/Aqueduct_05_festival.fsm.txt:6618, scenes/Aqueduct_05_festival.fsm.txt:6815 |
| Aqueduct_05_festival.11 | Bellflea Swooper ×13 | FSM Bonk Hero fe7ec09c0e, Control ae6345de04 | Swooping flea bonks hero on contact | maybe | Knocks back and animates only the local hero; random side. | Covered by Aqueduct_05_festival.5. | | scenes/Aqueduct_05_festival.fsm.txt:7167, scenes/Aqueduct_05_festival.fsm.txt:7243 |
| Aqueduct_05_festival.12 | Flea Hunter DodgeGame, Giant Flea DodgeGame | FSM Control 69e9826cc8, 67c7ad3e20 | Hunter throws at hero; giant flea charges | maybe | Random sides and aimed throw at the local hero. | Covered by Aqueduct_05_festival.5. | | scenes/Aqueduct_05_festival.fsm.txt:10786, scenes/Aqueduct_05_festival.fsm.txt:11278 |
| Aqueduct_05_festival.13 | Bellflea Juggler ×10, Grishkin and Varga JuggleGame | FSM Control 04b3d22d57, 4250990864 | Juggling fleas drop in and bounce away | maybe | Random entry side and targets; game object of the local game. | Covered by Aqueduct_05_festival.5. | | scenes/Aqueduct_05_festival.fsm.txt:11964, scenes/Aqueduct_05_festival.fsm.txt:16995 |
| Aqueduct_05_festival.14 | Flea Festival Pinata | FSM Rewards ceb7aa3dee | Pinata hits drop item rewards, sets pinata flag | sure | Sets PD FleaGamesPinataHit and drops reward pickups after hits by the local hero. | The pinata hit; the rewards then drop natively. | Rewards to the hitter or both. | scenes/Aqueduct_05_festival.fsm.txt:227 |
| Aqueduct_05_festival.15 | Festival Outro Giant Flea, Audio Flea Voice ×4, Audio Boss Tension Pauser | FSM Control 75a43b3718, FSM 366b87a37e, 22d5b20154 | Outro fly-past and flea audio | no | Cutscene visuals and audio started by the outro (Aqueduct_05_festival.3). | | | scenes/Aqueduct_05_festival.fsm.txt:205, scenes/Aqueduct_05_festival.fsm.txt:129, scenes/Aqueduct_05_festival.fsm.txt:21113 |
| Aqueduct_05_festival.16 | Flea Brew Surprise Flea | FSM Control 07bfa1df84 | Random chance a flea pops out of the pot | no | Cosmetic random surprise. | | | scenes/Aqueduct_05_festival.fsm.txt:544 |
| Aqueduct_05_festival.17 | Refill Inspect (brew pot) | FSM FSM 6c2f931302 | Gives or refills the Flea Brew tool | maybe | Unlock and refill apply to the interacting hero's tool inventory. | Nothing if tools stay per player. | Brew per player or shared. | scenes/Aqueduct_05_festival.fsm.txt:608 |
| Aqueduct_05_festival.18 | Flea Festival Scoreboard | FSM Scoreboard c49a1e30af | Opens high score board UI | no | Read-only UI of the local PD scores. | | | scenes/Aqueduct_05_festival.fsm.txt:1451 |
| Aqueduct_05_festival.19 | Surface Water Region Playing, pontoon | FSM Fake Surface Water 8b2933c91e, Reset Pos On Enable 2396dac0ad | Fake water and pontoon reset while a game runs | no | Visual. | | | scenes/Aqueduct_05_festival.fsm.txt:1884, scenes/Aqueduct_05_festival.fsm.txt:5522 |
| Aqueduct_05_festival.20 | Caravan Troupe Flea Festival Swimmer ×2 | FSM Control cee3ba927c | Ambient flea dives, reacts to needolin | no | Ambient random behavior outside games. | | | scenes/Aqueduct_05_festival.fsm.txt:17784 |
| Aqueduct_05_festival.21 | Seth Sit NPC Fleatopia | FSM Dialogue ca87f46afe | Seth festival talk, awards memento | sure | Reads PD FleaGamesBestedSeth and gives the memento to the talking hero. | The memento award and the Seth flags. | Memento to the talker only or both. | scenes/Aqueduct_05_festival.fsm.txt:20935 |
| Aqueduct_05_festival.22 | weaver_harp_sign_behind_scoreboard | FSM Control d4aae2fd7b | Needolin-triggered harp sign, map pickup | maybe | Reacts to the local needolin and a map take flag. | Nothing if map is per player. | | scenes/Aqueduct_05_festival.fsm.txt:20969 |

## Aqueduct_05_pre

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Aqueduct_05_pre.1 | Hornet Hint Dlg | FSM FSM acd21fdf73 | One-time Hornet line about Fleatopia search | maybe | Gated by PD TroupeLeaderSpokenFleatopiaSearch; the line plays for the local hero. | The PD flag if it should not repeat for the partner. | | scenes/Aqueduct_05_pre.fsm.txt:922 |
| Aqueduct_05_pre.2 | RestBench | C# OverrideExtraRestZone | Sets scene extra rest zone for bench | no | Per-player respawn zone bookkeeping. | | | scenes/Aqueduct_05_pre.objects.txt:33 |

## Aqueduct_06

no new elements

## Aqueduct_07

no new elements

## Aqueduct_08

no new elements

## Summary

Events that should pull or hold the partner (each needs a design choice):
1. Understore lift ride (Under_01.1): hero is parented to the lift, blacked out, critically damaged, teleported and saved. Partner either rides along, rides alone later, or is left alone.
2. Clawline shrine (Under_18.1) and its harpoon-ring arena (Under_18.2 to 18.5): input and camera locks, ability grant, then locked gates. Pull the partner in or leave them outside; the arena gate must not trap or block them.
3. Under_10 arena and Under_07 vents arena: same battle gate rules, pipe hatch levers lock while closed.
4. Swamp Barnacle grab (Aqueduct_01.1): takes control of one hero, chews them to death. Partner may rescue by hitting the tendril.
5. Fleatopia festival intro, outro, host and games (Aqueduct_05_festival.1 to 5): relinquish control, teleport the player to the game arena, set PD. Partner frozen, pulled or free.

Shared state that must reach both games live (otherwise one player is blocked): Cog shortcut break (Under_05.1), collapsing grates (Under_02.4), barrel lift dropper plus dock floor smash (Under_02.1 to 2.3), vine cut and twig wall shortcut (Under_27.1 to 27.4), Aqueduct_03 breakable wall, Understore toll bench payment (Under_01b.1), treadmill rosary payout (Under_03d.1, 03d.2), confessional toll (Under_08.1), Architect shop and flags (Under_17), pinata, quest and memento turn-ins (Aqueduct_05 family).

Enemy fights needing one-game-decides handling: Pilgrim 03 and Pilgrim Staff, Understore Automaton and EX, Swamp Ductsucker, Skinny Mosquito, Swamp Barnacle (grab target). Barnacle also eats mosquitoes and bombs and dust-bomb damage tags enemies, so enemy death must be consistent.

Timed hazards (grinders, grind platforms, steam vents, lava waterfalls, junk chutes) have independent clocks per game and hurt only the local hero; the open choice is whether one game drives their timing for both. Only the grinders (Under_03c.1, 03c.5, Under_23.1) matter further because they drag and kill enemies.

Quest, memento and tool choices (Mr Mushroom, Flea Games, Courier Delivery, Seth memento, Flea Brew, Architect crest): decide whether each is per player or shared.

Gaps: the flea games (bouncing, dodging, juggling) are single-player minigames with random spawns; if the partner should spectate, a shared random seed is needed, otherwise only start, end and high score need sharing.
