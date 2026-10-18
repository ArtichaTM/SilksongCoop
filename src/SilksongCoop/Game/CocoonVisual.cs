using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// The coop revive cocoon (D15): a copy of the game's own cocoon corpse with everything but its visuals
/// removed, so it has no pickup, save or hit logic of its own.
/// </summary>
internal static class CocoonVisual
{
    public static GameObject Spawn(Vector2 at, string name)
    {
        GameObject holder = new($"SilksongCoop Cocoon ({name})");
        holder.SetActive(false);
        holder.transform.position = new Vector3(at.x, at.y, 0.01f);
        GameObject? prefab = GameRefs.Manager?.sm?.heroCorpsePrefab;
        if (prefab)
        {
            GameObject copy = Object.Instantiate(prefab!, holder.transform, false);
            copy.transform.localPosition = Vector3.zero;
            StripLogic(copy);
            KeepNormalAppearance(copy);
        }
        else
        {
            AddFallbackSprite(holder);
        }
        holder.SetActive(true);
        return holder;
    }

    private static void StripLogic(GameObject copy)
    {
        // Inactive parent: none of these ran Awake, so removing them leaves no half-started state.
        foreach (MonoBehaviour behaviour in copy.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (!behaviour.GetType().Name.StartsWith("tk2d"))
            {
                Remove(behaviour);
            }
        }
        foreach (Collider2D collider in copy.GetComponentsInChildren<Collider2D>(true))
        {
            Remove(collider);
        }
        foreach (Rigidbody2D body in copy.GetComponentsInChildren<Rigidbody2D>(true))
        {
            Remove(body);
        }
        foreach (AudioSource audio in copy.GetComponentsInChildren<AudioSource>(true))
        {
            Remove(audio);
        }
    }

    // The prefab ships Appearances off with every child on; the stripped corpse logic turned one on.
    private static void KeepNormalAppearance(GameObject copy)
    {
        copy.transform.Find("Core")?.gameObject.SetActive(false);
        if (copy.transform.Find("Appearances") is not { } appearances)
        {
            return;
        }
        appearances.gameObject.SetActive(true);
        foreach (Transform look in appearances)
        {
            look.gameObject.SetActive(look.name == "Normal");
        }
    }

    // A component another one requires stays (Unity logs it); it never ran, so it does nothing.
    private static void Remove(Component component) => Object.DestroyImmediate(component);

    private static void AddFallbackSprite(GameObject holder)
    {
        if (GameRefs.Hero is not { } hero || !hero.TryGetComponent(out tk2dSprite heroSprite))
        {
            return;
        }
        tk2dSprite sprite = holder.AddComponent<tk2dSprite>();
        sprite.SetSprite(heroSprite.Collection, heroSprite.spriteId);
        sprite.color = new Color(0.6f, 0.6f, 0.6f, 1f);
    }
}
