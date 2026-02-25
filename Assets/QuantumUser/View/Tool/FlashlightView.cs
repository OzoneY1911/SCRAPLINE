using UnityEngine;

namespace Quantum
{
    public unsafe class FlashlightView : QuantumEntityViewComponent
    {
        [Header("Light Settings")]
        [SerializeField] private Light _light;

        [Header("Glass Settings")]
        [SerializeField] private Renderer _glassRenderer;
        [SerializeField] private Material _isOffMaterial;
        [SerializeField] private Material _isOnMaterial;

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

            var flashlightMaterials = _glassRenderer.sharedMaterials;

            flashlightMaterials[1] = isOn
                ? _isOnMaterial
                : _isOffMaterial;

            _glassRenderer.sharedMaterials = flashlightMaterials;
        }
    }
}
