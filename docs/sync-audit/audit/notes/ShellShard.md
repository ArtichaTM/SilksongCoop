# ShellShard [note by agent 2.06]
C# currency pickup (CurrencyObject): collected by the hero touching it, calls CurrencyManager.AddShards(value) and shows a first-time popup (PlayerData HasSeenShellShards). Persistent variants remember they were collected in a PersistentBoolItem.
Touches: shard currency of the collecting hero, PlayerData flag, persistent scene bool.
