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
                UpdateQuotaZone(frame, triggerInfo.Other, triggerInfo.Entity, true);
            }
        }

        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {
            if (frame.Has<Valuable>(triggerInfo.Entity))
            {
                UpdateQuotaZone(frame, triggerInfo.Other, triggerInfo.Entity, false);
            }
        }

        private void UpdateQuotaZone(Frame frame, EntityRef entity, EntityRef valuableEntity, bool isIncremental)
        {
            if (!frame.Unsafe.TryGetPointer<QuotaZone>(entity, out QuotaZone* quotaZone)) return;

            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;

            var resourceDemands = frame.ResolveList<ResourceDemand>(quotaZone->ResourceDemands);
            var resourceFractions = frame.ResolveList<ResourceFraction>(valuable->ResourceFractions);

            foreach (var resourceFraction in resourceFractions)
            {
                for (int i = 0; i < resourceDemands.Count; i++)
                {
                    if (resourceFraction.Type == resourceDemands[i].Type)
                    {
                        var demandPtr = resourceDemands.GetPointer(i);

                        var resourceContribution = resourceFraction.Value * valuable->CurrentValue;

                        demandPtr->Collected += isIncremental
                            ? resourceContribution
                            : -resourceContribution;

                        demandPtr->IsSatisfied = demandPtr->Collected >= demandPtr->Value;
                    }
                }
            }
            frame.Events.QuotaZoneUpdated(entity);

            foreach (var demand in resourceDemands)
            {
                if (!demand.IsSatisfied)
                {
                    quotaZone->IsSatisfied = false;
                    return;
                }
                quotaZone->IsSatisfied = true;
            }
        }
    }
}
