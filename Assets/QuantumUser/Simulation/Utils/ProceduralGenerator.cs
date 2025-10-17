using Photon.Deterministic;
using Quantum;
using Quantum.Collections;

public unsafe static class ProceduralGenerator
{
    public static DynamicMap GenerateMap(Frame frame, Interactable* interactable)
    {
        var generatedMapData = new GeneratedMapData(
            frame,
            frame.Unsafe.GetPointer<InteractableMapChanger>(interactable->Entity)
            );

        var spawnPoint = new FPPoint();
        var startRoomExitPoints = frame.ResolveList<FPPoint>(generatedMapData.StartRoom.ExitPoints);

        GenerateRoom(frame, generatedMapData.StartRoom, spawnPoint, ref generatedMapData);

        foreach (var startRoomExitPoint in startRoomExitPoints)
        {
            GenerateBranch(frame, startRoomExitPoint, ref generatedMapData);
        }

        return generatedMapData.Map;
    }

    private static void GenerateBranch(Frame frame, FPPoint spawnPoint, ref GeneratedMapData mapData)
    {
        QList<FPPoint> currentExitPoints = frame.AllocateList<FPPoint>();
        QList<FPPoint> nextExitPoints = frame.AllocateList<FPPoint>();

        currentExitPoints.Add(spawnPoint);

        ushort branchDepth = 0;

        while (currentExitPoints.Count > 0)
        {
            if (branchDepth >= mapData.BranchDepth) break;

            foreach (var currentExitPoint in currentExitPoints)
            {
                var randomRoom = GetRandomRoom(frame, ref mapData);

                var randomRoomMap = frame.FindAsset(randomRoom.MapAsset);
                var roomBounds = randomRoomMap.GetMapBounds(currentExitPoint);
                if (roomBounds.OverlapsCollection(mapData.Bounds))
                {
                    TryGenerateDeadRoom(frame, currentExitPoint, ref mapData);
                    continue;
                }

                // direction from root to this exit in world space
                // get branch root rotation as quaternion
                // right axis in world space (local +X)
                // signed lateral distance from the branch's center line
                var delta = currentExitPoint.Position - spawnPoint.Position;
                var rootRight = FPQuaternion.Euler(spawnPoint.RotationEuler) * FPVector3.Right;
                var rootRightDistance = FPMath.Abs(FPVector3.Dot(delta, rootRight));

                if (rootRightDistance > mapData.BranchWidth)
                {
                    TryGenerateDeadRoom(frame, currentExitPoint, ref mapData);
                    continue;
                }

                // Generate this room
                GenerateRoom(frame, randomRoom, currentExitPoint, ref mapData);

                // Calculate world rotation of this room
                var spawnRotation = FPQuaternion.Euler(currentExitPoint.RotationEuler);
                var exitPoints = frame.ResolveList<FPPoint>(randomRoom.ExitPoints);

                foreach (var exitPoint in exitPoints)
                {
                    var offsetExitPoint = new FPPoint();
                    offsetExitPoint.Position = currentExitPoint.Position + spawnRotation * exitPoint.Position;
                    offsetExitPoint.RotationEuler = (spawnRotation * FPQuaternion.Euler(exitPoint.RotationEuler)).AsEuler;

                    nextExitPoints.Add(offsetExitPoint);
                }
            }

            if (branchDepth + 1 >= mapData.BranchDepth || nextExitPoints.Count == 0)
            {
                foreach (var exitPoint in currentExitPoints)
                {
                    TryGenerateDeadRoom(frame, exitPoint, ref mapData);
                }
            }

            currentExitPoints.Clear();
            var temp = currentExitPoints;
            currentExitPoints = nextExitPoints;
            nextExitPoints = temp;

            branchDepth++;

            if (branchDepth >= mapData.BranchDepth)
            {
                foreach (var exitPoint in currentExitPoints)
                {
                    TryGenerateDeadRoom(frame, exitPoint, ref mapData);
                }
                break;
            }
        }
    }

    private static void GenerateRoom(Frame frame, ProceduralRoom room, FPPoint spawnPoint, ref GeneratedMapData mapData)
    {
        var mapAsset = frame.FindAsset(room.MapAsset);
        var bounds = mapAsset.GetMapBounds(spawnPoint);

        mapData.Bounds.Add(bounds);

        var spawnRotation = FPQuaternion.Euler(spawnPoint.RotationEuler);

        // Add colliders
        foreach (var collider in mapAsset.StaticColliders3D)
        {
            var offsetCollider = collider;
            offsetCollider.Position = spawnPoint.Position + spawnRotation * collider.Position;
            offsetCollider.Rotation = spawnRotation * collider.Rotation;
            mapData.Map.AddCollider3D(frame, offsetCollider);
        }

        // Spawn room entity
        var roomEntity = frame.Create(room.Prototype);
        var roomTransform = frame.Unsafe.GetPointer<Transform3D>(roomEntity);
        roomTransform->Teleport(frame, spawnPoint.Position);
        roomTransform->Teleport(frame, spawnRotation);
    }

    private static void TryGenerateDeadRoom(Frame frame, FPPoint spawnPoint, ref GeneratedMapData mapData, bool checkOverlap = true)
    {
        var deadRoomMap = frame.FindAsset(mapData.DeadEndRoom.MapAsset);
        var deadRoomBounds = deadRoomMap.GetMapBounds(spawnPoint);
        if (!deadRoomBounds.OverlapsCollection(mapData.Bounds))
        {
            GenerateRoom(frame, mapData.DeadEndRoom, spawnPoint, ref mapData);
        }
    }

    private static ProceduralRoom GetRandomRoom(Frame frame, ref GeneratedMapData mapData)
    {
        var randomIndex = frame.RNG->Next(0, mapData.AllRooms.Count);
        return mapData.AllRooms[randomIndex]; ;
    }

    private struct GeneratedMapData
    {
        public DynamicMap Map;
        public QList<FPBounds3> Bounds;
        public ushort BranchDepth;
        public ushort BranchWidth;
        public QList<ProceduralRoom> AllRooms { get; private set; }
        public ProceduralRoom StartRoom { get; private set; }
        public ProceduralRoom DeadEndRoom { get; private set; }

        public GeneratedMapData(Frame frame, InteractableMapChanger* config)
        {
            Map = DynamicMap.FromStaticMap<DynamicMap>(
                frame.FindAsset(config->SourceMapAsset)
                );
            Bounds = frame.AllocateList<FPBounds3>();
            BranchDepth = config->BranchDepth;
            BranchWidth = config->BranchWidth;
            AllRooms = frame.ResolveList<ProceduralRoom>(config->ProceduralRooms);
            StartRoom = config->StartRoom;
            DeadEndRoom = config->DeadEndRoom;
        }
    }
}