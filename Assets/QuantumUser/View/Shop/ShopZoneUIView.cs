using TMPro;
using UnityEngine;

namespace Quantum
{
    public unsafe class ShopZoneUIView : QuantumEntityViewComponent
    {
        [Header("Shop Zone Value")]
        [SerializeField] private TextMeshProUGUI _shopZoneValueTMP;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventShopZoneUpdated>(this, OnEventShopZoneUpdated);
        }

        private void OnEventShopZoneUpdated(EventShopZoneUpdated e)
        {
            if (e.Entity != EntityRef) return;

            var game = QuantumRunner.Default.Game;
            if (game == null) return;

            var frame = game.Frames.Predicted;
            if (frame == null) return;

            if (frame.Unsafe.TryGetPointer<ShopZone>(e.Entity, out var shopZone))
            {
                _shopZoneValueTMP.text = shopZone->InZoneValue.ToString();
            }
        }
    }
}
