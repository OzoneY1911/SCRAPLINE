using Quantum;
using TMPro;
using UnityEngine;

public unsafe class GameStateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dayCountTMP;

    private void Start()
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (frame == null) return;

        _dayCountTMP.text = $"Day {frame.Global->DayCount}";
    }
}
