using Photon.Deterministic;

namespace Quantum
{
    public unsafe class FlashlightSystem : SystemMainThreadFilter<FlashlightSystem.Filter>, ISignalOnValuableUseRequested
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Flashlight* Flashlight;
        }

        public void OnValuableUseRequested(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            ToggleFlashlight(frame, valuableEntity);
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Flashlight->IsOn && filter.Flashlight->CurrentCharge > 0)
            {
                DischargeFlashlight(frame, filter.Flashlight);
            }
        }

        private void DischargeFlashlight(Frame frame, Flashlight* flashlight)
        {
            var config = frame.FindAsset<FlashlightConfig>(flashlight->Config);

            flashlight->CurrentCharge -= config.DischargePerSecond * frame.DeltaTime;

            if (flashlight->CurrentCharge <= FP._0)
            {
                flashlight->CurrentCharge = FP._0;
                flashlight->IsOn = false;
            }
        }

        private void ToggleFlashlight(Frame frame, EntityRef flashlightEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Flashlight>(flashlightEntity, out var flashlight)) return;

            if (!flashlight->IsOn && flashlight->CurrentCharge <= 0) return;

            flashlight->IsOn = !flashlight->IsOn;
        }
    }
}
