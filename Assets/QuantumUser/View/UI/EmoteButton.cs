using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class EmoteButton : MonoBehaviour
{
    [SerializeField] private EmoteConfig _emoteConfig;

    private void Start()
    {
        if (!PlayerCustomizationUtils.TryGetPlayerEmoteConfigGuid(out var emoteConfigGuid)) return;
        if (_emoteConfig.Guid != emoteConfigGuid) return;

        GetComponent<Toggle>().isOn = true;
    }

    public void SetEmote()
    {
        CustomizationPreviewManager.Instance.SetEmote(_emoteConfig);
        PlayerCustomizationUtils.SavePlayerEmote(_emoteConfig.Guid);

        if (QuantumRunner.Default == null) return;

        var command = new CommandSetEmote()
        {
            EmoteConfig = _emoteConfig
        };
        QuantumRunner.Default.Game.AddCommand(command);
    }
}
