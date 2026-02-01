using Photon.Deterministic;

namespace Quantum
{
    public unsafe class ShopZoneSystem : SystemSignalsOnly, ISignalOnComponentAdded<ShopZone>, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D, ISignalOnShopPurchaseAttempted
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

            UpdateShopZone(frame, triggerInfo.Entity, triggerInfo.Other, true);
        }

        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<ShopZone>(triggerInfo.Entity, out ShopZone* shopZone)) return;
            if (!frame.Unsafe.TryGetPointer<Valuable>(triggerInfo.Other, out var valuable)) return;

            if (!valuable->IsShopValuable) return;

            var inZoneValuables = frame.ResolveHashSet<EntityRef>(shopZone->InZoneValuables);
            inZoneValuables.Remove(triggerInfo.Other);

            UpdateShopZone(frame, triggerInfo.Entity, triggerInfo.Other, false);
        }

        private void UpdateShopZone(Frame frame, EntityRef shopZoneEntity, EntityRef valuableEntity, bool isIncremental)
        {
            if (!frame.Unsafe.TryGetPointer<ShopZone>(shopZoneEntity, out var shopZone)) return;
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;

            shopZone->InZoneValue += isIncremental
                ? valuable->CurrentValue
                : -valuable->CurrentValue;

            frame.Events.ShopZoneUpdated(shopZoneEntity);
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
    }
}
