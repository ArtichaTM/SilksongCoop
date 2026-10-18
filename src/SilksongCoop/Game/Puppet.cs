using System.Collections.Generic;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// The partner's Hornet (D23, D36): plain tk2d sprites that copy the partner's body, attached effects and
/// thrown tools frame by frame, in the configured tint, with a name tag. No colliders, no game logic.
/// </summary>
internal sealed class Puppet
{
    private const float TagHeight = 2.4f;
    private readonly GameObject _root;
    private readonly tk2dSprite _body;
    private readonly Transform _tag;
    private readonly Dictionary<int, tk2dSprite> _effects = [];
    private readonly Dictionary<int, tk2dSprite> _tools = [];
    private readonly Dictionary<int, SpriteRenderer> _flatTools = [];
    private readonly HashSet<int> _seen = [];
    private Color _tint;
    private int _sortingLayer;
    private int _sortingOrder;

    public Puppet(string name, Color tint)
    {
        _tint = tint;
        _root = new GameObject("SilksongCoop Partner");
        Object.DontDestroyOnLoad(_root);
        _body = NewSprite("Body", _root.transform);
        _tag = NewTag(name);
        _root.SetActive(false);
    }

    public Transform Root => _root.transform;

    public bool Visible => _root.activeSelf;

    public void SetTint(Color tint)
    {
        _tint = tint;
        _body.color = tint;
        foreach (tk2dSprite sprite in _effects.Values)
        {
            sprite.color = tint;
        }
    }

    public void Hide() => _root.SetActive(false);

    /// <summary>Draws one received frame; the body stays hidden while the partner's is.</summary>
    public void Show(bool bodyShown, SpriteFrame body, List<SpriteFrame> effects, List<SpriteFrame> tools)
    {
        _root.SetActive(true);
        CopySorting();
        float z = GameRefs.Hero is { } hero ? hero.transform.position.z + 0.001f : 0f;
        _root.transform.position = new Vector3(body.Position.x, body.Position.y, z);
        // A pose from a collection not loaded here keeps the last known pose rather than vanishing.
        bool drawn = body.ApplyTo(_body, z) || _body.Collection;
        _body.gameObject.SetActive(bodyShown && drawn);
        _tag.position = new Vector3(body.Position.x, body.Position.y + TagHeight, z);
        Sync(_effects, effects, bodyShown, z, _sortingOrder + 1);
        Sync(_tools, tools.FindAll(frame => !frame.IsFlat), true, z, _sortingOrder + 1);
        SyncFlat(tools.FindAll(frame => frame.IsFlat), z);
    }

    public void Destroy() => Object.Destroy(_root);

    private void Sync(
        Dictionary<int, tk2dSprite> shown,
        List<SpriteFrame> frames,
        bool enabled,
        float z,
        int order
    )
    {
        _seen.Clear();
        foreach (SpriteFrame frame in frames)
        {
            _seen.Add(frame.Key);
            if (!shown.TryGetValue(frame.Key, out tk2dSprite? sprite) || !sprite)
            {
                sprite = NewSprite("Effect", _root.transform);
                shown[frame.Key] = sprite;
            }
            sprite.GetComponent<MeshRenderer>().sortingOrder = order;
            sprite.gameObject.SetActive(enabled && frame.ApplyTo(sprite, z));
        }
        foreach (KeyValuePair<int, tk2dSprite> entry in shown)
        {
            if (!_seen.Contains(entry.Key) && entry.Value)
            {
                entry.Value.gameObject.SetActive(false);
            }
        }
    }

    private void SyncFlat(List<SpriteFrame> frames, float z)
    {
        _seen.Clear();
        foreach (SpriteFrame frame in frames)
        {
            _seen.Add(frame.Key);
            if (!_flatTools.TryGetValue(frame.Key, out SpriteRenderer? renderer) || !renderer)
            {
                renderer = new GameObject("Flat Tool").AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(_root.transform, false);
                renderer.color = _tint;
                _flatTools[frame.Key] = renderer;
            }
            renderer.sortingLayerID = _sortingLayer;
            renderer.sortingOrder = _sortingOrder + 1;
            renderer.gameObject.SetActive(frame.ApplyTo(renderer, z));
        }
        foreach (KeyValuePair<int, SpriteRenderer> entry in _flatTools)
        {
            if (!_seen.Contains(entry.Key) && entry.Value)
            {
                entry.Value.gameObject.SetActive(false);
            }
        }
    }

    private void CopySorting()
    {
        if (GameRefs.Hero is not { } hero || !hero.TryGetComponent(out MeshRenderer renderer))
        {
            return;
        }
        if (renderer.sortingLayerID == _sortingLayer && renderer.sortingOrder == _sortingOrder)
        {
            return;
        }
        _sortingLayer = renderer.sortingLayerID;
        _sortingOrder = renderer.sortingOrder;
        foreach (MeshRenderer child in _root.GetComponentsInChildren<MeshRenderer>(true))
        {
            child.sortingLayerID = _sortingLayer;
        }
        _body.GetComponent<MeshRenderer>().sortingOrder = _sortingOrder;
        _tag.GetComponent<MeshRenderer>().sortingOrder = _sortingOrder + 10;
    }

    private tk2dSprite NewSprite(string name, Transform parent)
    {
        GameObject go = new(name);
        go.transform.SetParent(parent, false);
        tk2dSprite sprite = go.AddComponent<tk2dSprite>();
        sprite.color = _tint;
        go.GetComponent<MeshRenderer>().sortingLayerID = _sortingLayer;
        return sprite;
    }

    private Transform NewTag(string name)
    {
        GameObject go = new("Name");
        go.transform.SetParent(_root.transform, false);
        TextMesh text = go.AddComponent<TextMesh>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.font = font;
        text.text = name;
        text.fontSize = 48;
        text.characterSize = 0.05f;
        text.anchor = TextAnchor.LowerCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;
        go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        return go.transform;
    }
}
