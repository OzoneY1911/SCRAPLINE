using Photon.Deterministic;

namespace Quantum
{

    public unsafe class PlayerDeathSystem : SystemMainThreadFilter<PlayerDeathSystem.Filter>, ISignalOnEntityDeath
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
        }

        public void OnEntityDeath(Frame frame, EntityRef entity)
        {

        }

        public override void Update(Frame frame, ref Filter filter)
        {

        }
    }
}
