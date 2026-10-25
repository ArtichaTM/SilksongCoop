# ThreadSpinner [note by agent 2.04]
Nail-hit (IHitResponder) thread spool: each hit shrinks it, spawns thread pickups with UnityEngine.Random, and after addSilkDelay calls HeroController.AddSilk(addSilkPerHit) on the LOCAL hero. Hit count lives in a PersistentIntItem (semi-persistent: resets on bench/rest).
Coop: not an FSM, so the partner's hit is not seen; each game would keep its own hit count and give silk only to its own hero. Decide shared spool (share the hit) vs one spool per player.
