using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[assembly: Quantum.QuantumMapBakeAssemblyAttribute]

namespace Quantum
{
    public class SpawnPointBaker : MapDataBakerCallback
    {
        public override void OnBeforeBake(QuantumMapData data) { }

        public override void OnBake(QuantumMapData data)
        {
            if (!QuantumUnityDB.TryGetGlobalAsset<BaseCustomData>(data.GetAsset(true).UserAsset, out var customData)) return;

            var mapPoints = GameObject.FindObjectsByType<MapPoint>(FindObjectsSortMode.None);

            BakeBaseCustomData(ref customData, mapPoints);

            if (customData is RoomCustomData roomData)
            {
                BakeRoomCustomData(ref roomData, mapPoints);
            }
            else if (customData is MapCustomData mapData)
            {
                BakeMapCustomData(ref mapData);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(customData);
#endif
        }

        private void BakeBaseCustomData(ref BaseCustomData data, MapPoint[] mapPoints)
        {
            List<MapPoint> playerSpawnPoints = new();
            List<MapPoint> monsterSpawnPoints = new();
            List<MapPoint> monsterPatrolPoints = new();

            foreach (var mapPoint in mapPoints)
            {
                switch (mapPoint.Type)
                {
                    case MapPointType.PlayerSpawnPoint:
                        playerSpawnPoints.Add(mapPoint);
                        break;
                    case MapPointType.MonsterSpawnPoint:
                        monsterSpawnPoints.Add(mapPoint);
                        break;
                    case MapPointType.MonsterPatrolPoint:
                        monsterPatrolPoints.Add(mapPoint);
                        break;
                }
            }

            BakeMapPoints(playerSpawnPoints.ToArray(), ref data.PlayerSpawnPoints);
            BakeMapPoints(monsterSpawnPoints.ToArray(), ref data.MonsterSpawnPoints);
            BakeMapPoints(monsterPatrolPoints.ToArray(), ref data.MonsterPatrolPoints);

            Debug.Log($"Baked {data.PlayerSpawnPoints.Length} Player Spawn Points");
            Debug.Log($"Baked {data.MonsterSpawnPoints.Length} Monster Spawn Points");
            Debug.Log($"Baked {data.MonsterPatrolPoints.Length} Monster Patrol Points");
        }

        private void BakeRoomCustomData(ref RoomCustomData data, MapPoint[] mapPoints)
        {
            List<MapPoint> roomExitPoints = new();
            List<MapPoint> valuableSpawnPoints = new();

            foreach (var mapPoint in mapPoints)
            {
                switch (mapPoint.Type)
                {
                    case MapPointType.RoomExitPoint:
                        roomExitPoints.Add(mapPoint);
                        break;
                    case MapPointType.ValuableSpawnPoint:
                        valuableSpawnPoints.Add(mapPoint);
                        break;
                }
            }

            BakeMapPoints(roomExitPoints.ToArray(), ref data.RoomExitPoints);
            BakeMapPoints(valuableSpawnPoints.ToArray(), ref data.ValuableSpawnPoints);

            Debug.Log($"Baked {data.RoomExitPoints.Length} Room Exit Points");
            Debug.Log($"Baked {data.ValuableSpawnPoints.Length} Valuable Spawn Points");
        }

        private void BakeMapCustomData(ref MapCustomData data)
        {
            var valuableSpawnPools = GameObject.FindObjectsByType<ValuableSpawnPool>(FindObjectsSortMode.None);

            BakeValuableSpawnPools(valuableSpawnPools, data.ValuableSpawnPools);

            Debug.Log($"Baked {data.ValuableSpawnPools.Count} Valuable Spawn Pools");
        }

        private void BakeMapPoints(MapPoint[] mapPoints, ref MapPointData[] targetArray)
        {
            targetArray = new MapPointData[mapPoints.Length];
            for (var i = 0; i < mapPoints.Length; i++)
            {
                targetArray[i].Position = mapPoints[i].transform.position.ToFPVector3();
                targetArray[i].Rotation = mapPoints[i].transform.localRotation.ToFPQuaternion();
            }
        }

        private void BakeValuableSpawnPools(ValuableSpawnPool[] spawnPools, List<ValuableSpawnPoolData> targetList)
        {
            targetList.Clear();
            for (int i = 0; i < spawnPools.Length; i++)
            {
                targetList.Add(new ValuableSpawnPoolData
                {
                    Point = new MapPointData
                    {
                        Position = spawnPools[i].transform.position.ToFPVector3(),
                        Rotation = spawnPools[i].transform.rotation.ToFPQuaternion()
                    },
                    PossibleValuables = new List<AssetRef<EntityPrototype>>(spawnPools[i].PossibleValuables)
                });
            }
        }
    }
}
