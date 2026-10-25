# Audit 2.19: Abyss_*, Last_Dive, Last_Dive_Return, Cinematic_*, Meta scenes

Reading guide. Verdict: sure / maybe / no = does coop need to handle it. The Abyss is the Act 3 endgame: the White Flower route (Abyss_05 dive) leads through Last_Dive into the Lost Lace fight (Abyss_Cocoon) and straight into Ending E with no exit back to the world. Cinematic and Meta scenes run outside the world: whoever is in them has no hero in the partner's world, and several of them end the play session (return to menu, quit, Steel Soul game over).
Notes written: AbyssWaterTendrils, WorldRumbleArea, SplineDepress, StagTravel, CutsceneHelper, EndingSequence, QuitToMenu, GameCompletionScreen, SteelSoulUnlockSequence. Notes used: CheckHeroPerformanceRegion, StartRoarEmitter, TestGameObjectActivator, AlertRange, PersistentBoolItem, CameraLockArea, TransitionPoint.

## Abyss_01

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_01.1 | Abyss Crawler (2) | FSM Fall 40ab02092e | Crawler drops off and lands when ungrounded | no | Physics reaction of the crawler itself, no hero reads; follows from the crawler's own state. | | | scenes/Abyss_01.fsm.txt:4 |
| Abyss_01.2 | Plate | FSM FSM 16aa8d3bc9 | Super-jump pad plate dips on hard landing | no | Cosmetic dip and sound, no state. | | | scenes/Abyss_01.fsm.txt:25 |

## Abyss_02

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_02.1 | Gloomfly Cocoon L, Gloomfly Cocoon S ×2 | FSM Control 2f5153a650, Control e9b5d482c9 | Egg hatches near hero, flings 1-2 Gloomflies | sure | Hatch fires on the local hero's alert range (or a hit breaking the egg), so the flies spawn only in that game and the partner's egg stays intact. | The cocoon break; each game then flings its own Gloomflies. | | scenes/Abyss_02.fsm.txt:25 |

## Abyss_02b

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_02b.1 | Gloom Beast ×2 | FSM Control 8bcc26cbc4 | 200hp beast: chase, charge, spit, hatch Gloomflies | sure | Wakes on hero range, faces and chases the hero, random spit/hatch/charge choice, owns 4 spawned Gloomflies that it re-hatches, cancels attacks when the hero is grabbed. | Enemy state, attack choice and death from one game; hatchling spawns follow the HATCH choice. | | scenes/Abyss_02b.fsm.txt:25 |
| Abyss_02b.2 | Spike Collider ×6 | FSM Control 12e54eeda1 | Lets recoiling enemies pass spike blocker | no | Enemy physics helper, no hero reads. | | | scenes/Abyss_02b.fsm.txt:669 |

## Abyss_03

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_03.1 | door1 | FSM Quest End 598851c326 | Starts main quest Black Thread Pt2 Abyss | maybe | Fires only when the local hero enters through door1 from the diving bell room; a partner arriving by another route never gets the quest. | The quest begin, if main quests are meant to be shared. | | scenes/Abyss_03.fsm.txt:25 |

## Abyss_04

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_04.1 | Abyss Crawler Large | FSM Control cb282ed2ab | Big crawler charges at hero when alerted | sure | Alert range and FaceObjectV2 use the local hero, so each game charges at a different target. | Enemy state, charge start and death from one game. | | scenes/Abyss_04.fsm.txt:4 |

## Abyss_05

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_05.1 | Abyss Tendril Hero Damager | FSM Control 01b0b2b699 | Drags hero under, damage, hazard respawn, void silk | no | Hazard on the local hero only (skipped with White Flower); the partner just sees the other Hornet's respawn. | | | scenes/Abyss_05.fsm.txt:98 |
| Abyss_05.2 | Abyss Tendrils ×58 | FSM Control d02110d728 | Tendrils rise near hero or needolin | no | Driven by the local hero's range, swim state and needolin; partner's game shows no tendrils around the other Hornet, harmless. | | | scenes/Abyss_05.fsm.txt:139 |
| Abyss_05.3 | Abyss_Tendrils_Low ×47 | C# AbyssWaterTendrils | Decorative tendrils appear near hero | no | Visual only, reads the local hero position. | | | scenes/Abyss_05.objects.txt:1200 |
| Abyss_05.4 | Abyss Water Control | FSM abyss_water_cursed_control 1bc1872d40 | Cursed crest plus flower: water binds and damages | no | Local hero hazard chosen from the local player's crest and item. | | | scenes/Abyss_05.fsm.txt:1669 |
| Abyss_05.5 | Abyss Dive Cutscene | FSM Dive Cutscene 7abf190ecd | Lace dialogue, then interact to dive into abyss lake | sure | Captures the local hero, walks it to the edge, three Lace dialogues, sets disableInventory; the dive interact breaks all delivery items, hides the hero and transitions to Last_Dive (or Last_Dive_Return if PD EncounteredLostLace), one way to the final boss. Cursed players get a refusal dialogue (PD LastDiveCursedConvo). | The dive interact (the commitment to the final fight). | Pull the partner into Last_Dive with the diver (recommended, the fight and ending follow); or let each player dive alone and fight Lost Lace solo; or block the dive until both players stand at the edge. | scenes/Abyss_05.fsm.txt:1841 |
| Abyss_05.6 | Lace Abyss Ghost Cutscene | FSM Lace Cutscene a9884d3f65 | Hero collapses, abyss roar, Lace talks, escape quest starts | sure | Captures the local hero on trigger enter, forces every tendril up (ABYSS ROAR START), four dialogues, then begins main quest Black Thread Pt3 Escape; the partner stays free and its tendrils never roar. | The cutscene start (trigger entered); the quest begin follows natively if the partner also plays it. | Play it for both (pull or freeze the partner while it runs), or only for the trigger player and share just the quest begin. | scenes/Abyss_05.fsm.txt:2053 |
| Abyss_05.7 | Abyss Dive Cutscene, Lace Abyss Ghost Cutscene | FSM Music Control c056467690, Music Control 5ad9b7e297 | Music cues by range | no | Local audio. | | | scenes/Abyss_05.fsm.txt:2018 |
| Abyss_05.8 | particles | C# ParticleEffectsLerpEmission | Rumble particle emission ramp | no | Visual. | | | scenes/Abyss_05.objects.txt:631 |

## Abyss_06

no new elements

## Abyss_07

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_07.1 | Abyss_Weaver_Hanging_Plat ×8 | FSM Break Control 7ffd289014 | Tendrils pull platform away when hero nears | sure | Persistent one-shot: the local hero entering Break Range removes the platform and its terrain collider for good, so the partner's game keeps a platform the other player no longer has, and each player loses it at a different time. | The platform grab (Break Range entered); the tendril and pull animation replay natively. | Share the grab, so a partner on the platform falls too; or keep it per player (each loses its own platforms). | scenes/Abyss_07.fsm.txt:1288 |
| Abyss_07.2 | Music Control | FSM Control f312aaa154 | Music by White Flower | no | Local audio. | | | scenes/Abyss_07.fsm.txt:1680 |

## Abyss_08

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_08.1 | weaver_spire_base control | FSM Control 378c1f11b4 | After super-jump shrine, tendrils drag floor down | sure | Starts on SHRINE SEQUENCE END (the local player gaining super jump) and forces that player to super-jump out; the partner's floor stays put, and a partner without PD hasSuperJump cannot leave the Abyss at all (Abyss_09 ascent needs it). | The shrine sequence end; the drag and reminder replay natively. | Give super jump to both players when one gets it (recommended, otherwise the partner is stuck in the Abyss); or each player must do the shrine themselves. | scenes/Abyss_08.fsm.txt:2742 |
| Abyss_08.2 | terrain collider | C# SplineDepress | Vine floor sags under feet | no | Visual. | | | scenes/Abyss_08.objects.txt:2291 |

## Abyss_09

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_09.1 | First Rumble, Rising Lava | FSM FSM cc647cab8c, Rise Control f9879b6ae6 | Lava chase up the shaft, ends with save | sure | The local hero entering Start Range sets it off; the lava tracks and catches up to the local hero's Y, slows near the super-jump plate, and on escape sets the death respawn and saves; in the partner's game nothing rises, and each lava only chases its own hero. | The chase start (Start Range entered); each game's lava then tracks its own hero. | Start the chase for both when either enters (each outruns its own lava); or keep it per player. A player who dies resets only its own lava. | scenes/Abyss_09.fsm.txt:7007 |
| Abyss_09.2 | Lava_Waterfall Set ×2 | FSM Control 5845afc440 | Lava falls switch on near hero | maybe | Hazards appear on the local hero's range, so timing differs per game; only harms the local hero. | Nothing required. | | scenes/Abyss_09.fsm.txt:7534 |
| Abyss_09.3 | CamLock Final Ascent (2), Cam Controller | FSM Control 2e7b14b49e, Camera Control c5abc5e015 | Camera lock and follow during super jump and chase | no | Per-player camera. | | | scenes/Abyss_09.fsm.txt:6151 |
| Abyss_09.4 | Music Control (1) | FSM Control e7247ae285 | Ascent music by range | no | Local audio. | | | scenes/Abyss_09.fsm.txt:7600 |
| Abyss_09.5 | Rumble Prevent Area | C# WorldRumbleArea | Suppresses world rumble in zone | no | Local camera rumble. | | | scenes/Abyss_09.objects.txt:3691 |

## Abyss_11

no new elements

## Abyss_12

no new elements

## Abyss_13

no new elements

## Abyss_Cocoon

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Abyss_Cocoon.01 | door_entry | FSM Control 322b84b6b9 | Hero falls into arena, darkness, control lock | sure | Entry cutscene from Last_Dive on the local hero (relinquish control, darkness, disableInventory); on a refight it fires HORNET ENTERED to start Lace. A partner who arrives later lands into a fight already running. | Nothing beyond the arrival; it follows from how players get here. | Hold the fight until both players have landed, or let a late partner land into the running fight. | scenes/Abyss_Cocoon.fsm.txt:773 |
| Abyss_Cocoon.02 | Intro Control | FSM Control a3f0f9539c | Boss intro: Lace emerges, roar, title, fight starts | sure | Local hero Start Range trigger locks the hero (disablePause), plays emerge, roar and title, sets PD EncounteredLostLace and sends START to the boss; the refight branch skips straight to the roar. The partner's boss stays dormant. | The intro start (Start Range entered) so both games run the intro and START the boss together. | Pull the partner into the arena and freeze them for the intro (recommended, there is only one way in); or start without them. | scenes/Abyss_Cocoon.fsm.txt:844 |
| Abyss_Cocoon.03 | Lost Lace Boss | FSM Control 24da488c07 | Lost Lace boss AI, 1800 HP, 3 phases | sure | About 130 hero reads: teleports next to the hero, random attack choice (SendRandomEventV4), multihit grabs that snap the hero to her and deal damage, cross-slash catch, stun roars, phase shifts by HP. | Boss state and attack choice from one game, damage from both players to one HP pool. | Which player she targets (nearest, last hitter, alternate). | scenes/Abyss_Cocoon.fsm.txt:1007 |
| Abyss_Cocoon.04 | Lost Lace Boss | FSM Summon Bullets f95e081422 | Spawns orbs and cross-slash catchers near hero | sure | Spawn X comes from the local hero position plus random offsets and a random side. | Follows from the boss state; spawn positions come from the same game. | | scenes/Abyss_Cocoon.fsm.txt:2933 |
| Abyss_Cocoon.05 | Lost Lace Boss | FSM Death Control 71cdca82d1 | HP phase gates, mid-death stagger into phase 3 | sure | Phase 3 starts at 50% HP with a stagger, a splash and projectile cleanup; with two HP pools the phases split between the games. | Shared boss HP; the stagger then follows. | | scenes/Abyss_Cocoon.fsm.txt:3047 |
| Abyss_Cocoon.06 | Corpse Lost Lace(Clone) | FSM Control 6ca43bb1ee | Death explosion, sends LACE DEFEATED | sure | Its LACE DEFEATED starts the ending sequence; if only one game kills her, the other stays mid-fight. | The boss kill; the corpse sequence replays natively. | | scenes/Abyss_Cocoon.fsm.txt:3742 |
| Abyss_Cocoon.07 | Superjump Sequence | FSM Control 597b83e8c1 | Carry Lace, super-jump out, Act 3 ending | sure | Interact hides the local hero behind a dummy, the super-jump charge reads that player's input and silk, then the crash runs SetEndingCompleted Act3Ending, records the Lost Lace journal kill and loads Cinematic_Ending_E; the other player is left in the arena. | The interact (who carries Lace), then the launch, so both games move to the ending together. | Both players go to the ending when one launches (recommended); the partner watches the other Hornet or plays its own dummy; or only the carrier gets the ending and the partner stays in the world. | scenes/Abyss_Cocoon.fsm.txt:4895 |
| Abyss_Cocoon.08 | Damager ×4 (Abyss Wave L1..R2) | FSM Catch Hornet 921a30acc7 | Wave hit snaps hero to a teleplane | maybe | Moves the local hero only when hit; the wave itself comes from the boss state. | Nothing beyond the boss state. | | scenes/Abyss_Cocoon.fsm.txt:4 |
| Abyss_Cocoon.09 | Black Thread Attack (deprecated), Lost_Lace_Black_Thread_Appear_Set ×6 | FSM Catch Control 555df07bb0, Control 1aa58530ea, Control 10ba7135cf | Deprecated thread grab attack | no | Marked deprecated by the developers; if ever active it grabs the local hero and sets Caught Hornet on the boss. | | | scenes/Abyss_Cocoon.fsm.txt:593 |
| Abyss_Cocoon.10 | Lost Lace Boss | FSM Tendril Cooldown bd0aa3b439 | Tendril attack cooldown timer | no | Internal timer, follows the boss state. | | | scenes/Abyss_Cocoon.fsm.txt:3130 |
| Abyss_Cocoon.11 | Ascent Tendril ×10 | FSM Control 4e9b543e0a | Empty idle | no | No actions. | | | scenes/Abyss_Cocoon.fsm.txt:543 |
| Abyss_Cocoon.12 | Abyss_Cocoon_Silk, Single_Bursts, still water_glows | FSM Animate during lace death eeded7f352, muffle 3b94813068, FSM 0bdca65cbb | Arena visuals react to boss events | no | Visual, driven by boss events. | | | scenes/Abyss_Cocoon.fsm.txt:528 |
| Abyss_Cocoon.13 | Audio ChargeSuperJump, Audio Loop SilkChargeScream, Audio Loop SilkGive | FSM fadeUp e25f57d20b, FadeUp f47cb4fe04, FSM c74de1aefc | Audio fades | no | Audio. | | | scenes/Abyss_Cocoon.fsm.txt:5189 |

## Last_Dive

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Last_Dive.1 | Scene Control | FSM FSM 497a6b2416 | Dive cinematic, video, then load Abyss_Cocoon | sure | Cutscene-only scene: hides the hero, disablePause, plays the dive and a video, then BeginSceneTransition to Abyss_Cocoon door_entry; whoever is here is out of the world for its length. | Nothing; it follows from the Abyss_05 dive. | Run both players' dives together (both arrive at once), or let each arrive on its own timer. | scenes/Last_Dive.fsm.txt:4 |
| Last_Dive.2 | hornet | FSM Horizontal Control e7fd8a1bbf | Left/right input sways falling Hornet sprite | no | Cutscene sprite, local input. | | | scenes/Last_Dive.fsm.txt:48 |
| Last_Dive.3 | door_cutscenePosition | FSM Wake Up 50ee5db985 | Takes hero control on entry | no | Local control lock for the cutscene. | | | scenes/Last_Dive.fsm.txt:63 |

## Last_Dive_Return

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Last_Dive_Return.1 | Scene Control | FSM FSM 6d7adf08ca | Short repeat dive, then load Abyss_Cocoon | sure | Same as Last_Dive.1 without the video (refight after PD EncounteredLostLace); the two players can take different variants when their PD differs. | Nothing; follows from the Abyss_05 dive. | Same choice as Last_Dive.1. | scenes/Last_Dive_Return.fsm.txt:4 |
| Last_Dive_Return.2 | hornet | FSM FSM aa82f61a08 | Left/right input sways falling sprite | no | Cutscene sprite. | | | scenes/Last_Dive_Return.fsm.txt:30 |
| Last_Dive_Return.3 | door_cutscenePosition | FSM Wake Up f60b540781 | Takes hero control, fades in | no | Local control lock. | | | scenes/Last_Dive_Return.fsm.txt:45 |

## Cinematic_Ending_A

no new elements

## Cinematic_Ending_B

no new elements

## Cinematic_Ending_C

no new elements

## Cinematic_Ending_D

no new elements

## Cinematic_Ending_E

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cinematic_Ending_E.1 | Award Achievement | FSM FSM bcc59d8b0f | Queues ENDING_E achievement, menu style unlock | no | Local platform achievement; the partner gets it only by reaching this scene (see Abyss_Cocoon.07). | | | scenes/Cinematic_Ending_E.fsm.txt:4 |

## Cinematic_MrMushroom

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cinematic_MrMushroom.1 | ToBeContinued | FSM Text Control 644197eb54 | After video: text, HERALD_WISH achievement, load credits | maybe | Takes that player out of the world into the credits scroll and the menu; the partner is left alone. | Nothing in this scene; handle it where the ending is triggered. | | scenes/Cinematic_MrMushroom.fsm.txt:26 |
| Cinematic_MrMushroom.2 | Cutscene Blanker | FSM Blanker Control 6938ab5141 | Black fade blanker | no | Visual. | | | scenes/Cinematic_MrMushroom.fsm.txt:4 |

## Cinematic_Stag_travel

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Cinematic_Stag_travel.1 | StagTravel | C# StagTravel | Travel video, then load destination | maybe | The traveller leaves the world for the video and arrives in PD nextScene; the partner sees the other Hornet vanish and appear elsewhere. | Nothing new; follows the travel itself. | | scenes/Cinematic_Stag_travel.objects.txt:4 |
| Cinematic_Stag_travel.2 | CinematicSequence, DestroyPersonalPools, Screen Fader | C# CinematicSequence, FadeSequence; FSM FSM e09acf1f45, FSM 7ca18d2635 | Video playback, pool cleanup, fader | no | Local playback and cleanup. | | | scenes/Cinematic_Stag_travel.objects.txt:5 |

## Demo End

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Demo End.1 | Demo End Sequence, Title, TC logo | FSM FSM e8a70aa268; C# LocaliseSprite | Demo end card, menu without save | no | Demo build only. | | | scenes/Demo End.fsm.txt:4 |

## Demo Start

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Demo Start.1 | Demo Start Sequence | FSM FSM 4125678370 | Demo intro card, then new or continue | no | Demo build only. | | | scenes/Demo Start.fsm.txt:1 |

## End_Credits

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| End_Credits.1 | Hornet_Credits_Plates | C# CutsceneHelper | Routes to next scene or main menu after cutscene | maybe | Used by 10 cutscenes; ends with ReturnToMainMenu (save, end game) or a scene load, which ends that player's session while the partner stays in. | Nothing to sync. | What the partner does when the other player's game returns to menu (stay alone, wait, or go back to menu too). | scenes/End_Credits.objects.txt:8 |
| End_Credits.2 | Award Achievements, Ending Styles, Hornet_Credits_Plates, TC, TEAM ×12 | FSM Award 20ec565d9e; C# CompletionStyles, CreditsHelper, ScaleWithAspect, CreditsCardSection | Credits plates, achievements, menu style unlock | no | Local presentation and achievements. | | | scenes/End_Credits.fsm.txt:4 |

## End_Credits_Scroll

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| End_Credits_Scroll.1 | congrats body ×18, Award Achievements, Cutscene Blanker | FSM Perma Check 35ad09dc77, Award 65eecff842, Blanker Control 312bec3ccd | Steel Soul text toggle, achievements, fade | no | Local presentation. | | | scenes/End_Credits_Scroll.fsm.txt:4 |

## End_Game_Completion

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| End_Game_Completion.01 | Sequence | C# EndingSequence | Completion chain, then save and return to menu | maybe | Ends that player's session (ReturnToMainMenu with save); same partner question as End_Credits.1. | Nothing to sync. | | scenes/End_Game_Completion.objects.txt:3 |
| End_Game_Completion.02 | game completion ×2 | C# GameCompletionScreen | Counts completion %, shows playtime | no | Writes local PlayerData completion only; each save keeps its own percentage. | | | scenes/End_Game_Completion.objects.txt:13 |
| End_Game_Completion.03 | Steel Soul Unlock | C# SteelSoulUnlockSequence | Unlocks Steel Soul mode | no | Machine-wide status record. | | | scenes/End_Game_Completion.objects.txt:40 |
| End_Game_Completion.04 | Award Achievements ×2 | FSM Award 50350b6f52 | Checks completion achievements | no | Local achievements. | | | scenes/End_Game_Completion.fsm.txt:39 |
| End_Game_Completion.05 | Sequence, End Credits Thank, congrats body, credits logo, FG Art | C# ChainSequence, AnimatorSequence, TextMeshProContainerFitter, TextHeightYFollower, StatusRecordComparisonResponse, ScreenHeightAdjustPos | Ending presentation layout | no | Visual. | | | scenes/End_Game_Completion.objects.txt:4 |

## Menu_Credits

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Menu_Credits.1 | Ending Styles | C# MenuStyles | Menu style selection | no | Menu cosmetics. | | | scenes/Menu_Credits.objects.txt:2 |

## Menu_Title

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Menu_Title.1 | Fade Atmos In, Fade Music In, Subtitle, Quit Blanker, FG Art ×7, LogoTitle | FSM FSM 57d0f87fba, FSM bb297e3703, Control b4dc0da12a, Blanker Control 0685b0ccd1; C# SharedDataBoolActivator, LogoLanguage | Title audio, logo, menu art, fades | no | Main menu presentation. | | | scenes/Menu_Title.fsm.txt:4 |

## Opening_Sequence

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Opening_Sequence.1 | Sequence | C# OpeningSequence | Prologue, then loads Tut_01 or the save's respawn scene | no | Runs before the world exists; nothing shared. | | | scenes/Opening_Sequence.objects.txt:3 |
| Opening_Sequence.2 | Prologue Excerpt, excerpt author, Audio Loop Wind, Audio Loop Heartbeat | C# SetVector3PerSwitchMode, ChangeTextFontScaleOnHandHeld, AnimatedVolumeControl | Layout and audio | no | Presentation. | | | scenes/Opening_Sequence.objects.txt:5 |

## Opening_Sequence_Act3

no new elements

## PermaDeath

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| PermaDeath.1 | PermaDeath Prompt | FSM FSM 4c73188dc6 | Steel Soul game-over screen, blocks hero input | sure | In Steel Soul a death ends that save and sends that player here; the partner's run goes on with a dead partner. | The Steel Soul death of either player. | Shared fate: end both runs when either dies; or the survivor plays on alone; or forbid coop in Steel Soul. | scenes/PermaDeath.fsm.txt:26 |

## Pre_Menu_Intro

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Pre_Menu_Intro.1 | Camera, StartManager, UIAudioPlayer | C# ForceCameraAspectLite, StartManager, MenuAudioController | Boot logos, loads menu | no | Before any game session. | | | scenes/Pre_Menu_Intro.objects.txt:2 |

## Quit_To_Menu

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |
|---|---|---|---|---|---|---|---|---|
| Quit_To_Menu.1 | QuitToMenu | C# QuitToMenu | Destroys hero and managers, resets PlayerData | maybe | Every quit and every ending passes here: the hero, GameManager, cameras and PlayerData are destroyed or replaced, so anything holding them breaks, and the partner loses the other player mid-world. | The player leaving (session end). | What the partner sees when the other player quits (other Hornet removed, session paused, or partner also kicked). | scenes/Quit_To_Menu.objects.txt:2 |
| Quit_To_Menu.2 | StandaloneLoadingSpinner ×2, Icon ×2 | C# StandaloneLoadingSpinner, UIImageKeepNativeSize | Loading spinner | no | UI. | | | scenes/Quit_To_Menu.objects.txt:3 |

## Summary

1. Pull the partner in (only one way in, and both games must agree):
   - Abyss_05 dive (Abyss_05.5): a one-way commitment to the final fight. If only the diver goes, the partner stays in the Abyss and the fight is solo.
   - Abyss_Cocoon intro (Abyss_Cocoon.02): Lost Lace starts from the local hero's Start Range. Both games must START her together, or the partner's boss stays dormant.
   - Lava chase (Abyss_09.1): the lava only rises in the triggering game. Share the start; each lava then chases its own hero.
2. Ending E (Abyss_Cocoon.07) is the most important open choice. The super-jump crash runs SetEndingCompleted Act3Ending and loads Cinematic_Ending_E, then the credits, then a save and return to menu. Only the game that launches does this. Choose between taking both players to the ending and leaving the partner behind.
3. Progression gate (Abyss_08.1): super jump comes from a shrine and is needed to leave the Abyss. If abilities are not shared, a partner who skipped the shrine is stuck down there.
4. Lost Lace (Abyss_Cocoon.03-06) is a full boss: she targets the hero, teleports next to it, grabs it with multihits, uses RNG, and has HP phase gates at 75%, 55%, 50% and 30%. It needs one authoritative boss state and one HP pool. Then her kill (LACE DEFEATED) leads into Abyss_Cocoon.07.
5. Leaving the world: credits, endings (CutsceneHelper, EndingSequence), Quit_To_Menu and the Steel Soul PermaDeath screen all end one player's session. Quit_To_Menu also destroys the hero, GameManager and PlayerData. Two decisions are needed:
   - What the partner experiences when this happens.
   - Steel Soul fate: is a death shared, does the survivor play on, or is coop not allowed in Steel Soul?
6. Smaller sure items: Gloomfly cocoons and the Gloom Beast (Abyss_02.1, Abyss_02b.1) spawn enemies only in the game whose hero triggered them. The persistent hanging platforms (Abyss_07.1) disappear per player, which gives a choice: share the grab and drop the partner, or keep the platforms per player.
7. Mostly local and fine to leave alone: tendrils and abyss water hazards only hurt the local hero, and all Cinematic and Meta presentation is local.
