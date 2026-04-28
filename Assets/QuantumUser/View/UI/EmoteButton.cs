using Quantum;
using UnityEngine;

public class EmoteButton : MonoBehaviour
{
    [SerializeField] private EmoteConfig _emoteConfig;

    public void SetEmote()
    {
        CustomizationPreviewManager.Instance.SetEmote(_emoteConfig);

        if (QuantumRunner.Default == null) return;

        var command = new CommandSetEmote()
        {
            EmoteConfig = _emoteConfig
        };
        QuantumRunner.Default.Game.SendCommand(command);
    }
}
