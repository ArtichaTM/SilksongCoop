# TrackTriggerObjectsLineOfSight [note by agent 2.04]
TrackTriggerObjects variant: a collider only counts an object if a raycast from itself (or parent) to the object hits no trigger-free terrain (layer mask 256). Used as "Target Range" to feed GetTrackTriggerClosestObject (Garmond picks its nearest target: hero or enemy).
Coop: counts any collider in the trigger; a partner with no collider would never be a candidate, so target choice sees only the local hero and enemies.
