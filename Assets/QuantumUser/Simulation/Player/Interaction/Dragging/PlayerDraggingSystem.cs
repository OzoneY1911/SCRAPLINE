using Photon.Deterministic;
using System.Security.Principal;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerDraggingSystem : SystemMainThreadFilter<PlayerDraggingSystem.Filter>, ISignalOnValuableCollected
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public Player* Player;
            public PlayerDragging* PlayerDragging;
        }

        public void OnValuableCollected(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            var playerDragging = frame.Unsafe.GetPointer<PlayerDragging>(playerEntity);

            StopDragging(frame, playerDragging);
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var playerDragging = filter.PlayerDragging;
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

            if (input->Interact.IsDown && !playerDragging->IsDragging)
            {
                TryStartDragging(frame, ref filter);
            }

            if (playerDragging->IsDragging)
            {
                if (input->Interact.WasReleased || !frame.Exists(playerDragging->DraggedEntity))
                {
                    StopDragging(frame, filter.PlayerDragging);
                    return;
                }

                if (input->ScrollDelta != 0)
                {
                    PushPullDraggable(frame, ref filter);
                }

                if (input->Use.IsDown && input->LookRotationDelta != FPVector2.Zero)
                {
                    RotateDraggable(frame, ref filter);
                }

                if (input->CollectValuable.WasPressed)
                {
                    if (frame.Unsafe.TryGetPointer<Valuable>(playerDragging->DraggedEntity, out var valuable))
                    {
                        var valuableConfig = frame.FindAsset<ValuableConfig>(valuable->Config);
                        if (valuableConfig.IsPocketValuable || valuableConfig.IsBackDevice)
                        {
                            frame.Signals.OnValuableCollectAttempted(filter.Entity, playerDragging->DraggedEntity, valuableConfig);
                        }
                    }
                }

                DrivePosition(frame, ref filter);
                DriveRotation(frame, ref filter);
            }
        }

        private void TryStartDragging(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var playerDragging = filter.PlayerDragging;

            var hit = PlayerPhysicsUtils.PlayerInteractionHitscan(frame, player);

            if (hit.HasValue)
            {
                var hitEntity = hit.Value.Entity;

                if (frame.Has<Draggable>(hitEntity))
                {
                    playerDragging->IsDragging = true;
                    playerDragging->DraggedEntity = hitEntity;
                    playerDragging->DragDistance = hit.Value.CastDistanceNormalized * player->InteractionDistance;

                    if (frame.Unsafe.TryGetPointer<Transform3D>(hitEntity, out var hitTransform))
                    {
                        var invRot = FPQuaternion.Inverse(hitTransform->Rotation);
                        filter.PlayerDragging->GrabLocalPoint = invRot * (hit.Value.Point - hitTransform->Position);

                        playerDragging->DraggedRelativeRotation = FPQuaternion.Inverse(filter.Transform->Rotation) * hitTransform->Rotation;
                    }

                    if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(hitEntity, out var hitBody))
                    {
                        hitBody->AllowSleeping = false;
                    }

                    ClampDragDistance(frame, ref filter);
                }
            }
        }

        private void StopDragging(Frame frame, PlayerDragging* playerDragging)
        {
            if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(playerDragging->DraggedEntity, out var draggedBody))
            {
                draggedBody->AllowSleeping = true;
            }

            playerDragging->IsDragging = false;
            playerDragging->DraggedEntity = default;
        }

        private void DrivePosition(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var playerDragging = filter.PlayerDragging;

            if (!frame.Unsafe.TryGetPointer<PhysicsBody3D>(playerDragging->DraggedEntity, out var draggedBody)) return;

            if (!frame.Unsafe.TryGetPointer<Transform3D>(playerDragging->DraggedEntity, out var draggedTransform)) return;

            if (input->CameraPosition == default) return;

            FPVector3 targetPoint = input->CameraPosition + input->CameraForward * playerDragging->DragDistance;

            if (playerDragging->SagPerMass > FP._0)
            {
                targetPoint -= FPVector3.Up * (draggedBody->Mass * playerDragging->SagPerMass);
            }

            // World-space anchor of the grab point
            FPVector3 anchor = draggedTransform->Position + (draggedTransform->Rotation * playerDragging->GrabLocalPoint);

            FP kp = 12;                         // base stiffness (feel)
            FP kd = FPMath.Max(FP._0, playerDragging->DampingRatio) * 2; // more = less oscillation

            FPVector3 error = targetPoint - anchor;
            FPVector3 desiredVel = error * kp;
            FPVector3 deltaVel = desiredVel - draggedBody->Velocity;

            // impulse = m * Δv * kd * dt
            FPVector3 impulse = deltaVel * frame.DeltaTime * kd * 5 / draggedBody->Mass;

            draggedBody->AddLinearImpulse(impulse);
        }

        private void DriveRotation(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var playerDragging = filter.PlayerDragging;

            if (!frame.Unsafe.TryGetPointer<PhysicsBody3D>(playerDragging->DraggedEntity, out var body))
                return;

            if (!frame.Unsafe.TryGetPointer<Transform3D>(playerDragging->DraggedEntity, out var transform))
                return;

            FPQuaternion targetRotation =
                FPQuaternion.LookRotation(input->CameraForward, FPVector3.Up) *
                playerDragging->DraggedRelativeRotation;

            FPQuaternion currentRotation = transform->Rotation;

            FPQuaternion error =
                targetRotation * FPQuaternion.Inverse(currentRotation);

            error = error.Normalized;

            // Shortest path
            if (error.W < FP._0)
            {
                error.X = -error.X;
                error.Y = -error.Y;
                error.Z = -error.Z;
                error.W = -error.W;
            }

            FP w = FPMath.Clamp(error.W, -FP._1, FP._1);

            FP angle = FP._2 * FPMath.Acos(w);

            if (angle < FP.EN3)
            {
                return;
            }

            FP sinHalf =
                FPMath.Sqrt(FPMath.Max(FP._0, FP._1 - w * w));

            if (sinHalf < FP.EN3)
            {
                return;
            }

            FPVector3 axis = new FPVector3(
                error.X / sinHalf,
                error.Y / sinHalf,
                error.Z / sinHalf
            );

            // PD gains
            FP stiffness = 5;
            FP damping = 1;

            FPVector3 angularImpulse =
                axis * angle * stiffness
                - body->AngularVelocity * damping;

            // Clamp maximum impulse
            FP magnitude = angularImpulse.Magnitude;

            if (magnitude > 1)
            {
                angularImpulse = angularImpulse / magnitude;
            }

            body->AddAngularImpulse(angularImpulse * frame.DeltaTime);
        }

        private void ClampDragDistance(Frame frame, ref Filter filter)
        {
            var playerDragging = filter.PlayerDragging;

            playerDragging->DragDistance = FPMath.Clamp(playerDragging->DragDistance, playerDragging->MinDragDistance, playerDragging->MaxDragDistance);
        }

        private void PushPullDraggable(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var playerDragging = filter.PlayerDragging;

            playerDragging->DragDistance += input->ScrollDelta * playerDragging->PushPullStep;
            ClampDragDistance(frame, ref filter);
        }

        private void RotateDraggable(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var playerDragging = filter.PlayerDragging;

            var draggedTransform = frame.Unsafe.GetPointer<Transform3D>(playerDragging->DraggedEntity);

            // Rotation relative to camera
            FP upDelta = input->LookRotationDelta.Y;
            FP rightDelta = input->LookRotationDelta.X;

            // Get camera axes from player transform
            FPQuaternion playerRot = filter.Transform->Rotation;
            FPVector3 camRight = playerRot * FPVector3.Right;
            FPVector3 camUp = playerRot * FPVector3.Up;

            FPQuaternion pitchRot = FPQuaternion.AngleAxis(upDelta, camUp);
            FPQuaternion yawRot = FPQuaternion.AngleAxis(-rightDelta, camRight); // negative to feel natural

            // Combine
            draggedTransform->Rotation = yawRot * pitchRot * draggedTransform->Rotation;
        }
    }
}
