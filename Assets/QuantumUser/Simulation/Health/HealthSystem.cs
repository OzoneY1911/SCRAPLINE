using Photon.Deterministic;

namespace Quantum
{
    public unsafe class HealthSystem : SystemMainThreadFilter<HealthSystem.Filter>, ISignalOnComponentAdded<Health>, ISignalOnHealthChanged
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Health* Health;
        }

        public void OnAdded(Frame frame, EntityRef entity, Health* health)
        {
            health->Current = health->Max;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Health->Current <= 0)
            {
                frame.Events.EntityDeath(filter.Entity);
                frame.Signals.OnEntityDeath(filter.Entity);
            }
        }

        public void OnHealthChanged(Frame frame, EntityRef entity, FP changeDelta)
        {
            if (!frame.Unsafe.TryGetPointer<Health>(entity, out var health)) return;

            health->Current += changeDelta;

            if (health->Current > health->Max) health->Current = health->Max;
        }
    }
}
