using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;

public class TMPFontReplacer
{
    [MenuItem("Tools/Replace TMP Font (Current Scene)")]
    public static void ReplaceFont()
    {
        TMP_FontAsset newFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Art/Fonts/ZenDotsKir SDF.asset");

        if (newFont == null)
        {
            Debug.LogError("Font not found.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        int count = 0;

        foreach (GameObject root in roots)
        {
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text text in texts)
            {
                if (text.font != newFont)
                {
                    Undo.RecordObject(text, "Replace TMP Font");
                    text.font = newFont;
                    EditorUtility.SetDirty(text);
                    count++;
                }
            }
        }

        Debug.Log($"Replaced font on {count} TMP components in active scene.");
    }
}