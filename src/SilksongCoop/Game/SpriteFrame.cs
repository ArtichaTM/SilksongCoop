using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>One tk2d sprite as drawn this frame, in world space so the receiver needs no hierarchy.</summary>
internal readonly struct SpriteFrame(
    int key,
    int collection,
    int spriteId,
    Vector2 position,
    Vector2 scale,
    float angle,
    Color32 color,
    int material = 0,
    SpriteLayers layers = default
)
{
    public int Key { get; } = key;

    public int Collection { get; } = collection;

    public int SpriteId { get; } = spriteId;

    public Vector2 Position { get; } = position;

    public Vector2 Scale { get; } = scale;

    public float Angle { get; } = angle;

    /// <summary>The game fades many effects through this, alpha included.</summary>
    public Color32 Color { get; } = color;

    /// <summary>A Unity sprite's material by name hash (glows blend additively); 0 for tk2d.</summary>
    public int Material { get; } = material;

    /// <summary>Where a Unity sprite draws: glows sit on a lighting layer, not the hero's (3.1).</summary>
    public SpriteLayers Layers { get; } = layers;

    public static SpriteFrame Of(int key, tk2dBaseSprite sprite)
    {
        Transform transform = sprite.transform;
        int collection = SpriteCollections.Sent(sprite.Collection);
        Vector3 scale = transform.lossyScale;
        float angle = transform.eulerAngles.z;
        Vector2 at = transform.position;
        return new SpriteFrame(key, collection, sprite.spriteId, at, scale, angle, sprite.color);
    }

    public bool IsFlat => Collection == FlatSprites.Marker;

    /// <summary>A Unity sprite; flips fold into the scale.</summary>
    public static SpriteFrame Of(int key, SpriteRenderer renderer)
    {
        Transform transform = renderer.transform;
        Vector3 scale = transform.lossyScale;
        scale.x *= renderer.flipX ? -1f : 1f;
        scale.y *= renderer.flipY ? -1f : 1f;
        int sprite = FlatSprites.Sent(renderer.sprite);
        float angle = transform.eulerAngles.z;
        Vector2 at = transform.position;
        int material = FlatSprites.Sent(renderer.sharedMaterial);
        SpriteLayers layers = new(renderer.gameObject.layer, renderer.sortingLayerID, renderer.sortingOrder);
        Color32 color = renderer.color;
        return new SpriteFrame(key, FlatSprites.Marker, sprite, at, scale, angle, color, material, layers);
    }

    public void Write(PacketWriter writer)
    {
        writer.Int(Key).Int(Collection).Int(SpriteId)
            .Float(Position.x).Float(Position.y)
            .Float(Scale.x).Float(Scale.y)
            .Float(Angle)
            .Byte(Color.r).Byte(Color.g).Byte(Color.b).Byte(Color.a)
            .Int(Material)
            .Byte((byte)Layers.Layer).Int(Layers.SortingLayer).Int(Layers.Order);
    }

    public static SpriteFrame Read(PacketReader reader)
    {
        int key = reader.Int();
        int collection = reader.Int();
        int spriteId = reader.Int();
        Vector2 position = new(reader.Float(), reader.Float());
        Vector2 scale = new(reader.Float(), reader.Float());
        float angle = reader.Float();
        Color32 color = new(reader.Byte(), reader.Byte(), reader.Byte(), reader.Byte());
        int material = reader.Int();
        SpriteLayers layers = new(reader.Byte(), reader.Int(), reader.Int());
        return new SpriteFrame(key, collection, spriteId, position, scale, angle, color, material, layers);
    }

    /// <summary>Moves a sprite to this frame; false when the collection isn't loaded here.</summary>
    public bool ApplyTo(tk2dSprite sprite, float z, Color tint)
    {
        Place(sprite.transform, z);
        sprite.color = (Color)Color * tint;
        tk2dSpriteCollectionData? data = SpriteCollections.Find(Collection);
        if (data is null)
        {
            return false;
        }
        if (sprite.Collection != data || sprite.spriteId != SpriteId)
        {
            sprite.SetSprite(data, SpriteId);
        }
        return true;
    }

    /// <summary>Moves a Unity sprite to this frame; false when the sprite isn't loaded here.</summary>
    public bool ApplyTo(SpriteRenderer renderer, float z, Color tint)
    {
        Place(renderer.transform, z);
        renderer.color = (Color)Color * tint;
        if (FlatSprites.Find(SpriteId) is not { } sprite)
        {
            return false;
        }
        renderer.sprite = sprite;
        renderer.gameObject.layer = Layers.Layer;
        renderer.sortingLayerID = Layers.SortingLayer;
        renderer.sortingOrder = Layers.Order;
        if (FlatSprites.FindMaterial(Material) is { } material && renderer.sharedMaterial != material)
        {
            renderer.sharedMaterial = material;
        }
        FlatSprites.ReportDrawn(SpriteId, renderer, Color.a);
        return true;
    }

    private void Place(Transform transform, float z)
    {
        transform.position = new Vector3(Position.x, Position.y, z);
        transform.localScale = new Vector3(Scale.x, Scale.y, 1f);
        transform.rotation = Quaternion.Euler(0f, 0f, Angle);
    }
}

/// <summary>A renderer's Unity layer (which camera draws it), sorting layer and order.</summary>
internal readonly record struct SpriteLayers(int Layer, int SortingLayer, int Order);
