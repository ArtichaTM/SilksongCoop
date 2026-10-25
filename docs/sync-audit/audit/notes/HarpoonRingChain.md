# HarpoonRingChain + Harpoon Ring FSMs [note by agent 2.06]
C# is only the ring/chain visuals and swing impulses. The Control FSM does the logic: hero throws the harpoon/needle to the ring (CONNECT), hero can grab and hang (GRAB), ring drops/ejects the hero. A Pull Switch FSM (persistent Activated bool) lowers while a hero hangs and fires OPEN on a gate when it reaches the bottom.
Touches: hero state (grab, drop impact animation, controls), camera shake, a persistent scene bool for the pull switch.
