using Photon.Deterministic;

namespace Quantum
{
    public unsafe class ValuableSystem : SystemSignalsOnly, ISignalOnComponentAdded<Valuable>, ISignalOnCollisionEnter3D
    {
        public void OnAdded(Frame frame, EntityRef entity, Valuable* valuable)
        {
            var valuableConfig = frame.FindAsset<ValuableConfig>(valuable->Config);

            valuable->CurrentValue = valuableConfig.DefaultValue;
            valuable->CurrentFragility = valuableConfig.DefaultFragility;
        }

        public void OnCollisionEnter3D(Frame frame, CollisionInfo3D info)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(info.Entity, out var valuable)) return;

            if (valuable->IsShopValuable) return;

            var entityVelocity = FPVector3.Zero;
            var otherVelocity = FPVector3.Zero;

            if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(info.Entity, out var entityBody))
            {
                entityVelocity = entityBody->Velocity;
            }

            if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(info.Other, out var otherBody))
            {
                otherVelocity = otherBody->Velocity;
            }

            FPVector3 relativeVelocity = entityVelocity - otherVelocity;
            FP hitPower = relativeVelocity.Magnitude;

            if (hitPower < 2 || valuable->CurrentFragility == 0) return;

            var hitDamage = hitPower * valuable->CurrentFragility;

            valuable->CurrentValue -= FPMath.RoundToInt(hitDamage);

            if (valuable->TrackedZoneEntity != EntityRef.None)
            {
                if (!frame.Has<QuotaZone>(valuable->TrackedZoneEntity)) return;
                frame.Signals.OnInZoneValuableDamaged(info.Entity, hitDamage);
            }

            frame.Events.ValuableHit(info.Entity, info.ContactPoints.First, hitDamage);

            if (valuable->CurrentValue <= 0) Break(frame, info.Entity);
        }

        private void Break(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(entity, out var valuable)) return;

            if (valuable->TrackedZoneEntity != EntityRef.None)
            {
                if (!frame.Has<QuotaZone>(valuable->TrackedZoneEntity)) return;
                frame.Signals.OnInZoneValuableDestroyed(entity);
            }

            frame.Destroy(entity);
        }
    }
}
