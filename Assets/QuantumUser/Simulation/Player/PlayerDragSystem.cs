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
                frame.Physics3D.Raycast(
                    new FPVector3(filter.Transform->Position.X, player->CameraHeight, filter.Transform->Position.Z),
                    filter.Transform->Forward,
                    player->InteractionDistance,
                    ~player->LocalMask,
                    QueryOptions.HitSolids
                    );
            }
        }

        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public Transform3D* Transform;
        }
    }
}
