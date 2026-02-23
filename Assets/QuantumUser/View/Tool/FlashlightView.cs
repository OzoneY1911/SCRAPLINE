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

            VerifiedFrame.Unsafe.TryGetPointer<Flashlight>(EntityRef, out var flashlight);

            SetFlashlight(EntityRef, flashlight->IsOn);
        }

        private void OnEventValuableDropped(EventValuableDropped e)
        {
            if (EntityRef != e.ValuableEntity) return;

            VerifiedFrame.Unsafe.TryGetPointer<Flashlight>(e.ValuableEntity, out var flashlight);

            SetFlashlight(e.ValuableEntity, flashlight->IsOn);
        }

        private void OnEventFlashlightToggled(EventFlashlightToggled e)
        {
            SetFlashlight(e.FlashlightEntity, e.IsOn);
        }

        private void SetFlashlight(EntityRef flashlightEntity, bool isOn)
        {
            if (EntityRef != flashlightEntity) return;

            _light.enabled = isOn;
        }
    }
}
