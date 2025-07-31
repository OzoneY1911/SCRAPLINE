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
            if (!filter.Player->PlayerRef.IsValid) return;

            HandleRotation(frame, ref filter);
            HandleCrouching(frame, ref filter);
            HandleMovement(frame, ref filter);
            HandleJumping(frame, ref filter);
        }

        private void HandleRotation(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(player->PlayerRef);

            player->LookYaw += input->LookRotationDelta.Y;
            player->LookPitch += input->LookRotationDelta.X;

            filter.Transform->Rotation = FPQuaternion.Euler(0, player->LookYaw, 0);
        }

        private void HandleMovement(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(player->PlayerRef);
            var body = filter.Body;

            // Movement on XZ plane
            FPVector3 localMove = new FPVector3(input->MoveDirection.X, 0, input->MoveDirection.Y);

            // Rotate movement direction by the player's current rotation
            FPQuaternion rotation = filter.Transform->Rotation;
            FPVector3 worldMove = rotation * localMove;

            if (worldMove.SqrMagnitude > FP._0)
                worldMove = worldMove.Normalized;
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
        }

        private void HandleJumping(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(player->PlayerRef);
            var body = filter.Body;

            if (input->Jump.WasPressed && PlayerPhysicsUtils.IsGrounded(frame, in filter))
            {
                body->AddLinearImpulse(FPVector3.Up * player->JumpForce * body->Mass);
            }
        }

        private void HandleCrouching(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(player->PlayerRef);

            player->IsCrouching = input->Crouch.IsDown;

            ref Shape3D shape = ref filter.Collider->Shape;

            FP radius = shape.Capsule.Radius;

            FP currentHalfHeight = shape.Capsule.Height * FP._0_50;

            FP targetHalfHeight;

            if (filter.Player->IsCrouching)
            {
                targetHalfHeight = filter.Player->HeightCrouching * FP._0_50;
            }
            else
            {
                if (PlayerPhysicsUtils.CanStandUp(frame, in filter))
                {
                    targetHalfHeight = filter.Player->HeightStanding * FP._0_50;
                }
                else
                {
                    targetHalfHeight = filter.Player->HeightCrouching * FP._0_50;
                    filter.Player->IsCrouching = true;
                }
            }

            FP newHalfHeight = FPMath.Lerp(currentHalfHeight, targetHalfHeight, frame.DeltaTime * filter.Player->CrouchLerpSpeed);

            FPVector3 posOffset = new FPVector3(0, newHalfHeight, 0);

            shape = Shape3D.CreateCapsule(radius, newHalfHeight - shape.Capsule.Radius, posOffset);
        }
    }
}