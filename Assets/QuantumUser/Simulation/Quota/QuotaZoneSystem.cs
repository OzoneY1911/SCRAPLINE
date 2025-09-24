namespace Quantum
{
    public unsafe class QuotaZoneSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D, ISignalOnComponentAdded<QuotaZone>
    {
        public void OnAdded(Frame frame, EntityRef entity, QuotaZone* quotaZone)
        {
            quotaZone->ResourceDemands = frame.AllocateList<ResourceDemand>(2);
            var resourceDemands = frame.ResolveList<ResourceDemand>(quotaZone->ResourceDemands);

            resourceDemands.Add(
                new ResourceDemand
                {
                    Type = ResourceType.Metal,
                    Value = 300
                });
            resourceDemands.Add(
                new ResourceDemand
                {
                    Type = ResourceType.Plastic,
                    Value = 500
                });

            frame.Events.QuotaZoneInitialized(entity);
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
            if (frame.Has<Valuable>(triggerInfo.Entity))
            {
                frame.Events.ValuableExit(triggerInfo.Entity);
            }
        }
    }
}
