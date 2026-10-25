# Meta: Menus, intro, endings, credits

## Demo End
- FSM [FSM] id=e8a70aa268 at Demo End Sequence (1 uses in game) -> scenes/Demo End.fsm.txt:4
- C# LocaliseSprite at Title, TC logo (2 uses in game) -> scenes/Demo End.objects.txt:1

## Demo Start
- FSM [FSM] id=4125678370 at Demo Start Sequence (1 uses in game) -> scenes/Demo Start.fsm.txt:1

## End_Credits
- FSM [Award] id=20ec565d9e at Award Achievements (1 uses in game) -> scenes/End_Credits.fsm.txt:4
- C# CompletionStyles at Ending Styles (2 uses in game) -> scenes/End_Credits.objects.txt:5
- C# CutsceneHelper at Hornet_Credits_Plates (10 uses in game) -> scenes/End_Credits.objects.txt:8
- C# CreditsHelper at Hornet_Credits_Plates (1 uses in game) -> scenes/End_Credits.objects.txt:8
- C# ScaleWithAspect at Hornet_Credits_Plates, Hornet_Credits_Plates/TC/bg scaler, Hornet_Credits_Plates/TEAM/bg scaler ... (11 uses in game) -> scenes/End_Credits.objects.txt:8
- C# CreditsCardSection at Hornet_Credits_Plates/TC, Hornet_Credits_Plates/TEAM, Hornet_Credits_Plates/TEAM 2 ... (12 uses in game) -> scenes/End_Credits.objects.txt:13

## End_Credits_Scroll
- FSM [Perma Check] id=35ad09dc77 at Abyss_to_Surface_Style/Hornet_Credits_Scroll/9 Message from Team Cherry/congrats body, Abyss_to_Surface_Style/Hornet_Credits_Scroll/9 Message from Team Cherry/congrats body perma (18 uses in game) -> scenes/End_Credits_Scroll.fsm.txt:4
- FSM [Award] id=65eecff842 at Award Achievements (1 uses in game) -> scenes/End_Credits_Scroll.fsm.txt:88
- FSM [Blanker Control] id=312bec3ccd at Cutscene Blanker (1 uses in game) -> scenes/End_Credits_Scroll.fsm.txt:95

## End_Game_Completion
- FSM [Award] id=50350b6f52 at Sequence/Game Completion Act 2/Award Achievements, Sequence/Game Completion Act 3/Award Achievements (2 uses in game) -> scenes/End_Game_Completion.fsm.txt:39
- C# ChainSequence at Sequence (3 uses in game) -> scenes/End_Game_Completion.objects.txt:3
- C# EndingSequence at Sequence (1 uses in game) -> scenes/End_Game_Completion.objects.txt:3
- C# AnimatorSequence at Sequence/End Credits Thank, Sequence/Game Completion Act 2, Sequence/Game Completion Act 3 (6 uses in game) -> scenes/End_Game_Completion.objects.txt:4
- C# TextMeshProContainerFitter at Sequence/End Credits Thank/9 Message from Team Cherry/congrats body, Sequence/End Credits Thank/9 Message from Team Cherry/congrats body perma (2 uses in game) -> scenes/End_Game_Completion.objects.txt:8
- C# TextHeightYFollower at Sequence/End Credits Thank/9 Message from Team Cherry/credits logo (1 uses in game) -> scenes/End_Game_Completion.objects.txt:10
- C# GameCompletionScreen at Sequence/Game Completion Act 2/game completion, Sequence/Game Completion Act 3/game completion (2 uses in game) -> scenes/End_Game_Completion.objects.txt:13
- C# SteelSoulUnlockSequence at Sequence/Steel Soul Unlock (1 uses in game) -> scenes/End_Game_Completion.objects.txt:40
- C# StatusRecordComparisonResponse at Ending Styles/Cradle_Style/Group/FG Art (1 uses in game) -> scenes/End_Game_Completion.objects.txt:67
- C# ScreenHeightAdjustPos at Ending Styles/Cradle_Style/Group/FG Art (8 uses in game) -> scenes/End_Game_Completion.objects.txt:67

## Menu_Credits
- C# MenuStyles at Ending Styles (2 uses in game) -> scenes/Menu_Credits.objects.txt:2

## Menu_Title
- FSM [FSM] id=57d0f87fba at Fade Atmos In (1 uses in game) -> scenes/Menu_Title.fsm.txt:4
- FSM [FSM] id=bb297e3703 at Fade Music In (1 uses in game) -> scenes/Menu_Title.fsm.txt:16
- FSM [Control] id=b4dc0da12a at LogoTitle/Subtitle (1 uses in game) -> scenes/Menu_Title.fsm.txt:28
- FSM [Blanker Control] id=0685b0ccd1 at Quit Blanker (1 uses in game) -> scenes/Menu_Title.fsm.txt:47
- C# SharedDataBoolActivator at Menu_Styles/Hornet_Style/FG Art Container/FG Art, Menu_Styles/Song_Style/FG Art Container/FG Art, Menu_Styles/Cradle_Style/FG Art Container/FG Art ... (7 uses in game) -> scenes/Menu_Title.objects.txt:13
- C# LogoLanguage at LogoTitle (2 uses in game) -> scenes/Menu_Title.objects.txt:405

## Opening_Sequence
- C# OpeningSequence at Sequence (2 uses in game) -> scenes/Opening_Sequence.objects.txt:3
- C# SetVector3PerSwitchMode at Sequence/Excerpt/Prologue Excerpt (2 uses in game) -> scenes/Opening_Sequence.objects.txt:5
- C# ChangeTextFontScaleOnHandHeld at Sequence/Excerpt/Prologue Excerpt/text/excerpt author (1 uses in game) -> scenes/Opening_Sequence.objects.txt:16
- C# AnimatedVolumeControl at Sequence/Excerpt/Prologue Excerpt/Audio Loop Wind, Sequence/Excerpt/Prologue Excerpt/Audio Loop Heartbeat (3 uses in game) -> scenes/Opening_Sequence.objects.txt:59

## Opening_Sequence_Act3

## PermaDeath
- FSM [FSM] id=4c73188dc6 at PermaDeath Prompt (1 uses in game) -> scenes/PermaDeath.fsm.txt:26

## Pre_Menu_Intro
- C# ForceCameraAspectLite at Camera (1 uses in game) -> scenes/Pre_Menu_Intro.objects.txt:2
- C# StartManager at StartManager (1 uses in game) -> scenes/Pre_Menu_Intro.objects.txt:3
- C# MenuAudioController at StartManager/UIAudioPlayer (1 uses in game) -> scenes/Pre_Menu_Intro.objects.txt:14

## Quit_To_Menu
- C# QuitToMenu at QuitToMenu (1 uses in game) -> scenes/Quit_To_Menu.objects.txt:2
- C# StandaloneLoadingSpinner at StandaloneLoadingSpinner, StandaloneLoadingSpinner EndGame (2 uses in game) -> scenes/Quit_To_Menu.objects.txt:3
- C# UIImageKeepNativeSize at StandaloneLoadingSpinner/Canvas/Icon, StandaloneLoadingSpinner EndGame/Canvas/Icon (2 uses in game) -> scenes/Quit_To_Menu.objects.txt:6
