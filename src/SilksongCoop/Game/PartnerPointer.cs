using System.Collections.Generic;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// An arrow beside my Hornet, in the partner's tint, aiming at the partner or, from the next room, at the
/// door here they came in by. It fades in only once its target is off screen.
/// </summary>
internal sealed class PartnerPointer(Coop coop) : Module(coop)
{
    /// <summary>The arrow's object name; HeroSync leaves it out of Hornet's mirrored effects.</summary>
    public const string ArrowName = "SilksongCoop Partner Pointer";

    private const float Radius = 1.6f;
    private const float Scale = 0.3f;
    private const int Size = 64;
    private readonly List<Door> _doors = [];
    private readonly List<TransitionPoint> _points = [];
    private SpriteRenderer? _arrow;
    private Texture2D? _texture;
    private Vector2? _target;

    public override void LateUpdate()
    {
        HeroController? hero = GameRefs.Hero;
        Camera? camera = GameCameras.SilentInstance ? GameCameras.SilentInstance.mainCamera : null;
        bool active = Coop.InCoop && Coop.Config.ShowPointer.Value && Coop.Rooms.Current is not null;
        if (!active || hero is null || !camera)
        {
            _target = null;
            Hide();
            return;
        }
        Aim();
        if (_target is not { } target)
        {
            Hide();
            return;
        }
        Draw(hero, camera!, target);
    }

    public override void OnLeft() => _target = null;

    public override void Dispose()
    {
        if (_arrow)
        {
            Object.Destroy(_arrow!.gameObject);
        }
        if (_texture)
        {
            Object.Destroy(_texture);
        }
    }

    /// <summary>While the partner loads a room the last target stays until they arrive.</summary>
    private void Aim()
    {
        Partner partner = Coop.Partner;
        if (partner.Scene is null)
        {
            _target = partner.Heading is null ? null : _target;
            return;
        }
        _target = Coop.Rooms.PartnerHere ? partner.Position : DoorTo(partner);
    }

    private Vector2? DoorTo(Partner partner)
    {
        CollectDoors();
        int door = Pointer.DoorTo(_doors, partner.Scene!, partner.EntryGate);
        if (door < 0)
        {
            return null;
        }
        TransitionPoint point = _points[door];
        return point.collider ? point.collider.bounds.center : point.transform.position;
    }

    private void CollectDoors()
    {
        _doors.Clear();
        _points.Clear();
        foreach (TransitionPoint point in TransitionPoint.TransitionPoints ?? [])
        {
            if (point && !string.IsNullOrEmpty(point.targetScene))
            {
                _doors.Add(new Door(point.targetScene, point.entryPoint ?? ""));
                _points.Add(point);
            }
        }
    }

    private void Draw(HeroController hero, Camera camera, Vector2 target)
    {
        Transform self = hero.transform;
        // The collider's offset, not its bounds: bounds follow physics and trail the drawn Hornet.
        Vector2 centre = hero.TryGetComponent(out Collider2D body) ? body.offset : Vector2.zero;
        Vector2 toward = target - (Vector2)self.TransformPoint(centre);
        float alpha = Pointer.Alpha(PastScreenEdge(camera, target, hero));
        if (alpha <= 0f || toward.sqrMagnitude < 0.01f)
        {
            Hide();
            return;
        }
        SpriteRenderer arrow = Arrow(self);
        Vector2 direction = toward.normalized;
        Vector2 offset = self.InverseTransformVector(direction * Radius);
        Vector3 scale = self.lossyScale;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // Parented, so it moves with Hornet; dividing by her scale also undoes her facing flip.
        arrow.transform.localPosition = new Vector3(centre.x + offset.x, centre.y + offset.y, -0.01f);
        arrow.transform.localScale = new Vector3(Scale / scale.x, Scale / scale.y, 1f);
        arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        Color tint = Coop.Config.Tint.Value;
        arrow.color = new Color(tint.r, tint.g, tint.b, tint.a * alpha);
        if (hero.TryGetComponent(out MeshRenderer heroRenderer))
        {
            arrow.sortingLayerID = heroRenderer.sortingLayerID;
            arrow.sortingOrder = heroRenderer.sortingOrder + 20;
        }
        arrow.gameObject.SetActive(true);
    }

    /// <summary>World units from <paramref name="target"/> to the visible area on the hero's plane.</summary>
    private static float PastScreenEdge(Camera camera, Vector2 target, HeroController hero)
    {
        float depth = hero.transform.position.z - camera.transform.position.z;
        Vector3 min = camera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
        Vector3 max = camera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
        float dx = Mathf.Max(min.x - target.x, 0f, target.x - max.x);
        float dy = Mathf.Max(min.y - target.y, 0f, target.y - max.y);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private void Hide()
    {
        if (_arrow)
        {
            _arrow!.gameObject.SetActive(false);
        }
    }

    private SpriteRenderer Arrow(Transform hero)
    {
        if (!_arrow)
        {
            GameObject go = new(ArrowName);
            _arrow = go.AddComponent<SpriteRenderer>();
            _arrow.sprite = Arrowhead();
        }
        if (_arrow!.transform.parent != hero)
        {
            _arrow.transform.SetParent(hero, false);
        }
        return _arrow;
    }

    private Sprite Arrowhead()
    {
        _texture = _texture ? _texture : DrawArrowhead();
        Sprite sprite = Sprite.Create(_texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        sprite.name = ArrowName;
        return sprite;
    }

    /// <summary>A white arrowhead pointing right with a notched back, so the tint colours it.</summary>
    private static Texture2D DrawArrowhead()
    {
        Texture2D texture = new(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        Color32[] pixels = new Color32[Size * Size];
        Color32 solid = new(255, 255, 255, 255);
        Color32 clear = new(255, 255, 255, 0);
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float u = (x + 0.5f) / Size;
                float v = Mathf.Abs((y + 0.5f) / Size * 2f - 1f);
                bool inside = v <= 1f - u && u >= 0.4f * v;
                pixels[y * Size + x] = inside ? solid : clear;
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }
}
