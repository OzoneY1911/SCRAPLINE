using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerDraggingSystem : SystemMainThreadFilter<PlayerDraggingSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public Player* Player;
            public PlayerDragging* PlayerDragging;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var playerDragging = filter.PlayerDragging;
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

            if (input->Interact.IsDown && !playerDragging->IsDragging)
            {
                var hit = frame.Physics3D.Raycast(
                    input->CameraPosition,
                    input->CameraForward,
                    player->InteractionDistance,
                    ~player->LocalMask,
                    QueryOptions.HitSolids
                    );

                if (hit.HasValue)
                {
                    var hitEntity = hit.Value.Entity;

                    if (frame.Has<Draggable>(hitEntity))
                    {
                        playerDragging->IsDragging = true;
                        playerDragging->DraggedEntity = hitEntity;
                        playerDragging->DragDistance = hit.Value.CastDistanceNormalized * player->InteractionDistance;
                    }
                }
            }

            if (playerDragging->IsDragging)
            {
                if (input->Interact.WasReleased || !frame.Exists(playerDragging->DraggedEntity))
                {
                    playerDragging->IsDragging = false;
                    playerDragging->DraggedEntity = default;
                }
            }
        }
    }
}
