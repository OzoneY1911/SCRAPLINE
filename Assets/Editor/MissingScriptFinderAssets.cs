using UnityEngine;
using UnityEditor;

public class MissingScriptFinderAssets
{
    [MenuItem("Tools/Find Missing Scripts In Prefabs")]
    public static void FindMissingInPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        int missingCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);

            missingCount += FindMissingInGameObject(instance, path);

            PrefabUtility.UnloadPrefabContents(instance);
        }

        Debug.Log($"Total missing scripts in prefabs: {missingCount}");
    }

    private static int FindMissingInGameObject(GameObject go, string path)
    {
        int count = 0;

        Component[] components = go.GetComponents<Component>();

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                Debug.Log($"Missing script in prefab: {path} on GameObject: {GetFullPath(go)}");
                count++;
            }
        }

        foreach (Transform child in go.transform)
        {
            count += FindMissingInGameObject(child.gameObject, path);
        }

        return count;
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