# ItemReceptacle [note by agent 2.06]
C# inspectable lock that takes a required CollectableItem (e.g. Dock Key): inspect dialogue, yes/no prompt, then consumes the item, plays an unlock effect and calls Open on an UnlockablePropBase. Activated state stored in a PlayerData bool or a PersistentBoolItem.
Touches: hero input (dialogue), inventory (consumes item), PlayerData/persistent bool, the unlocked prop.
