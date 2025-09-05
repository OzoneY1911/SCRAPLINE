using Photon.Deterministic;

namespace Quantum
{

    public unsafe class HealthSystem : SystemMainThreadFilter<HealthSystem.Filter>, ISignalOnComponentAdded<Health>
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
                frame.Signals.OnEntityDeath(filter.Entity);
            }
        }
    }
}
