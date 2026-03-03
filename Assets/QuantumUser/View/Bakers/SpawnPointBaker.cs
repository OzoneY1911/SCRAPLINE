using Photon.Realtime;
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
            var customData = QuantumUnityDB.GetGlobalAssetEditorInstance<MapCustomData>(data.GetAsset(true).UserAsset);

            if (customData == null) return;

            var mapPoints = GameObject.FindObjectsByType<MapPoint>(FindObjectsSortMode.None);
            var valuableSpawnPoints = GameObject.FindObjectsByType<ValuableSpawnPoint>(FindObjectsSortMode.None);

            List<MapPoint> playerSpawnPoints = new();
            List<MapPoint> monsterSpawnPoints = new();
            List<MapPoint> monsterPatrolPoints = new();

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
                }
            }

            BakeMapPoints(playerSpawnPoints.ToArray(), ref customData.PlayerSpawnPoints);
            BakeMapPoints(monsterSpawnPoints.ToArray(), ref customData.MonsterSpawnPoints);
            BakeMapPoints(monsterPatrolPoints.ToArray(), ref customData.MonsterPatrolPoints);
            BakeValuableSpawnPoints(valuableSpawnPoints, ref customData.ValuableSpawnPoints);

#if UNITY_EDITOR
            Debug.Log($"Baked {customData.PlayerSpawnPoints.Length} Player Spawn Points");
            Debug.Log($"Baked {customData.MonsterSpawnPoints.Length} Monster Spawn Points");
            Debug.Log($"Baked {customData.MonsterPatrolPoints.Length} Monster Patrol Points");
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
