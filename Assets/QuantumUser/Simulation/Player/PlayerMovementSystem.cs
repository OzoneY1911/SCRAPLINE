using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public PhysicsBody3D* Body;
            public PhysicsCollider3D* Collider;
            public Player* Player;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            if (!player->PlayerRef.IsValid) return;

            var input = frame.GetPlayerInput(player->PlayerRef);
            var body = filter.Body;

            player->IsCrouching = input->Crouch.IsDown;

            UpdateColliderShape(frame, ref filter);

            // Update yaw and pitch using mouse deltas
            player->LookYaw += input->LookRotationDelta.Y;
            player->LookPitch += input->LookRotationDelta.X;

            filter.Transform->Rotation = FPQuaternion.Euler(0, player->LookYaw, 0);

            // Movement on XZ plane
            FPVector3 localMove = new FPVector3(input->MoveDirection.X, 0, input->MoveDirection.Y);

            // Rotate movement direction by the player's current rotation
            FPQuaternion rotation = filter.Transform->Rotation;
            FPVector3 worldMove = rotation * localMove;

            if (worldMove.SqrMagnitude > FP._0)
                worldMove = worldMove.Normalized;

            // Physics-based movement ---

            FPVector3 desiredVelocity;

            if (input->Run.IsDown && !player->IsCrouching)
            {
                desiredVelocity = worldMove * player->RunSpeed;
            }
            else if (player->IsCrouching)
            {
                desiredVelocity = worldMove * player->CrouchSpeed;
            }
            else
            {
                desiredVelocity = worldMove * player->WalkSpeed;
            }

            // Current velocity
            FPVector3 currentVel = body->Velocity;

            // Only control horizontal movement (ignore Y velocity)
            FPVector3 horizontalVel = new FPVector3(currentVel.X, 0, currentVel.Z);
            FPVector3 deltaVel = desiredVelocity - horizontalVel;

            // Apply impulse to achieve desired horizontal velocity
            body->AddLinearImpulse(deltaVel * body->Mass);

            if (input->Jump.WasPressed && IsGrounded(frame, ref filter))
            {
                body->AddLinearImpulse(FPVector3.Up * player->JumpForce * body->Mass);
            }
        }

        private bool IsGrounded(Frame frame, ref Filter filter)
        {
            FP yOffset = FP._0_10;

            ref Shape3D checkShape = ref filter.Collider->Shape;

            FPVector3 checkPosition = filter.Transform->Position - new FPVector3(0, yOffset, 0);

            var hits = frame.Physics3D.OverlapShape(checkPosition, filter.Transform->Rotation, checkShape);
            
            for (int i = 0; i < hits.Count; i++)
            {
                if (hits[i].Entity != filter.Entity)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateColliderShape(Frame frame, ref Filter filter)
        {
            ref Shape3D shape = ref filter.Collider->Shape;

            FP radius = shape.Capsule.Radius;

            FP targetHalfHeight = filter.Player->IsCrouching
                ? filter.Player->HeightCrouching * FP._0_50
                : filter.Player->HeightStanding * FP._0_50;

            FPVector3 posOffset = new FPVector3(0, targetHalfHeight, 0);

            shape = Shape3D.CreateCapsule(radius, targetHalfHeight - shape.Capsule.Radius, posOffset);
        }
    }
}