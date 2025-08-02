using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerDragSystem : SystemMainThreadFilter<PlayerDragSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
        }

        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
        }
    }
}
