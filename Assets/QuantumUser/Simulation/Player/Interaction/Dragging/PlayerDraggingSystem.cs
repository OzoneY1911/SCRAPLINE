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
                TryStartDragging(frame, ref filter);
            }

            if (playerDragging->IsDragging)
            {
                if (input->Interact.WasReleased || !frame.Exists(playerDragging->DraggedEntity))
                {
                    StopDragging(frame, ref filter);
                    return;
                }

                if (input->ScrollDelta != 0)
                {
                    PushPullDraggable(frame, ref filter);
                }

                if (input->SecondaryAction.IsDown && input->LookRotationDelta != FPVector2.Zero)
                {
                    //RotateDraggable(frame, ref filter);
                }

                DrivePosition(frame, ref filter);
                DriveRotation(frame, ref filter);
            }
        }

        private void TryStartDragging(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var playerDragging = filter.PlayerDragging;
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

                    ClampDragDistance(frame, ref filter);
                }
            }
        }

        private void StopDragging(Frame frame, ref Filter filter)
        {
            var playerDragging = filter.PlayerDragging;

            playerDragging->IsDragging = false;
            playerDragging->DraggedEntity = default;
        }

        private void DrivePosition(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var playerDragging = filter.PlayerDragging;

            var draggedBody = frame.Unsafe.GetPointer<PhysicsBody3D>(playerDragging->DraggedEntity);

            var draggedTransform = frame.Unsafe.GetPointer<Transform3D>(playerDragging->DraggedEntity);

            if (input->CameraPosition == default) return;

            FPVector3 targetPoint = input->CameraPosition + input->CameraForward * playerDragging->DragDistance;

            if (playerDragging->SagPerMass > FP._0)
            {
                targetPoint -= FPVector3.Up * (draggedBody->Mass * playerDragging->SagPerMass);
            }

            // World-space anchor of the grab point
            FPVector3 anchor = draggedTransform->Position + (draggedTransform->Rotation * playerDragging->GrabLocalPoint);

            // We'll keep a fixed stiffness (kp) and scale damping by your DampingRatio.
            FP kp = 12;                         // base stiffness (feel)
            FP kd = FPMath.Max(FP._0, playerDragging->DampingRatio) * 2; // more = less oscillation

            FPVector3 error = targetPoint - anchor;
            FPVector3 desiredVel = error * kp;
            FPVector3 deltaVel = desiredVel - draggedBody->Velocity;

            // impulse = m * Δv * kd * dt
            FPVector3 impulse = deltaVel * 15 * frame.DeltaTime * kd;

            draggedBody->AddLinearImpulse(impulse);

            /*
            UnityEngine.Debug.DrawLine(
                new UnityEngine.Vector3(
                    (float) targetPoint.X,
                    (float) targetPoint.Y,
                    (float) targetPoint.Z
                ),
                new UnityEngine.Vector3(
                    (float) anchor.X,
                    (float) anchor.Y,
                    (float) anchor.Z
                ),
                UnityEngine.Color.red
            );
            */
        }

        private void DriveRotation(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var playerDragging = filter.PlayerDragging;

            var draggedBody = frame.Unsafe.GetPointer<PhysicsBody3D>(playerDragging->DraggedEntity);

            var draggedTransform = frame.Unsafe.GetPointer<Transform3D>(playerDragging->DraggedEntity);

            // --- Rotation drive ---
            FPQuaternion cameraRotation = FPQuaternion.LookRotation(input->CameraForward, FPVector3.Up);

            // Use camera rotation instead of player body rotation
            FPQuaternion targetRotation = cameraRotation * playerDragging->DraggedRelativeRotation;

            // Get rotation difference (from current to target)
            FPQuaternion currentRotation = draggedTransform->Rotation;
            FPQuaternion deltaRot = targetRotation * FPQuaternion.Inverse(currentRotation);

            deltaRot = deltaRot.Normalized; // ensure it's a unit quaternion

            // Clamp W into [-1, 1] to avoid sqrt/acos domain errors
            FP w = FPMath.Clamp(deltaRot.W, -FP._1, FP._1);

            // Compute angle
            FP angle = FP._2 * FPMath.Acos(w);

            // Safe sqrt
            FP sinHalfAngle = FPMath.Sqrt(FPMath.Max(FP._0, FP._1 - w * w));

            FPVector3 axis;
            if (FPMath.Abs(sinHalfAngle) > FP.Epsilon)
            {
                axis = new FPVector3(
                    deltaRot.X / sinHalfAngle,
                    deltaRot.Y / sinHalfAngle,
                    deltaRot.Z / sinHalfAngle
                );
            }
            else
            {
                // If angle is very small, use any normalized axis
                axis = FPVector3.Right;
            }

            // Ensure shortest path
            if (angle > FP.Pi)
            {
                angle -= FP.Pi * FP._2;
            }

            // Angular velocity needed to reach target
            FP angularStiffness = FP._5; // tweak
            FP angularDamping = FP._2;    // tweak

            FPVector3 desiredAngularVel = axis * (angle * angularStiffness);
            FPVector3 deltaAngularVel = desiredAngularVel - draggedBody->AngularVelocity;

            // Impulse = I * Δω * dt
            // For simplicity, assume symmetrical inertia: I = Mass * radius² (Quantum doesn't expose per-axis inertia easily)
            FP mass = draggedBody->Mass;
            FPVector3 angularImpulse = deltaAngularVel * mass * frame.DeltaTime * angularDamping;

            draggedBody->AddAngularImpulse(angularImpulse);
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

            // Apply pitch (around camera right) and yaw (around camera up)
            FPQuaternion pitchRot = FPQuaternion.AngleAxis(upDelta, camRight);
            FPQuaternion yawRot = FPQuaternion.AngleAxis(-rightDelta, camUp); // negative to feel natural

            // Combine
            draggedTransform->Rotation = yawRot * pitchRot * draggedTransform->Rotation;
        }
    }
}
