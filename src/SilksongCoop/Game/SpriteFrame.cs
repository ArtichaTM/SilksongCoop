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
    int material = 0
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
        return new SpriteFrame(key, FlatSprites.Marker, sprite, at, scale, angle, renderer.color, material);
    }

    public void Write(PacketWriter writer)
    {
        writer.Int(Key).Int(Collection).Int(SpriteId)
            .Float(Position.x).Float(Position.y)
            .Float(Scale.x).Float(Scale.y)
            .Float(Angle)
            .Byte(Color.r).Byte(Color.g).Byte(Color.b).Byte(Color.a)
            .Int(Material);
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
        return new SpriteFrame(key, collection, spriteId, position, scale, angle, color, reader.Int());
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
