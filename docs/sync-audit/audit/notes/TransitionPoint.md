# TransitionPoint (+ GateSnap) [note by agent 2.01]
Scene exit/entry gate: hero entering the trigger (or door interact) calls GameManager.BeginSceneTransition to targetScene:entryGate; entry gate name is stored in the GameManager and read by TestGameObjectActivator and FSMs (GetEntryGateName). Local hero only.
Coop: a transition moves only the local player; the partner stays in the old scene unless the mod pulls them along.
