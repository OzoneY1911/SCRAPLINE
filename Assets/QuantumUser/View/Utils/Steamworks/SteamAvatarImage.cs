using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class SteamAvatarImage : MonoBehaviour
{
    private async void Start()
    {
        Texture2D avatarTexture =
            await SteamworksUtils.GetAvatarTextureAsync(SteamClient.SteamId);

        if (avatarTexture == null) return;

        Sprite avatarSprite = Sprite.Create(
            avatarTexture,
            new Rect(0f, 0f, avatarTexture.width, avatarTexture.height),
            new Vector2(.5f, .5f));

        GetComponent<Image>().sprite = avatarSprite;
    }
}