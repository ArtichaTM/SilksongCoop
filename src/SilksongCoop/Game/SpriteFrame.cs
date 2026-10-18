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
    float angle
)
{
    public int Key { get; } = key;

    public int Collection { get; } = collection;

    public int SpriteId { get; } = spriteId;

    public Vector2 Position { get; } = position;

    public Vector2 Scale { get; } = scale;

    public float Angle { get; } = angle;

    public static SpriteFrame Of(int key, tk2dBaseSprite sprite)
    {
        Transform transform = sprite.transform;
        int collection = SpriteCollections.Sent(sprite.Collection);
        Vector3 scale = transform.lossyScale;
        float angle = transform.eulerAngles.z;
        return new SpriteFrame(key, collection, sprite.spriteId, transform.position, scale, angle);
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
        return new SpriteFrame(key, FlatSprites.Marker, sprite, transform.position, scale, angle);
    }

    public void Write(PacketWriter writer)
    {
        writer.Int(Key).Int(Collection).Int(SpriteId)
            .Float(Position.x).Float(Position.y)
            .Float(Scale.x).Float(Scale.y)
            .Float(Angle);
    }

    public static SpriteFrame Read(PacketReader reader)
    {
        int key = reader.Int();
        int collection = reader.Int();
        int spriteId = reader.Int();
        Vector2 position = new(reader.Float(), reader.Float());
        Vector2 scale = new(reader.Float(), reader.Float());
        return new SpriteFrame(key, collection, spriteId, position, scale, reader.Float());
    }

    /// <summary>Moves a sprite to this frame; false when the collection isn't loaded here.</summary>
    public bool ApplyTo(tk2dSprite sprite, float z)
    {
        Place(sprite.transform, z);
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
    public bool ApplyTo(SpriteRenderer renderer, float z)
    {
        Place(renderer.transform, z);
        if (FlatSprites.Find(SpriteId) is not { } sprite)
        {
            return false;
        }
        renderer.sprite = sprite;
        return true;
    }

    private void Place(Transform transform, float z)
    {
        transform.position = new Vector3(Position.x, Position.y, z);
        transform.localScale = new Vector3(Scale.x, Scale.y, 1f);
        transform.rotation = Quaternion.Euler(0f, 0f, Angle);
    }
}
