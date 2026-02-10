using Photon.Deterministic;

namespace Quantum
{
    public unsafe class FlashlightSystem : SystemMainThreadFilter<FlashlightSystem.Filter>, ISignalOnComponentAdded<Flashlight>, ISignalOnValuableCollected, ISignalOnValuableUseRequested, ISignalOnInventorySlotDeselected
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Flashlight* Flashlight;
        }

        public void OnAdded(Frame frame, EntityRef entity, Flashlight* flashlight)
        {
            var config = frame.FindAsset<FlashlightConfig>(flashlight->Config);
            flashlight->CurrentCharge = config.MaxCharge;
        }

        public void OnInventorySlotDeselected(Frame frame, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Flashlight>(valuableEntity, out var flashlight)) return;

            if (flashlight->IsOn) ToggleFlashlight(frame, valuableEntity);
        }

        public void OnValuableCollected(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Flashlight>(valuableEntity, out var flashlight)) return;
            if (!frame.Unsafe.TryGetPointer<PlayerInventory>(playerEntity, out var playerInventory)) return;

            if (playerInventory->SelectedSlotIndex == SlotIndex.None || valuableEntity != playerInventory->Slots[(int)playerInventory->SelectedSlotIndex])
            {
                if (flashlight->IsOn) ToggleFlashlight(frame, valuableEntity);
            }
        }

        public void OnValuableUseRequested(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            ToggleFlashlight(frame, valuableEntity);
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Flashlight->IsOn && filter.Flashlight->CurrentCharge > 0)
            {
                DischargeFlashlight(frame, filter.Entity);
            }
        }

        private void DischargeFlashlight(Frame frame, EntityRef flashlightEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Flashlight>(flashlightEntity, out var flashlight)) return;

            var config = frame.FindAsset<FlashlightConfig>(flashlight->Config);

            flashlight->CurrentCharge -= config.DischargePerSecond * frame.DeltaTime;

            if (flashlight->CurrentCharge <= FP._0)
            {
                flashlight->CurrentCharge = FP._0;
                ToggleFlashlight(frame, flashlightEntity);
            }
        }

        private void ToggleFlashlight(Frame frame, EntityRef flashlightEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Flashlight>(flashlightEntity, out var flashlight)) return;

            if (!flashlight->IsOn && flashlight->CurrentCharge <= 0) return;

            flashlight->IsOn = !flashlight->IsOn;

            frame.Events.FlashlightToggled(flashlightEntity, flashlight->IsOn);
        }
    }
}
