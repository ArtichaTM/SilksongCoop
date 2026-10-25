# ToolGameObjectActivator [note by agent 2.08]
On enable, tests ToolItem.IsUnlocked against expected states and activates/deactivates two GameObjects (like TestGameObjectActivator but keyed on owned tools).
Reads the local save's tool unlocks; touches nothing else. Coop: two players with different tools see different scene dressing (e.g. a pickup string only shown when a tool is missing).
