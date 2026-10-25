# PersistentFolderReset [note by agent 2.14]
Keeps a hidden template folder (usually enemies) and spawns a fresh copy of it on Awake and again on GameManager.ResetSemiPersistentObjects (bench rest, optionally hero death).
Effect: the folder's enemies come back after a rest/death without a scene reload. Each game resets on its own hero's rest/death.
Touches: spawns objects (enemies), reads hero dead state.
