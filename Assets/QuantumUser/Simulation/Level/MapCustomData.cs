using Photon.Deterministic;
using System;

namespace Quantum
{
    public unsafe class MapCustomData : AssetObject
    {
        [Serializable]
        public struct SpawnPointData
        {
            public FPVector3 Position;
            public FPQuaternion Rotation;
        }

        public SpawnPointData DefaultSpawnPoint;
        public SpawnPointData[] SpawnPoints;

        public void SetEntityToSpawnPoint(Frame frame, EntityRef entity, Int32? index)
        {
            var spawnPoint = index.HasValue && index.Value < SpawnPoints.Length
                ? SpawnPoints[index.Value]
                : DefaultSpawnPoint;

            SetToSpawnPoint(frame, entity, spawnPoint);
        }

        public void SetEntityToRandomSpawnPoint(Frame frame, EntityRef entity)
        {
            var index = frame.RNG->Next(0, SpawnPoints.Length);
            var spawnPoint = SpawnPoints.Length > 0
                ? SpawnPoints[index]
                : DefaultSpawnPoint;

            SetToSpawnPoint(frame, entity, spawnPoint);
        }

        private void SetToSpawnPoint(Frame frame, EntityRef entity, SpawnPointData spawnPoint)
        {
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);

            transform->Position = spawnPoint.Position;
            transform->Rotation = spawnPoint.Rotation;
        }
    }
}
