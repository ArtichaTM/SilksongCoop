using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SilksongCoop.Game;
using SilksongCoop.Rules;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongCoop.Dev;

/// <summary>
/// Dev teleports to the scenes and objects the sync verdicts name (docs/sync-audit/teleports.tsv). Near
/// lands on the hazard respawn point closest to the object, or flies in place when none is close; only
/// this hero moves.
/// </summary>
internal sealed class TeleportMenu(Coop coop, FreeFly fly)
{
    private const string Resource = "SilksongCoop.teleports.tsv";
    private const float NearRadius = 30f;
    private const float RequestTimeout = 20f;

    private readonly List<TeleportRow> _rows = LoadRows();
    private readonly Dictionary<TeleportRow, int> _cycle = [];
    private readonly Dictionary<string, Transform?> _live = [];
    private string? _liveScene;
    private string _filter = "";
    private Vector2 _scroll;
    private bool _focus;
    private bool _heldControl;
    private bool _pauseWasAllowed;
    private Action? _request;
    private float _requestUntil;
    private (TeleportRow Row, int Index)? _arrival;

    public bool Open { get; private set; }

    public void Toggle()
    {
        if (Open)
        {
            Close();
            return;
        }
        Open = true;
        _focus = true;
        if (GameRefs.HeroInControl)
        {
            GameRefs.Hero!.RelinquishControl();
            _heldControl = true;
        }
        if (GameRefs.Manager is { } gm)
        {
            _pauseWasAllowed = gm.inputHandler.PauseAllowed;
            gm.inputHandler.PreventPause();
        }
    }

    public void Close()
    {
        if (!Open)
        {
            return;
        }
        Open = false;
        if (_heldControl && GameRefs.Hero is { } hero && hero.controlReqlinquished)
        {
            hero.RegainControl();
        }
        _heldControl = false;
        if (_pauseWasAllowed)
        {
            GameRefs.Manager?.inputHandler.AllowPause();
        }
    }

    public void Update()
    {
        if (_request is not null)
        {
            RunRequest();
        }
        if (_arrival is { } arrival && GameRefs.GameplayScene == arrival.Row.Load && GameRefs.HeroInControl)
        {
            _arrival = null;
            Place(arrival.Row, arrival.Index);
        }
        if (_arrival is not null && Time.unscaledTime > _requestUntil)
        {
            _arrival = null;
        }
    }

    public void Draw()
    {
        if (!Open)
        {
            return;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (Event.current.type == EventType.KeyDown && HandleKey(Event.current.keyCode))
        {
            Event.current.Use();
            return;
        }
        float width = Mathf.Min(760f, Screen.width - 40f);
        float height = Screen.height * 0.75f;
        Rect area = new((Screen.width - width) / 2f, Screen.height * 0.1f, width, height);
        GUI.Box(area, "");
        GUILayout.BeginArea(new Rect(area.x + 8, area.y + 8, area.width - 16, area.height - 16));
        GUI.SetNextControlName("TeleportFilter");
        _filter = GUILayout.TextField(_filter);
        if (_focus)
        {
            GUI.FocusControl("TeleportFilter");
            _focus = false;
        }
        GUILayout.Label(Summary());
        _scroll = GUILayout.BeginScrollView(_scroll);
        DrawRows();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private static List<TeleportRow> LoadRows()
    {
        using Stream? stream = typeof(TeleportMenu).Assembly.GetManifestResourceStream(Resource);
        if (stream is null)
        {
            return [];
        }
        using StreamReader reader = new(stream);
        return TeleportTable.Rows(TeleportTable.Parse(reader.ReadToEnd()));
    }

    /// <summary>Enter on an exact id teleports near it, Esc closes; true when the key was handled.</summary>
    private bool HandleKey(KeyCode key)
    {
        if (key == KeyCode.Escape)
        {
            Close();
            return true;
        }
        if (key is not (KeyCode.Return or KeyCode.KeypadEnter))
        {
            return false;
        }
        if (_rows.FirstOrDefault(row => row.Id == _filter.Trim()) is { } exact && CanNear(exact))
        {
            Ask(() => Near(exact));
        }
        else
        {
            coop.Hints.Show($"No teleport for {_filter.Trim()}");
        }
        return true;
    }

    private string Summary()
    {
        string query = _filter.Trim();
        TeleportRow? exact = _rows.FirstOrDefault(row => row.Id == query);
        if (exact is not null)
        {
            return $"Enter: near {exact.Label}, {exact.Scene}";
        }
        return query.Length == 0 ? "Type an id (02.01), a row id or a name" : "No exact id; pick below";
    }

    private void DrawRows()
    {
        string? file = null;
        string? scene = null;
        foreach (TeleportRow row in _rows.Where(row => TeleportTable.Matches(row, _filter)))
        {
            string rowFile = TeleportTable.File(row.Id);
            if (rowFile != file)
            {
                file = rowFile;
                scene = null;
                GUILayout.Label($"<b>verdicts/{file}.md</b>");
            }
            if (row.Scene != scene)
            {
                scene = row.Scene;
                DrawScene(row);
            }
            DrawRow(row);
        }
    }

    private void DrawScene(TeleportRow row)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"  <b>{row.Scene}</b>");
        GUI.enabled = row.Door.Length > 0;
        if (GUILayout.Button("Scene", GUILayout.Width(70)))
        {
            Ask(() => ToScene(row));
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }

    private void DrawRow(TeleportRow row)
    {
        GUILayout.BeginHorizontal();
        GUI.enabled = CanNear(row);
        GUILayout.Label($"    {row.Label}");
        if (GUILayout.Button("Near", GUILayout.Width(60)))
        {
            Ask(() => Near(row));
        }
        GUI.enabled = row.Door.Length > 0;
        if (GUILayout.Button("Scene", GUILayout.Width(70)))
        {
            Ask(() => ToScene(row));
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }

    /// <summary>A dump position, or, in its scene, the live object.</summary>
    private bool CanNear(TeleportRow row)
    {
        if (row.Objects.Any(t => t.X is not null))
        {
            return row.Door.Length > 0 || GameRefs.GameplayScene == row.Load;
        }
        return GameRefs.GameplayScene == row.Load && row.Objects.Any(t => LivePosition(t) is not null);
    }

    /// <summary>Runs once the menu is closed and the hero has its control back.</summary>
    private void Ask(Action request)
    {
        Close();
        _request = request;
        _requestUntil = Time.unscaledTime + RequestTimeout;
    }

    private void RunRequest()
    {
        if (Time.unscaledTime > _requestUntil)
        {
            _request = null;
            coop.Hints.Show("Teleport gave up: no control of the hero");
            return;
        }
        if (!GameRefs.HeroInControl)
        {
            return;
        }
        Action request = _request!;
        _request = null;
        request();
    }

    private void ToScene(TeleportRow row)
    {
        if (GameRefs.GameplayScene == row.Load)
        {
            coop.Hints.Show($"Already in {row.Scene}");
            return;
        }
        coop.Hints.Show($"Teleporting to {row.Scene}");
        coop.Pulls.TravelAlone(row.Load, row.Door);
    }

    private void Near(TeleportRow row)
    {
        int index = TeleportTable.Next(row, _cycle.TryGetValue(row, out int last) ? last : -1);
        if (index < 0)
        {
            coop.Hints.Show($"{row.Id} has no object");
            return;
        }
        _cycle[row] = index;
        if (GameRefs.GameplayScene == row.Load)
        {
            Place(row, index);
            return;
        }
        _arrival = (row, index);
        _requestUntil = Time.unscaledTime + RequestTimeout;
        coop.Pulls.TravelAlone(row.Load, row.Door);
    }

    private void Place(TeleportRow row, int index)
    {
        TeleportTarget target = row.Objects[index];
        string name = target.Path.Split('/').Last();
        string count = $"{index + 1}/{row.Objects.Count}";
        Vector2? at = LivePosition(target) ?? Dumped(target);
        if (at is not { } position)
        {
            coop.Hints.Show($"{count} {name}: no position");
            return;
        }
        if (SafeSpot(position) is { } safe)
        {
            coop.Pulls.MoveAlone(safe);
            coop.Hints.Show($"{count} {name}");
            return;
        }
        coop.Pulls.MoveAlone(position);
        if (!fly.Flying)
        {
            fly.Toggle();
        }
        coop.Hints.Show($"{count} {name}: no safe spot near, flying");
    }

    private static Vector2? Dumped(TeleportTarget target) =>
        target.X is { } x && target.Y is { } y ? new Vector2(x, y) : null;

    /// <summary>The hazard respawn point nearest the object: where the game puts the hero back.</summary>
    private static Vector2? SafeSpot(Vector2 near)
    {
        Vector2? best = null;
        float bestDistance = NearRadius;
        foreach (HazardRespawnMarker marker in UnityEngine.Object.FindObjectsByType<HazardRespawnMarker>(
                     FindObjectsSortMode.None))
        {
            Vector2 spot = marker.transform.position;
            float distance = Vector2.Distance(spot, near);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = spot;
            }
        }
        return best;
    }

    /// <summary>The object found by its dump path in the loaded scenes, looked up once per load.</summary>
    private Vector2? LivePosition(TeleportTarget target)
    {
        string? scene = GameRefs.GameplayScene;
        if (scene is null)
        {
            return null;
        }
        if (_liveScene != scene)
        {
            _liveScene = scene;
            _live.Clear();
        }
        bool stale = _live.TryGetValue(target.Path, out Transform? found) && found is not null && !found;
        if (stale || !_live.ContainsKey(target.Path))
        {
            found = Find(target.Path);
            _live[target.Path] = found;
        }
        return found ? (Vector2)found!.position : null;
    }

    private static Transform? Find(string path)
    {
        string[] parts = path.Split(['/'], 2);
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            foreach (GameObject root in SceneManager.GetSceneAt(i).GetRootGameObjects())
            {
                if (root.name != parts[0])
                {
                    continue;
                }
                Transform? found = parts.Length == 1 ? root.transform : root.transform.Find(parts[1]);
                if (found)
                {
                    return found;
                }
            }
        }
        return null;
    }
}
