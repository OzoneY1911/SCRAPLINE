using System.Collections.Generic;
using Quantum;
using TMPro;
using UnityEngine;

public unsafe class QuotaZoneUI : MonoBehaviour
{
    [Header("Resource Demand")]
    [SerializeField] private TextMeshPro _resourceDemandsTMP;

    [Header("Quota Zone Progress Bar")]
    [SerializeField] private Transform _progressBarContainer;
    [SerializeField] private GameObject _progressBarPrefab;

    private List<QuotaZoneProgressBar> _progressBars = new();

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventQuotaZoneInitialized>(this, OnEventQuotaZoneInitialized);
        QuantumEvent.Subscribe<EventValuableEnter>(this, OnEventValuableEnter);
        QuantumEvent.Subscribe<EventValuableExit>(this, OnEventValuableExit);
    }

    private void OnEventQuotaZoneInitialized(EventQuotaZoneInitialized e)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        var frame = game.Frames.Verified;
        if (frame == null) return;

        if (frame.Unsafe.TryGetPointer<QuotaZone>(e.Entity, out var quotaZone))
        {
            var resourceDemands = frame.ResolveList<ResourceDemand>(quotaZone->ResourceDemands);

            var demandsText = "";
            foreach (var demand in resourceDemands)
            {
                demandsText += $"{demand.Type}: {demand.Value}\n";

                var progressBar = Instantiate(_progressBarPrefab, _progressBarContainer).GetComponent<QuotaZoneProgressBar>();

                progressBar.Type = demand.Type;
                progressBar.TextTMP.text = demand.Type.ToString();
                progressBar.MaxValue = demand.Value.AsFloat;
                _progressBars.Add(progressBar);
            }
            _resourceDemandsTMP.text = demandsText;

            QuantumEvent.UnsubscribeListener<EventQuotaZoneInitialized>(this);
        }
    }

    private void OnEventValuableEnter(EventValuableEnter e) => UpdateProgressBars(e.Entity, true);

    private void OnEventValuableExit(EventValuableExit e) => UpdateProgressBars(e.Entity, false);

    private void UpdateProgressBars(EntityRef entity, bool isIncremental)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        var frame = game.Frames.Verified;
        if (frame == null) return;

        if (frame.Unsafe.TryGetPointer<Valuable>(entity, out var valuable))
        {
            var resourceFractions = frame.ResolveList<ResourceFraction>(valuable->ResourceFractions);

            foreach (var resourceFraction in resourceFractions)
            {
                foreach (var progressBar in _progressBars)
                {
                    if (progressBar.Type == resourceFraction.Type)
                    {
                        if (isIncremental)
                        {
                            progressBar.CurrentValue += resourceFraction.Value.AsFloat * valuable->CurrentValue.AsFloat;
                        }
                        else
                        {
                            progressBar.CurrentValue -= resourceFraction.Value.AsFloat * valuable->CurrentValue.AsFloat;
                        }
                        progressBar.BarImage.fillAmount = progressBar.CurrentValue / progressBar.MaxValue;
                    }
                }
            }
        }
    }
}
