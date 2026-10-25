# Sync audit agent prompt (template)

Fill in `{ID}`, `{SCENES}`, `{CATALOG_SLICE}` (catalog file(s) + line ranges) and `{OUT}` (audit/<NN>_<Area>.md).
Models: Sonnet 5.5 at high effort, Opus 5.5 at medium (2.01, 2.11, 2.19).

You are auditing Hollow Knight: Silksong scene logic for a 2-player coop mod. Each player runs their own full
game, and the mod mirrors the partner and shares selected events over the network. Goal: for every scene element
below, decide what coop needs, so bugs are pre-coded instead of discovered in play (today ~80% of enemies and
events break in coop).

THE GIST: judge every element as a 2-player coop design question, not only "does the state match". Either player
can trigger anything. What goes wrong for the other player, and what is the smallest thing to share so both games
behave the same?

YOUR SLICE: agent {ID}, scenes {SCENES}: {CATALOG_SLICE}.

INPUTS: `catalog/` and `audit/` are under `<repo>/docs/sync-audit/`; `scenes/` and `decomp/` are under `<repo>/data/FsmDump/`
(the untracked dump; `scenes/...` refs are relative to it):
- `catalog/<Area>.md`: per scene, the elements that appear there FOR THE FIRST TIME in the game. They are already
  deduplicated globally, so nobody else audits them. Each line: `FSM [<fsm name>] id=<sig>` or `C# <ComponentType>`,
  object paths, uses in the whole game, and `-> scenes/<file>:<line>` pointing at the definition.
- `scenes/<Scene>.fsm.txt`: full PlayMaker FSM dump. Read the block at the given line (it runs until the next `FSM ` line).
- `scenes/<Scene>.objects.txt`: object hierarchy with components (`HealthManager(hp=..)`, `TransitionPoint(->scene:gate)`,
  `PersistentBoolItem(id=..)`).
- `decomp/`: decompiled game C#, including custom PlayMaker actions under `decomp/HutongGames.PlayMaker.Actions/`.
- `audit/notes/<TypeName>.md`: SHARED notes with the other agents running in parallel. Check for a note before explaining
  a C# type or action. If none exists and the type matters, write a 2-5 line note covering what it does and what it
  touches (hero, PlayerData, scene bools, RNG, camera, spawns). Never overwrite an existing note.
Do not read the mod source. This audit describes the game, not our implementation. Use no mod terms in your output
(EnemySync, puppet, PickupSync, Shared/Authority and the like), and never read or write `Status`/`Code` columns.

WHAT MATTERS: anything that targets or reads the hero (aggro, chase, attack choice), writes PlayerData or persistent
scene bools/ints, battle scenes, arena locks and gates, bosses, cutscenes, camera and input locks, scene transitions,
spawns, RNG-driven attacks, pickups and currency, NPC dialogue and quests, breakables, platforms, levers and lifts.
Pure visuals and ambience are usually "no".

OUTPUT: write `{OUT}`. Write no other file except new notes in audit/notes/. Everything else is read-only:
no git, no builds. No horizontal rules (`---`).

One `## <Scene>` heading per scene in catalog order (write "no new elements" under empty scenes), then one Markdown table:

| # | Object | FSM / C# | Does | Verdict | Why | What to sync | Design choice | Ref |

- `#` (required): `<Scene>.<k>`, numbered from 1 within the scene.
- `Object` (required): GameObject name copied from the dump, last path segment only (`Mossbone Mother`, not
  `Black Thread States/Normal World/Battle Scene/Wave 1/Mossbone Mother`). When one FSM runs on several objects, name a
  representative one and summarise the rest (e.g. `Bone Flyer ×5`, `Pilgrim Trap Spike ×8`); exact paths aren't needed. When the
  FSM runs on or mainly controls Hornet, write `Hornet`.
- `FSM / C#` (required): FSM name + id, or C# type, as in the catalog. Group near-identical variants into one row
  that lists every id.
- `Does` (required): what it does, 8 words or fewer. Write `?` when the FSM does not make it clear; never invent a purpose.
- `Verdict` (required): `sure` | `maybe` | `no`, meaning: does coop need to handle it.
- `Why` (optional): why that verdict, in coop terms (what goes wrong for the other player). One sentence, hard limit;
  details belong in the dump, not the table. For an enemy or anything else that reacts to the hero, list EVERY
  input that reads the hero, not just the most unusual one: alert ranges by name (wake, chase, attack), line of
  sight, needolin, and targeting actions (face, chase, distance, aim, nearest-to-hero). Example: `Chase Range
  wakes the run, needolin, X distance, face on emerge and the teleplane near the hero read the local hero only.`
- `What to sync` (optional): the smallest event to share, after which each game replays the rest by itself. Example:
  a lever raises a platform, so share only the lever hit; the platform and its animations then play in both games
  natively. Don't sync the lever and the platform separately. Don't write implementation steps. One sentence.
- `Design choice` (optional): when the verdict is sure but the right coop behaviour is a choice only the user can make
  (e.g. a first-meeting cutscene: pull the partner in, freeze them, or leave them free?). Name the options.
- `Ref` (required): `scenes/<file>:<line>` from the catalog.
No `|` inside cells. Every catalog element in your slice must be covered by some row; trivial ones can be grouped
(e.g. `decorative breakables ×7`, verdict `no`).

End with a `## Summary` section (required). Its content is your call: put there whatever the user most needs to know
about your slice. For example, a list of events that should pull the partner to the player who triggered them
(boss arenas, forced cutscenes), each with a one-line reason; other important choices or gaps follow the same idea.

Your final message: only the output path and a 2-3 line summary.
