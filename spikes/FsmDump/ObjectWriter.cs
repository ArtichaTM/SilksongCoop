using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FsmDump;

/// <summary>The active scene's hierarchy with each object's components, to find what moves a thing.</summary>
internal static class ObjectWriter
{
    public static int DumpScene(string path) => DumpScene(SceneManager.GetActiveScene(), path);

    public static int DumpScene(Scene scene, string path)
    {
        using StreamWriter writer = new(path);
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            count += Write(writer, root.transform, 0);
        }
        return count;
    }

    /// <summary>Component type name plus the few fields that tell what a sync audit needs.</summary>
    private static string Describe(Component component)
    {
        string name = component.GetType().Name;
        try
        {
            return component switch
            {
                HealthManager health => $"{name}(hp={health.hp})",
                TransitionPoint gate => $"{name}(->{gate.targetScene}:{gate.entryPoint})",
                IPersistentItem item => $"{name}(id={item.GetId()})",
                _ => name,
            };
        }
        catch (Exception)
        {
            return name;
        }
    }

    private static int Write(StreamWriter writer, Transform transform, int depth)
    {
        GameObject go = transform.gameObject;
        string components = string.Join(", ", go.GetComponents<Component>()
            .Where(component => component && component is not Transform)
            .Select(Describe));
        Vector3 at = transform.position;
        string active = go.activeInHierarchy ? "" : " (inactive)";
        string indent = new(' ', depth * 2);
        writer.WriteLine($"{indent}{go.name}{active} @({at.x:0.0}, {at.y:0.0}) [{components}]");
        int count = 1;
        foreach (Transform child in transform)
        {
            count += Write(writer, child, depth + 1);
        }
        return count;
    }
}
