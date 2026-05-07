using System.Collections.Generic;
using Quantum;
using TMPro;
using UnityEngine;

public unsafe class QuotaZoneUIView : QuantumEntityViewComponent
{
    [Header("Helper Text")]
    [SerializeField] private TextMeshProUGUI _stateLabelTMP;
    [SerializeField] private TextMeshProUGUI _controlHintTMP;

    [Header("Resource Demands")]
    [SerializeField] private GameObject _resourceDemandsObject;
    [SerializeField] private TextMeshProUGUI _resourceDemandsTMP;

    [Header("Progress Bar")]
    [SerializeField] private Transform _progressBarContainer;
    [SerializeField] private GameObject _progressBarPrefab;

    private List<QuotaZoneProgressBar> _progressBars = new();

    public override void OnActivate(Frame frame)
    {
        QuantumEvent.Subscribe<EventQuotaZoneUpdated>(this, OnEventQuotaZoneUpdated);
        QuantumEvent.Subscribe<EventQuotaZoneActivated>(this, OnEventQuotaZoneActivated);
        QuantumEvent.Subscribe<EventQuotaZoneCompleted>(this, OnEventQuotaZoneCompleted);
    }

    private void OnEventQuotaZoneActivated(EventQuotaZoneActivated e)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;
        var frame = game.Frames.Verified;
        if (frame == null) return;

        if (!EntityUtils.EntityIsInGroup(frame, EntityRef, e.Entity)) return;

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

            _stateLabelTMP.gameObject.SetActive(false);
            _controlHintTMP.text = "TAP SCREEN TO COMPLETE";
            _resourceDemandsObject.SetActive(true);
            _resourceDemandsTMP.text = demandsText;

            QuantumEvent.UnsubscribeListener<EventQuotaZoneActivated>(this);
        }
    }

    private void OnEventQuotaZoneUpdated(EventQuotaZoneUpdated e)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;
        var frame = game.Frames.Verified;
        if (frame == null) return;
        if (!EntityUtils.EntityIsInGroup(frame, EntityRef, e.Entity)) return;

        UpdateProgressBars(e.Entity);
    }

    private void UpdateProgressBars(EntityRef entity)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;
        var frame = game.Frames.Predicted;
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

    private void OnEventQuotaZoneCompleted(EventQuotaZoneCompleted e)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;
        var frame = game.Frames.Verified;
        if (frame == null) return;
        if (!EntityUtils.EntityIsInGroup(frame, EntityRef, e.Entity)) return;

        foreach (var progressBar in _progressBars)
        {
            Destroy(progressBar.gameObject);
        }

        _stateLabelTMP.gameObject.SetActive(true);
        _stateLabelTMP.text = "COMPLETED";
        _stateLabelTMP.color = new Color(0f, .5f, 0f);

        _controlHintTMP.gameObject.SetActive(false);
        _resourceDemandsObject.SetActive(false);

        QuantumEvent.UnsubscribeListener<EventQuotaZoneActivated>(this);
    }
}
