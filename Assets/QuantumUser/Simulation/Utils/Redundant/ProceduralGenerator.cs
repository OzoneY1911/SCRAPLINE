using Photon.Deterministic;
using Quantum;
using Quantum.Collections;
using System.Linq;

public unsafe static class ProceduralGenerator
{
    public static DynamicMap GenerateMap(Frame frame, Interactable* interactable)
    {
        var generatedMapData = new GeneratedMapData(
            frame,
            frame.Unsafe.GetPointer<InteractableMapChanger>(interactable->Entity)
        );

        var startRoomMap = frame.FindAsset<Map>(generatedMapData.StartRoom.MapAsset);
        var startRoomMapCustomData = frame.FindAsset<MapCustomData>(startRoomMap.UserAsset);

        GenerateRoom(frame, generatedMapData.StartRoom, MapPointData.Default, ref generatedMapData);

        QList<MapPointData> startExitPoints = frame.AllocateList<MapPointData>();
        foreach (var exit in startRoomMapCustomData.RoomExitPoints)
        {
            startExitPoints.Add(exit);
        }

        GenerateBranch(frame, startExitPoints, ref generatedMapData);

        // ===== BUILD NAVMESH VIA BAKE =====
        var bakeVertices = new NavMeshBakeDataVertex[generatedMapData.NavVertices.Count];

        for (int i = 0; i < bakeVertices.Length; i++)
        {
            bakeVertices[i] = new NavMeshBakeDataVertex
            {
                Position = generatedMapData.NavVertices[i]
            };
        }

        var bakeTriangles = new NavMeshBakeDataTriangle[generatedMapData.NavTriangles.Count];

        for (int i = 0; i < bakeTriangles.Length; i++)
        {
            var t = generatedMapData.NavTriangles[i];

            bakeTriangles[i] = new NavMeshBakeDataTriangle
            {
                VertexIds = new int[3] { t.V0, t.V1, t.V2 },
                Cost = t.Cost,
                RegionId = "Default"
            };
        }

        var bakeData = new NavMeshBakeData
        {
            Name = "GeneratedNavMesh",
            AgentRadius = 0,
            Position = FPVector3.Zero,
            ClosestTriangleCalculation = NavMeshBakeDataFindClosestTriangle.SpiralOut,
            ClosestTriangleCalculationDepth = 2,
            Vertices = bakeVertices,
            Triangles = bakeTriangles,
            Regions = new string[] { "Default" },
            Links = new NavMeshBakeDataLink[0],
        };

        var navmesh = NavMeshBaker.BakeNavMesh(generatedMapData.Map, bakeData);

        frame.AddAsset(navmesh);

        generatedMapData.Map.NavMeshAssets = new AssetRef<NavMesh>[] { navmesh };

        return generatedMapData.Map;
    }

    private static void GenerateBranch(Frame frame, QList<MapPointData> initialExitPoints, ref GeneratedMapData mapData)
    {
        QList<MapPointData> currentExitPoints = frame.AllocateList<MapPointData>();
        QList<MapPointData> nextExitPoints = frame.AllocateList<MapPointData>();

        foreach (var p in initialExitPoints)
            currentExitPoints.Add(p);

        ushort currentBranchDepth = 0;

        while (currentExitPoints.Count > 0)
        {
            if (currentBranchDepth >= mapData.BranchDepth)
                break;

            foreach (var currentExitPoint in currentExitPoints)
            {
                ProceduralRoom room;
                Map mapAsset;
                bool found = false;

                int attempts = mapData.AllRooms.Count;

                for (int i = 0; i < attempts; i++)
                {
                    var candidate = GetRandomRoom(frame, ref mapData);
                    var candidateMap = frame.FindAsset<Map>(candidate.MapAsset);
                    var candidateBounds = candidateMap.GetMapBounds(currentExitPoint);

                    if (!candidateBounds.OverlapsCollection(mapData.Bounds))
                    {
                        room = candidate;
                        mapAsset = candidateMap;
                        found = true;
                        goto ROOM_FOUND;
                    }
                }

                room = mapData.DeadEndRoom;
                mapAsset = frame.FindAsset<Map>(room.MapAsset);

            ROOM_FOUND:
                var delta = currentExitPoint.Position - MapPointData.Default.Position;
                var rootRight = MapPointData.Default.Rotation * FPVector3.Right;
                var rootRightDistance = FPMath.Abs(FPVector3.Dot(delta, rootRight));

                if (rootRightDistance > mapData.BranchWidth)
                {
                    GenerateDeadRoom(frame, currentExitPoint, ref mapData);
                    continue;
                }

                GenerateRoom(frame, room, currentExitPoint, ref mapData);

                if (!found) continue;

                var spawnRotation = currentExitPoint.Rotation;
                var customData = frame.FindAsset<MapCustomData>(mapAsset.UserAsset);

                foreach (var exitPoint in customData.RoomExitPoints)
                {
                    var offsetExitPoint = MapPointData.Default;
                    offsetExitPoint.Position = currentExitPoint.Position + spawnRotation * exitPoint.Position;
                    offsetExitPoint.Rotation = spawnRotation * exitPoint.Rotation;

                    nextExitPoints.Add(offsetExitPoint);
                }
            }

            currentExitPoints.Clear();
            var temp = currentExitPoints;
            currentExitPoints = nextExitPoints;
            nextExitPoints = temp;

            currentBranchDepth++;

            if (currentBranchDepth >= mapData.BranchDepth)
            {
                foreach (var exitPoint in currentExitPoints)
                {
                    GenerateDeadRoom(frame, exitPoint, ref mapData);
                }
                break;
            }
        }
    }

    private static void GenerateRoom(Frame frame, ProceduralRoom room, MapPointData spawnPoint, ref GeneratedMapData mapData)
    {
        var mapAsset = frame.FindAsset<Map>(room.MapAsset);
        var bounds = mapAsset.GetMapBounds(spawnPoint);

        mapData.Bounds.Add(bounds);

        var spawnRotation = spawnPoint.Rotation;
        var spawnPosition = spawnPoint.Position;

        // Merge Colliders
        foreach (var collider in mapAsset.StaticColliders3D)
        {
            var offsetCollider = collider;
            offsetCollider.Position = spawnPosition + spawnRotation * collider.Position;
            offsetCollider.Rotation = spawnRotation * collider.Rotation;
            mapData.Map.AddCollider3D(frame, offsetCollider);
        }

        // Spawn Prototypes
        var roomEntity = frame.Create(room.Prototype);
        var roomTransform = frame.Unsafe.GetPointer<Transform3D>(roomEntity);
        roomTransform->Position = spawnPosition;
        roomTransform->Rotation = spawnRotation;

        // ===== NAVMESH MERGE =====
        if (mapAsset.NavMeshAssets != null && mapAsset.NavMeshAssets.Length > 0)
        {
            var navMesh = frame.FindAsset<NavMesh>(mapAsset.NavMeshAssets[0]);
            int baseIndex = mapData.NavVertices.Count;

            foreach (var v in navMesh.Vertices)
            {
                mapData.NavVertices.Add(
                    spawnPosition + (spawnRotation * v.Point)
                );
            }

            foreach (var t in navMesh.Triangles)
            {
                mapData.NavTriangles.Add(new TempTriangle
                {
                    V0 = t.Vertex0 + baseIndex,
                    V1 = t.Vertex1 + baseIndex,
                    V2 = t.Vertex2 + baseIndex,
                    Cost = t.Cost
                });
            }
        }
    }

    private static void GenerateDeadRoom(Frame frame, MapPointData spawnPoint, ref GeneratedMapData mapData)
    {
        GenerateRoom(frame, mapData.DeadEndRoom, spawnPoint, ref mapData);
    }

    private static ProceduralRoom GetRandomRoom(Frame frame, ref GeneratedMapData mapData)
    {
        int randomIndex = frame.RNG->Next(0, mapData.AllRooms.Count);
        return mapData.AllRooms[randomIndex];
    }

    private struct TempTriangle
    {
        public int V0;
        public int V1;
        public int V2;
        public FP Cost;
    }

    private struct GeneratedMapData
    {
        public DynamicMap Map;
        public QList<FPBounds3> Bounds;
        public QList<FPVector3> NavVertices;
        public QList<TempTriangle> NavTriangles;
        public ushort BranchDepth;
        public ushort BranchWidth;
        public QList<ProceduralRoom> AllRooms;
        public ProceduralRoom StartRoom;
        public ProceduralRoom DeadEndRoom;
        public ProceduralRoom QuotaZoneRoom;

        public GeneratedMapData(Frame frame, InteractableMapChanger* config)
        {
            Map = DynamicMap.FromStaticMap<DynamicMap>(frame.FindAsset(config->SourceMapAsset));
            Bounds = frame.AllocateList<FPBounds3>();
            NavVertices = frame.AllocateList<FPVector3>();
            NavTriangles = frame.AllocateList<TempTriangle>();
            BranchDepth = config->BranchDepth;
            BranchWidth = config->BranchWidth;
            AllRooms = frame.ResolveList<ProceduralRoom>(config->ProceduralRooms);
            StartRoom = config->StartRoom;
            DeadEndRoom = config->DeadEndRoom;
            QuotaZoneRoom = config->QuotaZoneRoom;
        }
    }
}