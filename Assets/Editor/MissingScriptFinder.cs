using UnityEngine;
using UnityEditor;

public class MissingScriptFinder
{
    [MenuItem("Tools/Find Missing Scripts In Scene")]
    public static void FindMissing()
    {
        GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

        int count = 0;

        foreach (GameObject go in objects)
        {
            Component[] components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.Log($"Missing script on: {GetFullPath(go)}", go);
                    count++;
                }
            }
        }

        Debug.Log($"Total missing scripts: {count}");
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        while (go.transform.parent != null)
        {
            go = go.transform.parent.gameObject;
            path = go.name + "/" + path;
        }
        return path;
    }
}