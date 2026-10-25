# CaravanTroupeHunter [note by agent 2.04]
SimpleShopMenuOwner: shop menu selling map-pin packs (6 regions) for rosaries; purchase sets PlayerData bools hasPinFlea<Region> and refreshes the map. Opened by the NPC Dialogue FSM (Troupe Hunter) via interact.
Coop: purely per-player menu and currency; the PD bool diff reaches the partner only as a saved flag. Pins are per-player progress unless the user wants a shared map.
