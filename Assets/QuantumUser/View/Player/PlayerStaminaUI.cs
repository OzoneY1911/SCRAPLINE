using Quantum;
using TMPro;
using UnityEngine;

public unsafe class PlayerStaminaUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _staminaTMP;

    private void Update()
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;

        if (frame == null) return;

        var localPlayerRef = QuantumRunner.Default.Game.GetLocalPlayers()[0];

        var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

        if (activePlayers.TryGetValue(localPlayerRef, out var localPlayerEntity))
        {
            if (frame.TryGet(localPlayerEntity, out PlayerStamina stamina))
            {
                _staminaTMP.text = stamina.Current.AsInt.ToString();
            }
        }
    }
}
