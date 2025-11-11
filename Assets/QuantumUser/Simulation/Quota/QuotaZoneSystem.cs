namespace Quantum
{
    public unsafe class QuotaZoneSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D, ISignalOnComponentAdded<QuotaZone>, ISignalOnActivateQuotaZone, ISignalOnCompleteQuotaZone, ISignalOnMapChanged
    {
        public void OnMapChanged(Frame frame, AssetRef<Map> previousMap)
        {
            frame.ResolveList<EntityRef>(frame.Global->TrackedQuotaZones).Clear();
        }

        public void OnAdded(Frame frame, EntityRef entity, QuotaZone* quotaZone)
        {
            quotaZone->ResourceDemands = frame.AllocateList<ResourceDemand>(2);
            quotaZone->InZoneValuables = frame.AllocateHashSet<EntityRef>();

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

            if (frame.Global->TrackedQuotaZones.Ptr.Equals(Ptr.Null))
            {
                frame.Global->TrackedQuotaZones = frame.AllocateList<EntityRef>();
            }
            var trackedQuotaZones = frame.ResolveList<EntityRef>(frame.Global->TrackedQuotaZones);
            trackedQuotaZones.Add(entity);
        }

        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<QuotaZone>(triggerInfo.Entity, out QuotaZone* quotaZone)) return;

            if (frame.Has<Valuable>(triggerInfo.Other))
            {
                var inZoneValuables = frame.ResolveHashSet<EntityRef>(quotaZone->InZoneValuables);
                inZoneValuables.Add(triggerInfo.Other);

                if (!quotaZone->IsActivated) return;

                UpdateQuotaZone(frame, triggerInfo.Entity, triggerInfo.Other, true);
            }
        }

        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<QuotaZone>(triggerInfo.Entity, out QuotaZone* quotaZone)) return;

            if (frame.Has<Valuable>(triggerInfo.Other))
            {
                var inZoneValuables = frame.ResolveHashSet<EntityRef>(quotaZone->InZoneValuables);
                inZoneValuables.Remove(triggerInfo.Other);

                if (!quotaZone->IsActivated) return;

                UpdateQuotaZone(frame, triggerInfo.Entity, triggerInfo.Other, false);
            }
        }

        public void OnActivateQuotaZone(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<QuotaZone>(entity, out var quotaZone)) return;

            quotaZone->IsActivated = true;
            frame.Events.QuotaZoneActivated(entity);

            var inZoneValuables = frame.ResolveHashSet<EntityRef>(quotaZone->InZoneValuables);
            if (inZoneValuables.Count != 0)
            {
                foreach (var inZoneValuable in inZoneValuables)
                {
                    UpdateQuotaZone(frame, entity, inZoneValuable, true);
                }
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

        public void OnCompleteQuotaZone(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<QuotaZone>(entity, out var quotaZone)) return;

            var resourceDemands = frame.ResolveList<ResourceDemand>(quotaZone->ResourceDemands);

            quotaZone->IsCompleted = true;
            frame.Events.QuotaZoneCompleted(entity);

            var inZoneValuables = frame.ResolveHashSet<EntityRef>(quotaZone->InZoneValuables);
            foreach (var inZoneValuable in inZoneValuables)
            {
                frame.Destroy(inZoneValuable);
            }
            inZoneValuables.Clear();

            foreach (var resourceDemand in resourceDemands)
            {
                frame.Global->PlayerMoney += resourceDemand.Collected;
            }

            var trackedQuotaZones = frame.ResolveList<EntityRef>(frame.Global->TrackedQuotaZones);
            foreach (var trackedQuotaZone in trackedQuotaZones)
            {
                if (!frame.Unsafe.GetPointer<QuotaZone>(trackedQuotaZone)->IsCompleted) return;
            }

            frame.Signals.OnCompleteAllQuotaZones();
        }
    }
}
