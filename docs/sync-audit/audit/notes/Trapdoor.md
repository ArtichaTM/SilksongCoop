# Trapdoor [note by agent 2.06]
C# lever-driven door/platform: a Lever (top or bottom side) opens it in a direction, it stays open for openWaitTime then closes unless something is inside cannotCloseTrigger; optional persistent stay-open. Retract levers can reset it.
Touches: animators, colliders, optional PersistentBoolItem. Open direction depends on which lever was hit.
