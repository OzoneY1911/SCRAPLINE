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
            public PlayerMovement* Movement;
            public PlayerStamina* Stamina;
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
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var body = filter.Body;
            var movement = filter.Movement;
            var stamina = filter.Stamina;

            // Movement on XZ plane
            FPVector3 localMove = new FPVector3(input->MoveDirection.X, 0, input->MoveDirection.Y);

            // Rotate movement direction by the player's current rotation
            FPQuaternion rotation = filter.Transform->Rotation;
            FPVector3 worldMove = rotation * localMove;

            if (worldMove.SqrMagnitude > FP._0)
                worldMove = worldMove.Normalized;
            FPVector3 desiredVelocity;

            movement->IsRunning =
                input->Run.IsDown
                && localMove != FPVector3.Zero
                && !movement->IsCrouching
                && !stamina->IsExhausted;

            if (movement->IsRunning)
            {
                desiredVelocity = worldMove * movement->RunSpeed;
            }
            else if (movement->IsCrouching)
            {
                desiredVelocity = worldMove * movement->CrouchSpeed;
            }
            else
            {
                desiredVelocity = worldMove * movement->WalkSpeed;
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
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var body = filter.Body;
            var movement = filter.Movement;
            var stamina = filter.Stamina;

            if (input->Jump.WasPressed && PlayerPhysicsUtils.IsGrounded(frame, in filter) && stamina->Current >= stamina->CostPerJump)
            {
                body->AddLinearImpulse(FPVector3.Up * filter.Movement->JumpForce * body->Mass);

                frame.Signals.OnPlayerJump(filter.Entity);
            }
        }

        private void HandleCrouching(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var movement = filter.Movement;

            ref Shape3D shape = ref filter.Collider->Shape;

            FP radius = shape.Capsule.Radius;

            FP currentHalfHeight = shape.Capsule.Height * FP._0_50;

            FP targetHalfHeight;

            if (input->Crouch.IsDown)
            {
                movement->IsCrouching = true;
                targetHalfHeight = movement->HeightCrouching * FP._0_50;
            }
            else
            {
                targetHalfHeight = movement->HeightStanding * FP._0_50;
            }

            if (movement->IsCrouching && !input->Crouch.IsDown)
            {
                if (PlayerPhysicsUtils.CanStandUp(frame, in filter))
                {
                    movement->IsCrouching = false;
                }
                else
                {
                    targetHalfHeight = movement->HeightCrouching * FP._0_50;
                }
            }

            if (currentHalfHeight == targetHalfHeight) return;

            FP newHalfHeight = FPMath.Lerp(currentHalfHeight, targetHalfHeight, frame.DeltaTime * movement->CrouchLerpSpeed);

            FPVector3 posOffset = new FPVector3(0, newHalfHeight, 0);

            shape = Shape3D.CreateCapsule(radius, newHalfHeight - shape.Capsule.Radius, posOffset);
        }
    }
}