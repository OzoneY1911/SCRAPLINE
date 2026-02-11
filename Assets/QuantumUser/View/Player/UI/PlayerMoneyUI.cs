using TMPro;
using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerMoneyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _moneyTMP;

        private void Awake()
        {
            _moneyTMP.text = "0";
        }

        private void OnEnable()
        {
            QuantumEvent.Subscribe<EventPlayerMoneyUpdated>(this, OnEventPlayerMoneyUpdated);
        }

        private void OnEventPlayerMoneyUpdated(EventPlayerMoneyUpdated e)
        {
            var frame = QuantumRunner.Default.Game.Frames.Verified;

            if (frame == null) return;

            _moneyTMP.text = frame.Global->PlayerMoney.ToString();
        }
    }
}
