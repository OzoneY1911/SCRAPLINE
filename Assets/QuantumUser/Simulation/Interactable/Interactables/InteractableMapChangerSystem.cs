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
        }
    }
}
