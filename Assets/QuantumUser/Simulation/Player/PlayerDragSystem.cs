using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerDragSystem : SystemMainThreadFilter<PlayerDragSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

            if (input->Interact.IsDown)
            {
            }
        }

        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
        }
    }
}
