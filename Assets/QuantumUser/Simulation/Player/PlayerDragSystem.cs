using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerDragSystem : SystemMainThreadFilter<PlayerDragSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public Transform3D* Transform;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

            if (input->Interact.IsDown)
            {
                var hit = frame.Physics3D.Raycast(
                    input->CameraPosition,
                    input->CameraForward,
                    player->InteractionDistance,
                    ~player->LocalMask,
                    QueryOptions.HitSolids
                    );
            }
        }
    }
}
