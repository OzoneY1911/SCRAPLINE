using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public static class PlayerCustomizationUtils
    {
        private const string PlayerColorRKey = "PlayerColorR";
        private const string PlayerColorGKey = "PlayerColorG";
        private const string PlayerColorBKey = "PlayerColorB";

        private const string PlayerEmoteKey = "PlayerEmote";

        public static void SavePlayerEmote(AssetGuid configGuid)
        {
            PlayerPrefs.SetString(PlayerEmoteKey, configGuid.Value.ToString());
        }

        public static bool TryGetPlayerEmoteConfigGuid(out AssetGuid guid)
        {
            if (!PlayerPrefs.HasKey(PlayerEmoteKey))
            {
                guid = AssetGuid.Invalid;
                return false;
            }

            string guidString = PlayerPrefs.GetString(PlayerEmoteKey);

            if (!long.TryParse(guidString, out var guidValue))
            {
                guid = AssetGuid.Invalid;
                return false;
            }

            guid = new AssetGuid(guidValue);

            return true;
        }

        public static void SavePlayerColor(Color color)
        {
            PlayerPrefs.SetFloat(PlayerColorRKey, color.r);
            PlayerPrefs.SetFloat(PlayerColorGKey, color.g);
            PlayerPrefs.SetFloat(PlayerColorBKey, color.b);
        }

        public static FPVector3 GetPlayerColorRGB()
        {
            if (!PlayerPrefs.HasKey(PlayerColorRKey))
            {
                return new FPVector3(1, 1, 1);
            }

            return new FPVector3(
                FP.FromFloat_UNSAFE(PlayerPrefs.GetFloat(PlayerColorRKey)),
                FP.FromFloat_UNSAFE(PlayerPrefs.GetFloat(PlayerColorGKey)),
                FP.FromFloat_UNSAFE(PlayerPrefs.GetFloat(PlayerColorBKey))
            );
        }

        public static Color GetPlayerColor()
        {
            if (!PlayerPrefs.HasKey(PlayerColorRKey))
            {
                return Color.white;
            }

            return new Color(
                PlayerPrefs.GetFloat(PlayerColorRKey),
                PlayerPrefs.GetFloat(PlayerColorGKey),
                PlayerPrefs.GetFloat(PlayerColorBKey)
                );
        }
    }
}
