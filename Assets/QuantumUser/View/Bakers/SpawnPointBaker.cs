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

            var playerSpawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");
            var valuableSpawnPoints = GameObject.FindObjectsByType<ValuableSpawnPoint>(FindObjectsSortMode.None);

            customData.PlayerSpawnPoints = new MapCustomData.SpawnPointData[playerSpawnPoints.Length];
            for (var i = 0; i < playerSpawnPoints.Length; i++)
            {
                customData.PlayerSpawnPoints[i].Position = playerSpawnPoints[i].transform.position.ToFPVector3();
                customData.PlayerSpawnPoints[i].Rotation = playerSpawnPoints[i].transform.localRotation.ToFPQuaternion();
            }
            
            customData.ValuableSpawnPoints = new MapCustomData.ValuableSpawnPointData[valuableSpawnPoints.Length];
            for (var i = 0; i < valuableSpawnPoints.Length; i++)
            {
                customData.ValuableSpawnPoints[i].Data.Position = valuableSpawnPoints[i].transform.position.ToFPVector3();
                customData.ValuableSpawnPoints[i].Data.Rotation = valuableSpawnPoints[i].transform.localRotation.ToFPQuaternion();
                customData.ValuableSpawnPoints[i].PossibleValuables = valuableSpawnPoints[i].PossibleValuables;
            }

#if UNITY_EDITOR
            Debug.Log($"Baked {customData.PlayerSpawnPoints.Length} Player Spawn Points");
            Debug.Log($"Baked {customData.ValuableSpawnPoints.Length} Valuable Spawn Points");
            EditorUtility.SetDirty(customData);
#endif
        }
    }
}
