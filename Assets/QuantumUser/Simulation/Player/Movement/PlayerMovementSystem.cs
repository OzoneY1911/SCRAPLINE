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
            public KCC* KCC;
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

            player->LookPitch = FPMath.Clamp(player->LookPitch, -FP.FromFloat_UNSAFE(89f), FP.FromFloat_UNSAFE(89f));

            filter.KCC->SetLookRotation(0, player->LookYaw);
        }

        private void HandleMovement(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var movement = filter.Movement;
            var stamina = filter.Stamina;

            FPVector3 localMove = new FPVector3(input->MoveDirection.X, 0, input->MoveDirection.Y);

            FPQuaternion rotation = filter.Transform->Rotation;
            FPVector3 worldMove = rotation * localMove;

            if (worldMove.SqrMagnitude > FP._0)
                worldMove = worldMove.Normalized;

            movement->IsRunning =
                input->Run.IsDown &&
                localMove != FPVector3.Zero &&
                !movement->IsCrouching &&
                !stamina->IsExhausted;

            FP speed;
            if (movement->IsRunning)
                speed = movement->RunSpeed;
            else if (movement->IsCrouching)
                speed = movement->CrouchSpeed;
            else
                speed = movement->WalkSpeed;

            // ---- KCC ADD-ON INPUT ----
            var kcc = filter.KCC;

            kcc->SetKinematicVelocity(worldMove * speed);
        }

        private void HandleJumping(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);
            var stamina = filter.Stamina;
            var kcc = filter.KCC;

            if (input->Jump.WasPressed &&
                kcc->IsGrounded &&
                stamina->Current >= stamina->CostPerJump)
            {
                kcc->AddExternalImpulse(new FPVector3(0, filter.Movement->JumpForce, 0));

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