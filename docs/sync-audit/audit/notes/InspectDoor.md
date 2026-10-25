# InspectDoor [note by agent 2.17]
Interactable wrapper around another door: first use asks a Yes/No box ("enter?"), then calls door.Interact(); a PersistentBoolItem remembers hasEnteredBefore so later uses skip the prompt.
Touches: local hero input/menu, the wrapped door (scene transition), one scene bool.
Coop: only the interacting hero enters; the prompt flag is saved per game.
