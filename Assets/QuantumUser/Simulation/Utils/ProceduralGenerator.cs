using Photon.Deterministic;
using Quantum;
using System.Collections.Generic;

public unsafe static class ProceduralGenerator
{
    public static void GenerateMap(Frame frame, Interactable* interactable, out DynamicMap generatedMap)
    {
        var changer = frame.Unsafe.GetPointer<InteractableMapChanger>(interactable->Entity);

        var baseMap = frame.FindAsset(changer->ProceduralMapAsset);
        generatedMap = DynamicMap.FromStaticMap<DynamicMap>(baseMap);

        var allRooms = frame.ResolveList<ProceduralRoom>(changer->ProceduralRooms);
        var deadEnd = changer->DeadEndRoom;

        var startTransform = Transform3D.Create(FPVector3.Zero, FPQuaternion.Identity);
        var startRoom = allRooms[frame.RNG->Next(0, allRooms.Count)];

        List<FPBounds3> placedBounds = new();
        int maxRooms = changer->ProceduralMapSize;

        // Place starting room
        var rootNode = new RoomNode(startRoom, startTransform);
        var roomQueue = new Queue<RoomNode>();
        roomQueue.Enqueue(rootNode);

        PlaceRoom(frame, ref generatedMap, ref rootNode, placedBounds);

        int roomCount = 1;

        // --- Breadth-first room generation ---
        while (roomQueue.Count > 0 && roomCount < maxRooms)
        {
            var currentNode = roomQueue.Dequeue();

            var exits = frame.ResolveList<FPPoint>(currentNode.Room.ExitPoints);

            foreach (var exit in exits)
            {
                if (roomCount >= maxRooms)
                    break;

                // Compute world-space transform of exit
                var exitWorld = Transform3D.Create(
                    currentNode.Transform.Position + currentNode.Transform.Rotation * exit.Position,
                    currentNode.Transform.Rotation * FPQuaternion.Euler(exit.RotationEuler)
                );

                bool placed = false;

                // Try several random rooms
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    var candidate = allRooms[frame.RNG->Next(0, allRooms.Count)];
                    var candidateMap = frame.FindAsset(candidate.MapAsset);
                    var candidateBounds = candidateMap.GetMapBounds(exitWorld);

                    if (!IsOverlapping(candidateBounds, placedBounds))
                    {
                        var node = new RoomNode(candidate, exitWorld);
                        PlaceRoom(frame, ref generatedMap, ref node, placedBounds);
                        roomQueue.Enqueue(node);
                        placed = true;
                        roomCount++;
                        break;
                    }
                }

                // If no room fit, add a dead-end
                if (!placed)
                {
                    var deadEndMap = frame.FindAsset(deadEnd.MapAsset);
                    var deadEndBounds = deadEndMap.GetMapBounds(exitWorld);
                    if (!IsOverlapping(deadEndBounds, placedBounds))
                    {
                        var deadNode = new RoomNode(deadEnd, exitWorld);
                        PlaceRoom(frame, ref generatedMap, ref deadNode, placedBounds);
                        roomCount++;
                    }
                }
            }
        }
    }

    private static bool IsOverlapping(FPBounds3 bounds, List<FPBounds3> placed)
    {
        foreach (var b in placed)
            if (bounds.Overlaps(b))
                return true;
        return false;
    }

    private static void PlaceRoom(Frame frame, ref DynamicMap map, ref RoomNode node, List<FPBounds3> placedBounds)
    {
        var mapAsset = frame.FindAsset(node.Room.MapAsset);
        var bounds = mapAsset.GetMapBounds(node.Transform);

        placedBounds.Add(bounds);

        // Add colliders
        foreach (var collider in mapAsset.StaticColliders3D)
        {
            var offsetCollider = collider;
            offsetCollider.Position = node.Transform.Position + node.Transform.Rotation * collider.Position;
            offsetCollider.Rotation = node.Transform.Rotation * collider.Rotation;
            map.AddCollider3D(frame, offsetCollider);
        }

        // Spawn room entity
        var entity = frame.Create(node.Room.Prototype);
        var transform = frame.Unsafe.GetPointer<Transform3D>(entity);
        transform->Teleport(frame, node.Transform.Position);
        transform->Teleport(frame, node.Transform.Rotation);
    }

    private struct RoomNode
    {
        public ProceduralRoom Room;
        public Transform3D Transform;

        public RoomNode(ProceduralRoom room, Transform3D transform)
        {
            Room = room;
            Transform = transform;
        }
    }
}
