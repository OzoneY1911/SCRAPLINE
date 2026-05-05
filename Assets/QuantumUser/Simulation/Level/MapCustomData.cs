using UnityEngine;

namespace Quantum
{
    public unsafe class MapCustomData : AssetObject
    {
        [Header("Monster Settings")]
        public AssetRef<EntityPrototype>[] MonsterPrototypes;

        [Header("Baked Spawn Points")]
        public MapPointData[] PlayerSpawnPoints;
        public MapPointData[] MonsterSpawnPoints;
        public MapPointData[] MonsterPatrolPoints;
        public MapPointData[] RoomExitPoints;
        public ValuableSpawnPointData[] ValuableSpawnPoints;

        private void SetToMapPoint(Frame frame, EntityRef entity, MapPointData mapPoint)
        {
            var transform = frame.Unsafe.GetPointer<Transform3D>(entity);

            transform->Position = mapPoint.Position;
            transform->Rotation = mapPoint.Rotation;
        }

        private void SetEntityToRandomMapPoint(Frame frame, EntityRef entity, MapPointData[] mapPoints)
        {
            var index = frame.RNG->Next(0, mapPoints.Length);

            MapPointData mapPoint = MapPointData.Default;
            if (mapPoints.Length > 0)
            {
                mapPoint = mapPoints[index];
            }

            SetToMapPoint(frame, entity, mapPoint);
        }

        public void SetPlayerToRandomSpawnPoint(Frame frame, EntityRef entity)
        {
            SetEntityToRandomMapPoint(frame, entity, PlayerSpawnPoints);
        }

        public void SetMonsterToRandomSpawnPoint(Frame frame, EntityRef entity)
        {
            SetEntityToRandomMapPoint(frame, entity, MonsterSpawnPoints);
        }

        public void SpawnValuables(Frame frame, bool inShop = false)
        {
            for (int i = 0; i < ValuableSpawnPoints.Length; i++)
            {
                var index = frame.RNG->Next(0, ValuableSpawnPoints[i].PossibleValuables.Length);

                var valuableEntity = frame.Create(ValuableSpawnPoints[i].PossibleValuables[index]);
                SetToMapPoint(frame, valuableEntity, ValuableSpawnPoints[i].Data);

                if (inShop)
                {
                    if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
                    valuable->IsShopValuable = true;
                }
            }
        }
    }
}
