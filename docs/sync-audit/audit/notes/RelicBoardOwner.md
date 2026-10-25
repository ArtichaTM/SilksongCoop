# RelicBoardOwner / CollectableRelicBoard / CollectionGramaphone [note by agent 2.10]
Relic dealer: DepositCollectedRelics marks each collected relic as deposited (relic SavedData in the hero's PlayerData) and sets completedBool when all are in. The gramaphone plays a deposited relic's music and stores the playing relic in a PlayerData field. Board UI is inventory-pane code.
Touches: PlayerData relic data and one PD bool; UI and audio. Per-save; nothing else in the scene.
