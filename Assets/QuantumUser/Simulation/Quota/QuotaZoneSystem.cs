using Photon.Deterministic;
namespace Quantum
{
    public unsafe class QuotaZoneSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D, ISignalOnComponentAdded<QuotaZone>, ISignalOnMapChanged, ISignalOnActivateQuotaZone, ISignalOnCompleteQuotaZone, ISignalOnInZoneValuableDamaged, ISignalOnInZoneValuableDestroyed, ISignalOnInZoneValuableCollectedByPlayer
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

            if (frame.Unsafe.TryGetPointer<Valuable>(triggerInfo.Other, out Valuable* valuable))
            {
                var inZoneValuables = frame.ResolveHashSet<EntityRef>(quotaZone->InZoneValuables);
                inZoneValuables.Add(triggerInfo.Other);

                if (!quotaZone->IsActivated) return;

                valuable->QuotaZoneEntity = triggerInfo.Entity;
                UpdateQuotaZone(frame, triggerInfo.Other, true);
            }
        }

        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {
            RemoveValuableFromQuotaZone(frame, triggerInfo.Other);
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
                    var valuable = frame.Unsafe.GetPointer<Valuable>(inZoneValuable);
                    valuable->QuotaZoneEntity = entity;
                    UpdateQuotaZone(frame, inZoneValuable, true);
                }
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

        public void OnInZoneValuableDamaged(Frame frame, EntityRef valuableEntity, FP damage)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
            CalculateResourceDemands(frame, valuableEntity, damage, false);
        }

        public void OnInZoneValuableDestroyed(Frame frame, EntityRef valuableEntity)
        {
            RemoveValuableFromQuotaZone(frame, valuableEntity);
        }

        public void OnInZoneValuableCollectedByPlayer(Frame frame, EntityRef valuableEntity)
        {
            RemoveValuableFromQuotaZone(frame, valuableEntity);
        }

        private void UpdateQuotaZone(Frame frame, EntityRef valuableEntity, bool isIncremental)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
            CalculateResourceDemands(frame, valuableEntity, valuable->CurrentValue, isIncremental);
        }

        private void CalculateResourceDemands(Frame frame, EntityRef valuableEntity, FP value, bool isIncremental)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
            if (!frame.Unsafe.TryGetPointer<QuotaZone>(valuable->QuotaZoneEntity, out QuotaZone* quotaZone)) return;

            var resourceDemands = frame.ResolveList<ResourceDemand>(quotaZone->ResourceDemands);
            var resourceFractions = frame.ResolveList<ResourceFraction>(valuable->ResourceFractions);

            foreach (var resourceFraction in resourceFractions)
            {
                for (int i = 0; i < resourceDemands.Count; i++)
                {
                    if (resourceFraction.Type == resourceDemands[i].Type)
                    {
                        var demandPtr = resourceDemands.GetPointer(i);

                        var resourceContribution = resourceFraction.Value * value;

                        demandPtr->Collected += isIncremental
                            ? resourceContribution
                            : -resourceContribution;

                        demandPtr->Collected = FPMath.Max(FP._0, demandPtr->Collected);

                        demandPtr->IsSatisfied = demandPtr->Collected >= demandPtr->Value;
                    }
                }
            }
            frame.Events.QuotaZoneUpdated(valuable->QuotaZoneEntity);

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

        private void RemoveValuableFromQuotaZone(Frame frame, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
            if (!frame.Unsafe.TryGetPointer<QuotaZone>(valuable->QuotaZoneEntity, out QuotaZone* quotaZone)) return;

            var inZoneValuables = frame.ResolveHashSet<EntityRef>(quotaZone->InZoneValuables);
            inZoneValuables.Remove(valuableEntity);

            if (!quotaZone->IsActivated) return;

            UpdateQuotaZone(frame, valuableEntity, false);
            valuable->QuotaZoneEntity = EntityRef.None;
        }
    }
}
