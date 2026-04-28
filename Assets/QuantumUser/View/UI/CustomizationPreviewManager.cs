using Quantum;
using UnityEngine;

public class CustomizationPreviewManager : PersistentSingletonMono<CustomizationPreviewManager>
{
    [SerializeField] private Renderer _visorRenderer;

    private EmoteConfig _currentEmoteConfig;

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventPlayerSpawned>(this, OnPlayerSpawned);
    }

    private void OnPlayerSpawned(EventPlayerSpawned e)
    {
        if (_currentEmoteConfig == null) return; 

        var command = new CommandSetEmote()
        {
            EmoteConfig = _currentEmoteConfig
        };
        QuantumRunner.Default.Game.SendCommand(command);
    }

    public void SetEmote(EmoteConfig config)
    {
        _currentEmoteConfig = config;
        _visorRenderer.material.SetTexture("_Emote_Texture", config.Texture);
    }
}
