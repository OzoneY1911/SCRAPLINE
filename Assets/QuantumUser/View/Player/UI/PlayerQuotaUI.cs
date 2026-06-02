using Quantum;
using TMPro;
using UnityEngine;

public class PlayerQuotaUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _quotaCountTMP;

    private int _completedQuotaCount;
    private int _totalQuotaCount;

    private void Awake()
    {
        _quotaCountTMP.enabled = false;
    }

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventMapChangedToProcedural>(this, OnMapChangedToProcedural);
        QuantumEvent.Subscribe<EventMapChangedToHub>(this, OnMapChangedToHub);
        QuantumEvent.Subscribe<EventQuotaZoneSpawned>(this, OnEventQuotaZoneSpawned);
        QuantumEvent.Subscribe<EventQuotaZoneCompleted>(this, OnEventQuotaZoneCompleted);
    }

    private void UpdateQuotaCount(int completedQuotaCount = 0)
    {
        _quotaCountTMP.text = $"{completedQuotaCount} / {_totalQuotaCount}";
    }

    private void OnMapChangedToProcedural(EventMapChangedToProcedural e)
    {
        _quotaCountTMP.enabled = true;
    }

    private void OnMapChangedToHub(EventMapChangedToHub e)
    {
        _quotaCountTMP.enabled = false;
        _completedQuotaCount = 0;
        _totalQuotaCount = 0;
        UpdateQuotaCount();
    }

    private void OnEventQuotaZoneSpawned(EventQuotaZoneSpawned e)
    {
        _totalQuotaCount++;
        UpdateQuotaCount();
    }

    private void OnEventQuotaZoneCompleted(EventQuotaZoneCompleted e)
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (frame == null) return;

        UpdateQuotaCount(++_completedQuotaCount);
    }
}
