using Steamworks;
using Steamworks.Data;
using System;
using System.Threading.Tasks;
using UnityEngine;

public static class SteamworksUtils
{
    public static async Task<Texture2D> GetAvatarTextureAsync(SteamId steamId)
    {
        try
        {
            Image? image = await SteamFriends.GetLargeAvatarAsync(steamId);
            if (image == null) return null;
            return image.Value.Convert();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    private static Texture2D Convert(this Image image)
    {
        Texture2D avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);

        avatar.filterMode = FilterMode.Trilinear;

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x, y);

                avatar.SetPixel(
                    x,
                    (int)image.Height - y,
                    new UnityEngine.Color(p.r / 255f, p.g / 255f, p.b / 255f, p.a / 255f));
            }
        }

        avatar.Apply();
        return avatar;
    }
}