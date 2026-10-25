# WindRegion / UmbrellaWindRegion / WindCameraRegion [note by agent 2.10]
Trigger regions counting how many are active around the LOCAL hero; flips hero cState.inWindRegion. UmbrellaWindRegion adds a horizontal push speed (SpeedX) that the hero's brolly float reads, plus a WindZone for particles. WindCameraRegion applies a camera sway shake.
Touches: hero cState and camera only; no PlayerData, no RNG. Per-player by nature.
