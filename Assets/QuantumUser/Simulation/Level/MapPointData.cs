using Photon.Deterministic;

namespace Quantum
{
    public partial struct MapPointData
    {
        public static MapPointData Default =>
            new MapPointData
            {
                Position = FPVector3.Zero,
                Rotation = FPQuaternion.Identity
            };
    }
}
