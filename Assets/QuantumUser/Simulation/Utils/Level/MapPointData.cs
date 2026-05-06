using Photon.Deterministic;
using System;

namespace Quantum
{
    [Serializable]
    public struct MapPointData
    {
        public FPVector3 Position;
        public FPQuaternion Rotation;

        public static MapPointData Default =>
            new MapPointData
            {
                Position = FPVector3.Zero,
                Rotation = FPQuaternion.Identity
            };
    }

    [Serializable]
    public struct ValuableSpawnPointData
    {
        public MapPointData Data;
        public AssetRef<EntityPrototype>[] PossibleValuables;
    }
}
