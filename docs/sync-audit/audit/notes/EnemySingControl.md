# EnemySingControl [note by agent 2.02]
Enemy "sings" back while the hero plays needolin: loops sing audio, spawns thread/puppet-string effects, picks a random sing duration (4-8 s) and sends SING DURATION END. Audio/visual + timing only, no PlayerData.
Coop: purely cosmetic once the FSM state is mirrored (owner's FsmState is sent); trigger is the owner's own needolin (see CheckHeroPerformanceRegion).
