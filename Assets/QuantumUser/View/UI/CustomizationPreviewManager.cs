using Photon.Deterministic;
using Quantum;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationPreviewManager : PersistentSingletonMono<CustomizationPreviewManager>
{
    [SerializeField] private Transform _playerObject;
    [SerializeField] private Renderer _visorRenderer;

    private List<Renderer> _playerRenderers = new();

    private EmoteConfig _currentEmoteConfig;
    private FPVector3 _currentColorRGB;
    
    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < _playerObject.childCount; i++)
        {
            var renderer = _playerObject.GetChild(i).GetComponent<Renderer>();

            if (renderer == null) continue;

            _playerRenderers.Add(renderer);
        }
    }

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventPlayerSpawned>(this, OnPlayerSpawned);
    }

    private void OnPlayerSpawned(EventPlayerSpawned e)
    {
        var game = QuantumRunner.Default.Game;

        if (_currentEmoteConfig != null)
        {
            var emoteCommand = new CommandSetEmote()
            {
                EmoteConfig = _currentEmoteConfig
            };
            game.SendCommand(emoteCommand);
        }

        if (_currentColorRGB != FPVector3.Zero)
        {
            var colorCommand = new CommandSetPlayerColor()
            {
                ColorRGB = _currentColorRGB
            };
            game.SendCommand(colorCommand);
        }
    }

    public void SetEmote(EmoteConfig config)
    {
        _currentEmoteConfig = config;
        _visorRenderer.material.SetTexture("_Emote_Texture", config.Texture);
    }

    public void SetColor(Color color)
    {
        _currentColorRGB = new FPVector3(
            FP.FromFloat_UNSAFE(color.r),
            FP.FromFloat_UNSAFE(color.g),
            FP.FromFloat_UNSAFE(color.b)
            );

        foreach (var renderer in _playerRenderers)
        {
            renderer.material.SetColor("_BaseColor", color);
        }
    }
}
