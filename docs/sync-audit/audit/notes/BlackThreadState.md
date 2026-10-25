# BlackThreadState [note by agent 2.01]
Component on enemies/props that switches them to a black-thread variant when PlayerData.blackThreadWorld is true (extra HP/behaviour/visuals); reads HeroController position for thread effects. Variant roots are usually chosen by TestGameObjectActivator.
Coop: depends on a PD flag that must match on both sides, otherwise partner's mirrored enemy is a different variant.
