# QuitToMenu [note by agent 2.19]
Quit_To_Menu scene driver: destroys UIManager, HeroController, GameCameras and GameManager, replaces PlayerData with a fresh singleton, resets sceneData, releases pools, then loads the menu.
Touches: everything global; any object holding the old hero, GameManager or PlayerData reference is left dangling.
