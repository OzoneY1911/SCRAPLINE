using Photon.Deterministic;

namespace Quantum
{
    public unsafe class ValuableSystem : SystemSignalsOnly, ISignalOnCollisionEnter3D
    {
        public void OnCollisionEnter3D(Frame frame, CollisionInfo3D info)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(info.Entity, out var valuable)) return;

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

            if (hitPower < 2) return;

            var hitDamage = hitPower * valuable->Fragility;

            valuable->CurrentValue -= FPMath.RoundToInt(hitDamage);

            frame.Events.ValuableHit(info.Entity, info.ContactPoints.First, hitDamage);

            if (valuable->CurrentValue <= 0) Break(frame, info.Entity);
        }

        private void Break(Frame frame, EntityRef entity)
        {
            frame.Destroy(entity);
        }
    }
}
