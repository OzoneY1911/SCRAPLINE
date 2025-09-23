using Photon.Deterministic;
using Quantum;
using TMPro;
using UnityEngine;

public unsafe class QuotaZoneUI : MonoBehaviour
{
    [SerializeField] private TextMeshPro _resourceDemandsTMP;
    [SerializeField] private TextMeshPro _currentResourcesTMP;

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventValuableEnter>(this, OnEventValuableEnter);
    }

    private void OnEventValuableEnter(EventValuableEnter e)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        var frame = game.Frames.Verified;
        if (frame == null) return;

        if (frame.Unsafe.TryGetPointer<Valuable>(e.Entity, out var valuable))
        {
            var resources = frame.ResolveDictionary<ResourceType, FP>(valuable->Resources);

            string text = "";
            foreach (var resource in resources)
            {
                text += $"{resource.Key}: {(int)System.Math.Round((resource.Value * valuable->CurrentValue).AsFloat)}\n";
            }
            _currentResourcesTMP.text = text;
        }
    }
}
