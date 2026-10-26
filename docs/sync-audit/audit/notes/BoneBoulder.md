# Bone_Boulder, FSM Control 46fae9c7dc [note by main 2026-10-26]
Audit correction (Bone_01c.6 said `no`): the boulder hangs until a hero enters its Alert Range (or BOULDER HIT / TRAP), then drops, hurts any hero under it (2) and enemies (80) and breaks on the ground. Drop Antic sets the FSM bool Activated; nothing is saved, so it hangs again on every room load. Used in about 15 Bone, Bone_East, Coral and Clover rooms (`grep 46fae9c7dc index.tsv`).
Coop: a boulder dropped by one hero must drop in both games (it hurts both), and a player entering after it fell must not find it hanging.
