using Photon.Deterministic;

namespace Quantum
{
    public static unsafe class PlayerPhysicsUtils
    {
        public static bool IsGrounded(Frame frame, in PlayerMovementSystem.Filter filter)
        {
            FP yOffset = FP._0_10;

            Shape3D checkShape = filter.Collider->Shape;

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

        public static bool CanStandUp(Frame frame, in PlayerMovementSystem.Filter filter)
        {
            FP radius = filter.Collider->Shape.Capsule.Radius - FP._0_10;
            FP standingHeight = filter.Movement->HeightStanding;
            FPVector3 posOffset = new FPVector3(0, (standingHeight * FP._0_50) + FP._0_10, 0);

            Shape3D standShape = Shape3D.CreateCapsule(radius, (standingHeight * FP._0_50) - radius, posOffset);

            var hits = frame.Physics3D.OverlapShape(filter.Transform->Position, filter.Transform->Rotation, standShape, options: QueryOptions.HitSolids);

            for (int i = 0; i < hits.Count; i++)
            {
                if (hits[i].Entity != filter.Entity)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
