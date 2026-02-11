using Photon.Deterministic;

namespace Quantum
{
    public unsafe class ShopZoneSystem : SystemSignalsOnly, ISignalOnComponentAdded<ShopZone>, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D, ISignalOnShopPurchaseAttempted, ISignalOnInZoneValuableCollectedByPlayer
    {
        public void OnAdded(Frame frame, EntityRef entity, ShopZone* shopZone)
        {
            shopZone->InZoneValuables = frame.AllocateHashSet<EntityRef>();
        }

        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<ShopZone>(triggerInfo.Entity, out ShopZone* shopZone)) return;
            if (!frame.Unsafe.TryGetPointer<Valuable>(triggerInfo.Other, out var valuable)) return;

            if (!valuable->IsShopValuable) return;

            var inZoneValuables = frame.ResolveHashSet<EntityRef>(shopZone->InZoneValuables);
            inZoneValuables.Add(triggerInfo.Other);

            valuable->TrackedZoneEntity = triggerInfo.Entity;
            UpdateShopZone(frame, triggerInfo.Other, true);
        }

        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(triggerInfo.Other, out var valuable)) return;
            if (!frame.Unsafe.TryGetPointer<ShopZone>(triggerInfo.Entity, out ShopZone* shopZone)) return;

            if (!valuable->IsShopValuable) return;

            var inZoneValuables = frame.ResolveHashSet<EntityRef>(shopZone->InZoneValuables);
            inZoneValuables.Remove(triggerInfo.Other);

            UpdateShopZone(frame, triggerInfo.Other, false);
            valuable->TrackedZoneEntity = EntityRef.None;
        }

        private void UpdateShopZone(Frame frame, EntityRef valuableEntity, bool isIncremental)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
            if (!frame.Unsafe.TryGetPointer<ShopZone>(valuable->TrackedZoneEntity, out var shopZone)) return;

            shopZone->InZoneValue += isIncremental
                ? valuable->CurrentValue
                : -valuable->CurrentValue;

            frame.Events.ShopZoneUpdated(valuable->TrackedZoneEntity);
        }

        public void OnShopPurchaseAttempted(Frame frame, EntityRef shopZoneEntity)
        {
            if (!frame.Unsafe.TryGetPointer<ShopZone>(shopZoneEntity, out var shopZone)) return;

            if (shopZone->InZoneValue == FP._0)
            {
                // Nothing to purchase
            }
            else if (shopZone->InZoneValue > frame.Global->PlayerMoney)
            {
                // Insufficient money
            }
            else
            {
                var inZoneValuables = frame.ResolveHashSet<EntityRef>(shopZone->InZoneValuables);

                foreach (var inZoneValuableEntity in inZoneValuables)
                {
                    if (frame.Unsafe.TryGetPointer<Valuable>(inZoneValuableEntity, out var inZoneValuable))
                    {
                        inZoneValuable->IsShopValuable = false;
                        inZoneValuable->TrackedZoneEntity = EntityRef.None;

                        frame.Unsafe.GetPointer<PhysicsCollider3D>(inZoneValuableEntity)->Layer = 0;
                    }
                }

                frame.Global->PlayerMoney -= shopZone->InZoneValue;

                inZoneValuables.Clear();
                shopZone->InZoneValue = FP._0;
                
                frame.Events.PlayerMoneyUpdated();
                frame.Events.ShopZoneUpdated(shopZoneEntity);
                frame.Events.ShopPurchaseSucceeded(shopZoneEntity);
            }
        }
        public void OnInZoneValuableCollectedByPlayer(Frame frame, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
            if (!frame.Unsafe.TryGetPointer<ShopZone>(valuable->TrackedZoneEntity, out ShopZone* shopZone)) return;

            var inZoneValuables = frame.ResolveHashSet<EntityRef>(shopZone->InZoneValuables);
            inZoneValuables.Remove(valuableEntity);

            UpdateShopZone(frame, valuableEntity, false);
            valuable->TrackedZoneEntity = EntityRef.None;
        }
    }
}
