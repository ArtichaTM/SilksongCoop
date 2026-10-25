# PlayMakerEventRange (+ SendEventToRange) [note by agent 2.08]
Registers each instance in a static list; SendEventToRange (from an FSM action) delivers an event to the target FSM and recursively to every other registered range within sendRange (handledEvents stops loops). Used to spread a crow's wake/alert/death cry to nearby crows.
Pure in-scene messaging, no hero or PlayerData access; the cascade starts wherever the first enemy's own logic raised the event.
