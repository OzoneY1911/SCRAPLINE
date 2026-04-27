using Quantum;
using UnityEngine;

public class EmoteButton : MonoBehaviour
{
    [SerializeField] private EmoteConfig _emoteConfig;

    public void SetEmote()
    {
        var command = new CommandSetEmote()
        {
            EmoteConfig = _emoteConfig
        };

        QuantumRunner.Default.Game.SendCommand(command);

        CustomizationPreviewManager.Instance.SetEmote(_emoteConfig.Texture);
    }
}
