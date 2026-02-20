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
            var spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

            if (customData == null || spawnPoints.Length == 0) return;

            var defaultSpawnPoint = spawnPoints[0];
            if (customData.DefaultSpawnPoint.Equals(default(MapCustomData.SpawnPointData)))
            {
                customData.DefaultSpawnPoint.Position = defaultSpawnPoint.transform.position.ToFPVector3();
                customData.DefaultSpawnPoint.Rotation = defaultSpawnPoint.transform.rotation.ToFPQuaternion();
            }

            customData.SpawnPoints = new MapCustomData.SpawnPointData[spawnPoints.Length];
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                customData.SpawnPoints[i].Position = spawnPoints[i].transform.position.ToFPVector3();
                customData.SpawnPoints[i].Rotation = spawnPoints[i].transform.rotation.ToFPQuaternion();
            }

#if UNITY_EDITOR
            Debug.Log($"Baked {customData.SpawnPoints.Length} Spawn Points");
            EditorUtility.SetDirty(customData);
#endif
        }
    }
}
