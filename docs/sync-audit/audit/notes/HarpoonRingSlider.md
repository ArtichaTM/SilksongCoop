# HarpoonRingSlider [note by agent 2.14]
A harpoon ring that slides up to a target while the hero hangs on it (HeroOnRing) and back down when the hero lets go (HeroOffRing). It is inert until ActivateRing() enables its collider.
On arrival it sends RING DROP IMPACT to HeroController.instance (the local hero only) and turns on a camera lock while ridden.
Touches: local hero, camera lock. No saved state, no RNG.
