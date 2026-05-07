using Quantum;
using Quantum.Collections;
using System.Collections.Generic;

public static unsafe class MapCustomDataUtils
{
    private static void SetToMapPoint(Frame frame, EntityRef entity, MapPointData mapPoint)
    {
        var transform = frame.Unsafe.GetPointer<Transform3D>(entity);

        transform->Position = mapPoint.Position;
        transform->Rotation = mapPoint.Rotation;
    }

    private static void SetEntityToRandomMapPoint(Frame frame, EntityRef entity, QListPtr<MapPointData> mapPointsPtr)
    {
        var mapPoints = frame.ResolveList(mapPointsPtr);
        var index = frame.RNG->Next(0, mapPoints.Count);

        MapPointData mapPoint = MapPointData.Default;
        if (mapPoints.Count > 0)
        {
            mapPoint = mapPoints[index];
        }

        SetToMapPoint(frame, entity, mapPoint);
    }

    private static void SetEntityToRandomMapPoint(Frame frame, EntityRef entity, MapPointData[] mapPoints)
    {
        var index = frame.RNG->Next(0, mapPoints.Length);

        MapPointData mapPoint = MapPointData.Default;
        if (mapPoints.Length > 0)
        {
            mapPoint = mapPoints[index];
        }

        SetToMapPoint(frame, entity, mapPoint);
    }

    public static void SetPlayerToRandomSpawnPoint(Frame frame, EntityRef entity)
    {
        var mapData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);
        if (mapData.PlayerSpawnPoints.Length > 0)
        {
            SetEntityToRandomMapPoint(frame, entity, mapData.PlayerSpawnPoints);
        }
        else
        {
            SetEntityToRandomMapPoint(frame, entity, frame.Global->RuntimeCustomData.PlayerSpawnPoints);
        }
    }

    public static void SetMonsterToRandomSpawnPoint(Frame frame, EntityRef entity)
    {
        var mapData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);
        if (mapData.MonsterSpawnPoints.Length > 0)
        {
            SetEntityToRandomMapPoint(frame, entity, mapData.MonsterSpawnPoints);
        }
        else
        {
            SetEntityToRandomMapPoint(frame, entity, frame.Global->RuntimeCustomData.MonsterSpawnPoints);
        }
    }

    public static void SpawnValuables(Frame frame)
    {
        var mapData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);
        if (mapData.ValuableSpawnPools.Count > 0)
        {
            SpawnValuablesFromPools(frame, mapData.ValuableSpawnPools);
        }
        else
        {
            SpawnValuablesFromPoints(frame, mapData.PossibleValuables);
        }
    }

    private static void SpawnValuablesFromPools(Frame frame, List<ValuableSpawnPoolData> pools)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            var possibleValuables = pools[i].PossibleValuables;
            var index = frame.RNG->Next(0, possibleValuables.Count);

            var valuableEntity = frame.Create(possibleValuables[index]);
            SetToMapPoint(frame, valuableEntity, pools[i].Point);

            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) continue;
            valuable->IsShopValuable = true;
        }
    }

    private static void SpawnValuablesFromPoints(Frame frame, AssetRef<EntityPrototype>[] possibleValuables)
    {
        var mapPoints = frame.ResolveList(frame.Global->RuntimeCustomData.ValuableSpawnPoints);
        for (int i = 0; i < mapPoints.Count; i++)
        {
            var index = frame.RNG->Next(0, possibleValuables.Length);

            var valuableEntity = frame.Create(possibleValuables[index]);
            SetToMapPoint(frame, valuableEntity, mapPoints[i]);
        }
    }
}