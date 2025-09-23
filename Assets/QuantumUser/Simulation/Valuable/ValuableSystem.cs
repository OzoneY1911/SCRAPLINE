using Photon.Deterministic;

namespace Quantum
{
    public unsafe class ValuableSystem : SystemMainThreadFilter<ValuableSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Valuable* Valuable;
        }

        public override void Update(Frame frame, ref Filter filter)
        {

        }
    }
}
