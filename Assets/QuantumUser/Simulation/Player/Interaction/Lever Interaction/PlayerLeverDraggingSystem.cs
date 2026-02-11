using Photon.Deterministic;

namespace Quantum
{
    public unsafe class PlayerLeverDraggingSystem : SystemMainThreadFilter<PlayerLeverDraggingSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public Player* Player;
            public PlayerLeverDragging* PlayerLeverDragging;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var playerLeverDragging = filter.PlayerLeverDragging;
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

            if (input->Interact.IsDown && !playerLeverDragging->IsDragging)
            {
                TryStartDragging(frame, ref filter);
            }

            if (playerLeverDragging->IsDragging)
            {
                if (input->Interact.WasReleased
                    || !frame.Exists(playerLeverDragging->DraggedEntity)
                    || FPVector3.Distance(input->CameraPosition, frame.Unsafe.GetPointer<Transform3D>(playerLeverDragging->DraggedEntity)->Position) > player->InteractionDistance)
                {
                    StopDragging(frame, ref filter);
                    return;
                }

                if (frame.Unsafe.TryGetPointer<Transform3D>(playerLeverDragging->DraggedEntity, out var draggedTransform))
                {
                    var lever = frame.Unsafe.GetPointer<Lever>(playerLeverDragging->DraggedEntity);

                    lever->CurrentAngle = FPMath.Clamp(
                        lever->CurrentAngle + (input->LookRotationDelta.X * (FP._2 + FP._0_50)),
                        lever->InitialAngle,
                        lever->MaxAngle
                    );

                    draggedTransform->Rotation = lever->InitialRotation * FPQuaternion.Euler(
                        new FPVector3(lever->CurrentAngle - lever->InitialAngle, 0, 0));
                }
            }
        }

        private void TryStartDragging(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var playerLeverDragging = filter.PlayerLeverDragging;
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

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

                if (frame.Unsafe.TryGetPointer<Lever>(hitEntity, out Lever* lever))
                {
                    if (!lever->IsBeingInteracted)
                    {
                        lever->IsBeingInteracted = true;
                        playerLeverDragging->IsDragging = true;
                        playerLeverDragging->DraggedEntity = hitEntity;
                    }
                }
            }
        }

        private void StopDragging(Frame frame, ref Filter filter)
        {
            var playerLeverDragging = filter.PlayerLeverDragging;
            var lever = frame.Unsafe.GetPointer<Lever>(playerLeverDragging->DraggedEntity);

            lever->IsBeingInteracted = false;
            playerLeverDragging->IsDragging = false;
            playerLeverDragging->DraggedEntity = default;
        }
    }
}
