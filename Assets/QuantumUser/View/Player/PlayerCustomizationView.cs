using Photon.Deterministic;
using Quantum;
using System.Collections.Generic;
using UnityEngine;

public unsafe class PlayerCustomizationView : QuantumEntityViewComponent
{
    [SerializeField] private Transform _playerObject;
    [SerializeField] private Transform _playerFPVObject;
    [SerializeField] private Renderer _visorRenderer;

    private List<Renderer> _playerColorableRenderers = new();
    private List<Renderer> _playerFPVRenderers = new();

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventPlayerEmoteChanged>(this, OnEventPlayerEmoteChanged);
        QuantumEvent.Subscribe<EventPlayerColorChanged>(this, OnEventPlayerColorChanged);
    }

    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);

        _playerColorableRenderers.Clear();
        _playerFPVRenderers.Clear();

        for (int i = 0; i < _playerObject.childCount; i++)
        {
            var renderer = _playerObject.GetChild(i).GetComponent<Renderer>();

            if (renderer == null || !renderer.gameObject.CompareTag("Colorable")) continue;

            _playerColorableRenderers.Add(renderer);
        }

        for (int i = 0; i < _playerFPVObject.childCount; i++)
        {
            var renderer = _playerFPVObject.GetChild(i).GetComponent<Renderer>();

            if (renderer == null) continue;

            _playerFPVRenderers.Add(renderer);
        }

        if (!frame.Unsafe.TryGetPointer<Player>(EntityRef, out var player)) return;

        if (player->EmoteConfig.IsValid)
        {
            ApplyEmote(frame.FindAsset<EmoteConfig>(player->EmoteConfig));
        }

        ApplyColor(player->ColorRGB);
    }

    private void OnEventPlayerEmoteChanged(EventPlayerEmoteChanged e)
    {
        if (e.PlayerEntity != EntityRef) return;
        var frame = e.Game.Frames.Verified;

        ApplyEmote(frame.FindAsset<EmoteConfig>(e.EmoteConfig));
    }

    private void OnEventPlayerColorChanged(EventPlayerColorChanged e)
    {
        if (e.PlayerEntity != EntityRef) return;

        ApplyColor(e.ColorRGB);
    }

    private void ApplyColor(FPVector3 colorRGB)
    {
        var newColor = new Color(
            colorRGB.X.AsFloat,
            colorRGB.Y.AsFloat,
            colorRGB.Z.AsFloat
            );

        foreach (var renderer in _playerColorableRenderers)
        {
            renderer.materials[0].SetColor("_BaseColor", newColor);
        }

        foreach (var renderer in _playerFPVRenderers)
        {
            renderer.materials[0].SetColor("_BaseColor", newColor);
        }
    }

    private void ApplyEmote(EmoteConfig config)
    {
        _visorRenderer.material.SetTexture("_Emote_Texture", config.Texture);
    }
}
