using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class InteractableMapChangerSystem : SystemMainThreadFilter<InteractableMapChangerSystem.Filter>, ISignalOnMapChanged, ISignalOnCompleteAllQuotaZones, ISignalOnMapChangeAvailable
    {
        public struct Filter
        {
            public EntityRef Entity;
            public InteractableMapChanger* MapChanger;
        }

        public void OnMapChanged(Frame frame, AssetRef<Map> previousMap)
        {
            frame.Global->TrackedInteractableMapChanger = EntityRef.None;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!frame.Global->TrackedInteractableMapChanger.IsValid)
            {
                frame.Global->TrackedInteractableMapChanger = filter.Entity;
            }

            if (filter.MapChanger->IsActive)
            {
                OpenHatch(frame, ref filter);
            }
        }

        public void OnCompleteAllQuotaZones(Frame frame)
        {
            ActivateMapChanger(frame);
        }

        public void OnMapChangeAvailable(Frame frame)
        {
            ActivateMapChanger(frame);
        }

        private void ActivateMapChanger(Frame frame)
        {
            var mapChanger = frame.Unsafe.GetPointer<InteractableMapChanger>(frame.Global->TrackedInteractableMapChanger);

            mapChanger->IsActive = true;

            if (mapChanger->HatchIsOpen) return;

            if (!frame.Unsafe.TryGetPointer<Transform3D>(mapChanger->HatchEntity, out var hatchTransform)) return;

            mapChanger->HatchInitialRotation = hatchTransform->Rotation;
            mapChanger->HatchIsOpen = true;
        }

        private void OpenHatch(Frame frame, ref Filter filter)
        {
            if (!frame.Unsafe.TryGetPointer<Transform3D>(filter.MapChanger->HatchEntity, out var hatchTransform)) return;

            hatchTransform->Rotation = FPQuaternion.Slerp(hatchTransform->Rotation, filter.MapChanger->HatchInitialRotation * FPQuaternion.Euler(0, FP.FromFloat_UNSAFE(15), 0), frame.DeltaTime);
        }
    }
}
