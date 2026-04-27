using UnityEngine;
using TMPro;
using UnityEditor;

public class TMPFontReplacer
{
    [MenuItem("Tools/Replace TMP Font")]
    public static void ReplaceFont()
    {
        TMP_FontAsset newFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Art/Fonts/ZenDotsKir SDF");

        string[] guids = AssetDatabase.FindAssets("t:Prefab t:Scene");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            GameObject go = asset as GameObject;
            if (go == null) continue;

            TMP_Text[] texts = go.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text text in texts)
            {
                text.font = newFont;
                EditorUtility.SetDirty(text);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Font replacement done.");
    }
}