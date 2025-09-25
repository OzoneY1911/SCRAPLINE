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
        QuantumEvent.Subscribe<EventQuotaZoneUpdated>(this, OnEventQuotaZoneUpdated);
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

    private void OnEventQuotaZoneUpdated(EventQuotaZoneUpdated e) => UpdateProgressBars(e.Entity);

    private void UpdateProgressBars(EntityRef entity)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        var frame = game.Frames.Verified;
        if (frame == null) return;

        if (frame.Unsafe.TryGetPointer<QuotaZone>(entity, out var quotaZone))
        {
            var resourceDemands = frame.ResolveList<ResourceDemand>(quotaZone->ResourceDemands);

            for (int i = 0; i < _progressBars.Count; i++)
            {
                _progressBars[i].CurrentValue = resourceDemands[i].Collected.AsFloat;
                _progressBars[i].BarImage.fillAmount = _progressBars[i].CurrentValue / _progressBars[i].MaxValue;
            }
        }
    }
}
