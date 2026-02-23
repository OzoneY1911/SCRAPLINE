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

            public static SpawnPointData Default =>
                new SpawnPointData
                {
                    Position = FPVector3.Zero,
                    Rotation = FPQuaternion.Identity
                };
        }

        [Serializable]
        public struct ValuableSpawnPointData
        {
            public SpawnPointData Data;
            public AssetRef<EntityPrototype>[] PossibleValuables;
        }

        public SpawnPointData[] PlayerSpawnPoints;
        public ValuableSpawnPointData[] ValuableSpawnPoints;

        public void SetPlayerToRandomSpawnPoint(Frame frame, EntityRef entity)
        {
            SetEntityToRandomSpawnPoint(frame, entity, PlayerSpawnPoints);
        }

        public void SpawnValuables(Frame frame, bool inShop = false)
        {
            for (int i = 0; i < ValuableSpawnPoints.Length; i++)
            {
                var index = frame.RNG->Next(0, ValuableSpawnPoints[i].PossibleValuables.Length);

                var valuableEntity = frame.Create(ValuableSpawnPoints[i].PossibleValuables[index]);
                SetToSpawnPoint(frame, valuableEntity, ValuableSpawnPoints[i].Data);

                if (inShop)
                {
                    if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
                    valuable->IsShopValuable = true;
                }
            }
        }

        private void SetEntityToRandomSpawnPoint(Frame frame, EntityRef entity, SpawnPointData[] spawnPoints)
        {
            var index = frame.RNG->Next(0, spawnPoints.Length);

            SpawnPointData spawnPoint = SpawnPointData.Default;
            if (spawnPoints.Length > 0)
            {
                spawnPoint = spawnPoints[index];
            }

            SetToSpawnPoint(frame, entity, spawnPoint);
        }

        public void SetToSpawnPoint(Frame frame, EntityRef entity, SpawnPointData spawnPoint)
        {
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);

            transform->Position = spawnPoint.Position;
            transform->Rotation = spawnPoint.Rotation;
        }
    }
}
