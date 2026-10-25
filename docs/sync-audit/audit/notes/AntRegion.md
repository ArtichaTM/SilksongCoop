# AntRegion / AntRegionHandler [note by agent 2.08]
Pickup trigger that carries loose objects away with an ant swarm: anything on layer 18 or 26 (corpses, dropped currency, shell shards) or with AntRegionHandler, unless its ICheck.CanEnterAntRegion is false. It makes the body kinematic, moves it (direction away from the hero or fixed), then drops it and notifies INotify/IPickUpNotify.
Touches loose scene objects' Rigidbody2D and reads HeroController.instance position; no PlayerData. Hero itself is not carried.
