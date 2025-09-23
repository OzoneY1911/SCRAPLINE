using Photon.Deterministic;

namespace Quantum
{
    public unsafe class QuotaZoneSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D, ISignalOnComponentAdded<QuotaZone>
    {
        public void OnAdded(Frame frame, EntityRef entity, QuotaZone* quotaZone)
        {
            quotaZone->ResourceDemands = frame.AllocateDictionary<ResourceType, FP>(2);
            var resourceDemands = frame.ResolveDictionary<ResourceType, FP>(quotaZone->ResourceDemands);

            resourceDemands.Add(ResourceType.Metal, 500);
            resourceDemands.Add(ResourceType.Plastic, 300);
        }

        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D triggerInfo)
        {
            if (frame.Has<Valuable>(triggerInfo.Entity))
            {
                frame.Events.ValuableEnter(triggerInfo.Entity);
            }
        }

        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {

        }
    }
}
