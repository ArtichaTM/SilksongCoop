# RosaryCache / RosaryCacheString / RosaryCacheShrine [note by agent 2.02]
Hit-responder (IHitResponder) hanging string or shrine full of rosaries: each nail hit flings random rosary pickups (UnityEngine.Random counts/angles); hits-left kept in a PersistentIntItem (written on SavePersistentObjects).
Coop: a hit is seen only in the hitting game and each game flings its own random rosaries, so loot copies differ. Share the hit with one game's roll, or accept per-game loot.
