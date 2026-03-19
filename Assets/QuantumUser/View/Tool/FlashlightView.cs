using UnityEngine;

namespace Quantum
{
    public unsafe class FlashlightView : QuantumEntityViewComponent
    {
        [Header("Light Settings")]
        [SerializeField] private Light _light;

        [Header("Glass Settings")]
        [SerializeField] private Renderer _glassRenderer;

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
            if (!VerifiedFrame.Unsafe.TryGetPointer<Flashlight>(flashlightEntity, out var flashlight)) return;

            var flashlightConfig = VerifiedFrame.FindAsset<FlashlightConfig>(flashlight->Config);
            var flashlightMaterials = _glassRenderer.sharedMaterials;

            _light.enabled = isOn;

            flashlightMaterials[1] = isOn
                ? flashlightConfig.IsOnMaterial
                : flashlightConfig.IsOffMaterial;

            _glassRenderer.sharedMaterials = flashlightMaterials;
        }
    }
}
