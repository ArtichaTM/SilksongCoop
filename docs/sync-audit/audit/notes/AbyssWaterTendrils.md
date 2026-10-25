# AbyssWaterTendrils [note by agent 2.19]
Decorative abyss tendril cluster: each frame reads the LOCAL hero position and PlayerData.HasWhiteFlower, sets the animator bool ShouldAppear when the hero is inside appearRadius (and outside flowerDisappearRadius if holding the White Flower). Random flip on first appear.
Touches: local hero position and PD read only; no damage, no writes. Partner's game shows tendrils only around its own hero.
