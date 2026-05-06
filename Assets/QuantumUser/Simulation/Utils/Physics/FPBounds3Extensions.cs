using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum
{
    public static class FPBounds3Extensions
    {
        public static FPBounds3 ToWorldBounds(this MapStaticCollider3D collider, MapPointData offsetPoint)
        {
            // Collider local center and half-size
            FPVector3 localCenter = collider.Position;
            FPVector3 localExtents = collider.BoxExtents;

            FPQuaternion offsetRotation = offsetPoint.Rotation;

            // Transform to world
            FPVector3 worldCenter = offsetPoint.Position + offsetRotation * localCenter;

            FPVector3 axisX = offsetRotation * new FPVector3(localExtents.X, FP._0, FP._0);
            FPVector3 axisY = offsetRotation * new FPVector3(FP._0, localExtents.Y, FP._0);
            FPVector3 axisZ = offsetRotation * new FPVector3(FP._0, FP._0, localExtents.Z);

            FPVector3 rotatedExtents = new FPVector3(
                FPMath.Abs(axisX.X) + FPMath.Abs(axisY.X) + FPMath.Abs(axisZ.X),
                FPMath.Abs(axisX.Y) + FPMath.Abs(axisY.Y) + FPMath.Abs(axisZ.Y),
                FPMath.Abs(axisX.Z) + FPMath.Abs(axisY.Z) + FPMath.Abs(axisZ.Z)
            );

            return new FPBounds3(worldCenter, rotatedExtents);
        }

        public static FPBounds3 GetMapBounds(this Map map, MapPointData offsetPoint)
        {
            bool initialized = false;
            FPBounds3 result = default;

            foreach (var collider in map.StaticColliders3D)
            {
                var bounds = collider.ToWorldBounds(offsetPoint);

                if (!initialized)
                {
                    result = bounds;
                    initialized = true;
                }
                else
                {
                    result.Encapsulate(bounds);
                }
            }

            return result;
        }

        public static bool Overlaps(this FPBounds3 a, FPBounds3 b)
        {
            a.Expand(-5);
            return (a.Min.X < b.Max.X && a.Max.X > b.Min.X &&
                    a.Min.Y < b.Max.Y && a.Max.Y > b.Min.Y &&
                    a.Min.Z < b.Max.Z && a.Max.Z > b.Min.Z);
        }

        public static bool OverlapsCollection(this FPBounds3 bounds, QList<FPBounds3> collection)
        {
            foreach (var bound in collection)
            {
                if (bounds.Overlaps(bound)) return true;
            }
            return false;
        }
    }
}
