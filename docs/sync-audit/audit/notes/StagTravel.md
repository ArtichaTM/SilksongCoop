# StagTravel (FastTravelCutscene) [note by agent 2.19]
Bellway/stag travel interstitial in Cinematic_Stag_travel: plays a video (children variant if PD UnlockedFastTravelTeleport), flips by map direction lastScene -> PD nextScene, then BeginSceneTransition to PD nextScene once faded out or skipped.
Touches: reads PD, does the scene load; the traveller is out of the world for the whole video.
