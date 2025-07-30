using Photon.Deterministic;
using UnityEngine.LowLevel;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public PhysicsBody3D* Body;
            public Transform3D* Transform;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            if (!player->PlayerRef.IsValid) return;

            var input = frame.GetPlayerInput(player->PlayerRef);
            var body = filter.Body;

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

            FPVector3 desiredVelocity = worldMove * player->MoveSpeed;

            // Current velocity
            FPVector3 currentVel = body->Velocity;

            // Only control horizontal movement (ignore Y velocity)
            FPVector3 horizontalVel = new FPVector3(currentVel.X, 0, currentVel.Z);
            FPVector3 deltaVel = desiredVelocity - horizontalVel;

            // Apply impulse to achieve desired horizontal velocity
            body->AddLinearImpulse(deltaVel * body->Mass);
        }
    }
}