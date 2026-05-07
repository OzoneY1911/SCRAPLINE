using Photon.Deterministic;
using Quantum;
using Quantum.Collections;

public unsafe static class ProceduralGenerator
{
    private struct GeneratedMapData
    {
        public DynamicMap Map;
        public QList<FPBounds3> Bounds;
        public QList<FPVector3> NavVertices;
        public QList<CustomNavMeshTriangle> NavTriangles;

        public ushort RoomCount;

        public QList<ProceduralRoom> AllRooms;

        public ProceduralRoom StartRoom;
        public ProceduralRoom DeadEndRoom;
        public ProceduralRoom QuotaZoneRoom;

        public GeneratedMapData(Frame frame, InteractableMapChanger* config)
        {
            var sourceMap = frame.FindAsset(config->SourceMapAsset);
            Map = DynamicMap.FromStaticMap<DynamicMap>(frame.FindAsset(config->SourceMapAsset));
            Map.MapEntities = sourceMap.MapEntities;

            Bounds = frame.AllocateList<FPBounds3>();
            NavVertices = frame.AllocateList<FPVector3>();
            NavTriangles = frame.AllocateList<CustomNavMeshTriangle>();

            RoomCount = config->RoomCount;

            AllRooms = frame.ResolveList<ProceduralRoom>(config->ProceduralRooms);
            StartRoom = config->StartRoom;
            DeadEndRoom = config->DeadEndRoom;
            QuotaZoneRoom = config->QuotaZoneRoom;
        }
    }

    public static DynamicMap GenerateMap(Frame frame, InteractableMapChanger* config)
    {
        var generatedMapData = new GeneratedMapData(frame, config);

        var startRoomMap = frame.FindAsset<Map>(generatedMapData.StartRoom.MapAsset);
        var startRoomCustomData = frame.FindAsset<MapCustomData>(startRoomMap.UserAsset);

        GenerateRoom(frame, generatedMapData.StartRoom, MapPointData.Default, ref generatedMapData);
        GenerateTree(frame, startRoomCustomData.RoomExitPoints, ref generatedMapData);

        //var newNavMesh = NavMeshUtils.WeldAndBakeNavMesh(frame, generatedMapData.Map, generatedMapData.NavVertices, generatedMapData.NavTriangles, FP._0_30);
        //frame.AddAsset(newNavMesh);
        //generatedMapData.Map.NavMeshAssets = new AssetRef<NavMesh>[] { newNavMesh };
        return generatedMapData.Map;
    }

    private static void GenerateTree(Frame frame, MapPointData[] treeExitPoints, ref GeneratedMapData mapData)
    {
        QList<MapPointData> roomSpawnPoints = frame.AllocateList<MapPointData>();
        QList<MapPointData> roomExitPoints = frame.AllocateList<MapPointData>();

        foreach (var treeExitPoint in treeExitPoints)
        {
            roomSpawnPoints.Add(treeExitPoint);
        }

        ushort currentRoomCount = 0;

        while (roomSpawnPoints.Count > 0)
        {
            foreach (var roomSpawnPoint in roomSpawnPoints)
            {
                if (currentRoomCount >= mapData.RoomCount || !TryFindSuitableRoom(frame, ref mapData, roomSpawnPoint, out var room, out var roomMap))
                {
                    GenerateDeadRoom(frame, roomSpawnPoint, ref mapData);
                    continue;
                }

                GenerateRoom(frame, room, roomSpawnPoint, ref mapData);
                currentRoomCount++;

                var spawnRotation = roomSpawnPoint.Rotation;
                var customData = frame.FindAsset<MapCustomData>(roomMap.UserAsset);
                foreach (var roomExitPoint in customData.RoomExitPoints)
                {
                    var offsetExitPoint = MapPointData.Default;

                    offsetExitPoint.Position = roomSpawnPoint.Position + (spawnRotation * roomExitPoint.Position);
                    offsetExitPoint.Rotation = spawnRotation * roomExitPoint.Rotation;

                    roomExitPoints.Add(offsetExitPoint);
                }
            }

            roomSpawnPoints.Clear();

            var temp = roomSpawnPoints;
            roomSpawnPoints = roomExitPoints;
            roomExitPoints = temp;

            roomExitPoints.Clear();
        }
    }

    private static void GenerateRoom(Frame frame, ProceduralRoom room, MapPointData spawnPoint, ref GeneratedMapData mapData)
    {
        var roomMap = frame.FindAsset<Map>(room.MapAsset);

        mapData.Bounds.Add(roomMap.GetMapBounds(spawnPoint));
        AddCollidersFromMap(frame, roomMap, ref mapData, spawnPoint);

        var roomEntity = frame.Create(room.Prototype);
        var roomTransform = frame.Unsafe.GetPointer<Transform3D>(roomEntity);
        roomTransform->Position = spawnPoint.Position;
        roomTransform->Rotation = spawnPoint.Rotation;

        EntityUtils.SyncEntityGroupTransform(frame, roomEntity, spawnPoint);

        //NavMeshUtils.AddNavMeshData(frame, roomMap, spawnPoint, mapData.NavVertices, mapData.NavTriangles);
    }

    private static void GenerateDeadRoom(Frame frame, MapPointData spawnPoint, ref GeneratedMapData mapData)
    {
        GenerateRoom(frame, mapData.DeadEndRoom, spawnPoint, ref mapData);
    }

    private static ProceduralRoom GetRandomRoom(Frame frame, ref GeneratedMapData mapData)
    {
        return mapData.AllRooms[frame.RNG->Next(0, mapData.AllRooms.Count)];
    }

    private static bool TryFindSuitableRoom(Frame frame, ref GeneratedMapData mapData, MapPointData spawnPoint, out ProceduralRoom room, out Map roomMap)
    {
        room = mapData.DeadEndRoom;
        roomMap = frame.FindAsset<Map>(room.MapAsset);

        for (int i = 0; i < mapData.AllRooms.Count; i++)
        {
            var candidateRoom = GetRandomRoom(frame, ref mapData);
            var candidateMap = frame.FindAsset<Map>(candidateRoom.MapAsset);

            var candidateBounds = candidateMap.GetMapBounds(spawnPoint);

            if (!candidateBounds.OverlapsCollection(mapData.Bounds))
            {
                room = candidateRoom;
                roomMap = candidateMap;
                return true;
            }
        }
        return false;
    }

    private static void AddCollidersFromMap(Frame frame, Map roomMap, ref GeneratedMapData mapData, MapPointData roomSpawnPoint)
    {
        foreach (var collider in roomMap.StaticColliders3D)
        {
            var offsetCollider = collider;

            offsetCollider.Position = roomSpawnPoint.Position + (roomSpawnPoint.Rotation * collider.Position);
            offsetCollider.Rotation = roomSpawnPoint.Rotation * collider.Rotation;

            mapData.Map.AddCollider3D(frame, offsetCollider);
        }
    }
}