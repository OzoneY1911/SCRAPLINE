using UnityEngine;

public class CustomizationPreviewManager : PersistentSingletonMono<CustomizationPreviewManager>
{
    [SerializeField] private Renderer _visorRenderer;

    public void SetEmote(Texture2D texture)
    {
        _visorRenderer.material.SetTexture("_Emote_Texture", texture);
    }
}
