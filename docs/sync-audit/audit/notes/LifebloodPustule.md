# LifebloodPustule (+ LifebloodPustuleSubWither) [note by agent 2.05]
IHitResponder blob: normal hits break it and it regenerates; an ExtractMoss attack pins the hero (EnsureExtractPosition) and on success breaks it for good, saves a PersistentBoolItem, and withers linked sub-objects (vines, sometimes path-blocking roots).
Touches: hero position/state during extract, extract events, SceneData bool. Coop: only the extractor's game withers the vines; share the extracted break.
