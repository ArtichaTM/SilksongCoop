using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FsmDump;

/// <summary>The active scene's hierarchy with each object's components, to find what moves a thing.</summary>
internal static class ObjectWriter
{
    public static int DumpScene(string path)
    {
        using StreamWriter writer = new(path);
        int count = 0;
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            count += Write(writer, root.transform, 0);
        }
        return count;
    }

    private static int Write(StreamWriter writer, Transform transform, int depth)
    {
        GameObject go = transform.gameObject;
        string components = string.Join(", ", go.GetComponents<Component>()
            .Where(component => component && component is not Transform)
            .Select(component => component.GetType().Name));
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
