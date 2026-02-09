using UnityEngine;

namespace Quantum
{
    public unsafe class FlashlightView : QuantumEntityViewComponent
    {
        [SerializeField] private Light _light;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventFlashlightToggled>(this, OnEventFlashlightToggled);
            QuantumEvent.Subscribe<EventValuableDropped>(this, OnEventValuableDropped);
        }

        private void OnEventValuableDropped(EventValuableDropped e)
        {
            if (EntityRef != e.ValuableEntity) return;

            var frame = VerifiedFrame;

            frame.Unsafe.TryGetPointer<Flashlight>(e.ValuableEntity, out var flashlight);

            _light.enabled = flashlight->IsOn;
        }

        private void OnEventFlashlightToggled(EventFlashlightToggled e)
        {
            if (EntityRef != e.FlashlightEntity) return;

            _light.enabled = e.IsOn;
        }
    }
}
