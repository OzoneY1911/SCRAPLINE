using Photon.Deterministic;

namespace Quantum
{
    public unsafe static class PhysicsUtils
    {
        public static bool PlayerIsInRange(Frame frame, FPVector3 originPosition, out EntityRef playerEntity)
        {
            playerEntity = EntityRef.None;
            var checkShape = Shape3D.CreateSphere(3);

            var hits = frame.Physics3D.OverlapShape(originPosition, FPQuaternion.Identity, checkShape, options: QueryOptions.HitDynamics);

            for (int i = 0; i < hits.Count; i++)
            {
                if (frame.Unsafe.TryGetPointer<Player>(hits[i].Entity, out var player))
                {
                    playerEntity = hits[i].Entity;
                    return true;
                }
            }
            return false;
        }
    }
}
