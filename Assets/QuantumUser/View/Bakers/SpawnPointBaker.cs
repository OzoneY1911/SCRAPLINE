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
            if (!QuantumUnityDB.TryGetGlobalAsset<MapCustomData>(data.GetAsset(true).UserAsset, out var customData)) return;

            var mapPoints = GameObject.FindObjectsByType<MapPoint>(FindObjectsSortMode.None);
            var valuableSpawnPoints = GameObject.FindObjectsByType<ValuableSpawnPoint>(FindObjectsSortMode.None);

            List<MapPoint> playerSpawnPoints = new();
            List<MapPoint> monsterSpawnPoints = new();
            List<MapPoint> monsterPatrolPoints = new();
            List<MapPoint> roomExitPoints = new();

            for (int i = 0; i < mapPoints.Length; i++)
            {
                switch (mapPoints[i].Type)
                {
                    case MapPointType.PlayerSpawnPoint:
                        playerSpawnPoints.Add(mapPoints[i]);
                        break;
                    case MapPointType.MonsterSpawnPoint:
                        monsterSpawnPoints.Add(mapPoints[i]);
                        break;
                    case MapPointType.MonsterPatrolPoint:
                        monsterPatrolPoints.Add(mapPoints[i]);
                        break;
                    case MapPointType.RoomExitPoint:
                        roomExitPoints.Add(mapPoints[i]);
                        break;
                }
            }

            BakeMapPoints(playerSpawnPoints.ToArray(), ref customData.PlayerSpawnPoints);
            BakeMapPoints(monsterSpawnPoints.ToArray(), ref customData.MonsterSpawnPoints);
            BakeMapPoints(monsterPatrolPoints.ToArray(), ref customData.MonsterPatrolPoints);
            BakeMapPoints(roomExitPoints.ToArray(), ref customData.RoomExitPoints);
            BakeValuableSpawnPoints(valuableSpawnPoints, ref customData.ValuableSpawnPoints);

#if UNITY_EDITOR
            Debug.Log($"Baked {customData.PlayerSpawnPoints.Length} Player Spawn Points");
            Debug.Log($"Baked {customData.MonsterSpawnPoints.Length} Monster Spawn Points");
            Debug.Log($"Baked {customData.MonsterPatrolPoints.Length} Monster Patrol Points");
            Debug.Log($"Baked {customData.RoomExitPoints.Length} Room Exit Points");
            Debug.Log($"Baked {customData.ValuableSpawnPoints.Length} Valuable Spawn Points");
            EditorUtility.SetDirty(customData);
#endif
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

        private void BakeValuableSpawnPoints(ValuableSpawnPoint[] spawnPoints, ref ValuableSpawnPointData[] targetArray)
        {
            targetArray = new ValuableSpawnPointData[spawnPoints.Length];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                targetArray[i].Data.Position = spawnPoints[i].transform.position.ToFPVector3();
                targetArray[i].Data.Rotation = spawnPoints[i].transform.localRotation.ToFPQuaternion();
                targetArray[i].PossibleValuables = spawnPoints[i].PossibleValuables;
            }
        }
    }
}
