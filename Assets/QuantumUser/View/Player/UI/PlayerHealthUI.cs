using Quantum;
using TMPro;
using UnityEngine;

public unsafe class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _healthTMP;

    private void Update()
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;

        if (frame == null) return;

        var localPlayers = QuantumRunner.Default.Game.GetLocalPlayers();

        if (localPlayers.Count == 0) return;

        var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

        if (activePlayers.TryGetValue(localPlayers[0], out var localPlayerEntity))
        {
            if (frame.TryGet(localPlayerEntity, out Health health))
            {
                _healthTMP.text = health.Current.AsInt.ToString();
            }
        }
    }
}
