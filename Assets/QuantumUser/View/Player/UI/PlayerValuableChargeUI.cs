using Quantum;
using UnityEngine;
using UnityEngine.UI;

public unsafe class PlayerValuableChargeUI : MonoBehaviour
{
    [SerializeField] private Slider _chargeSlider;

    private void Update()
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;
        var frame = game.Frames.Verified;
        if (frame == null) return;
        var localPlayers = QuantumRunner.Default.Game.GetLocalPlayers();
        if (localPlayers.Count == 0) return;

        var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

        if (activePlayers.TryGetValue(localPlayers[0], out var localPlayerEntity))
        {
            if (frame.Unsafe.TryGetPointer(localPlayerEntity, out PlayerInventory* inventory))
            {
                if (!PlayerInventoryUtils.IsSlotSelected(inventory) || PlayerInventoryUtils.IsSelectedSlotEmpty(inventory))
                {
                    _chargeSlider.gameObject.SetActive(false);
                    return;
                }
                else
                {
                    _chargeSlider.gameObject.SetActive(true);
                }

                if (!frame.Unsafe.TryGetPointer<Flashlight>(inventory->Slots[(int)inventory->SelectedSlotIndex], out var flashlight)) return;

                _chargeSlider.value = (flashlight->CurrentCharge / frame.FindAsset(flashlight->Config).MaxCharge).AsFloat;
            }
        }
    }
}
