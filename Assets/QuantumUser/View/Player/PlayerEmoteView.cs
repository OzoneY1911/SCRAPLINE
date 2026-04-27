using Quantum;
using UnityEngine;

public class PlayerEmoteView : QuantumEntityViewComponent
{
    [SerializeField] private Renderer _visorRenderer;

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventPlayerEmoteChanged>(this, OnEventPlayerEmoteChanged);
    }

    private void OnEventPlayerEmoteChanged(EventPlayerEmoteChanged e)
    {
        if (e.PlayerEntity != EntityRef) return;
        var frame = e.Game.Frames.Verified;

        var emoteConfig = frame.FindAsset<EmoteConfig>(e.EmoteConfig);
        _visorRenderer.material.SetTexture("_Emote_Texture", emoteConfig.Texture);
    }
}
