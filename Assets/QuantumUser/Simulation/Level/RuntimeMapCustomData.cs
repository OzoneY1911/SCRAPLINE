using Quantum.Collections;

namespace Quantum
{
    public unsafe partial struct RuntimeMapCustomData
    {
        public static void AddMapPoints(Frame frame, MapPointData[] mapPoints, MapPointData spawnPoint, QListPtr<MapPointData> outputPtr)
        {
            foreach (var mapPoint in mapPoints)
            {
                AddMapPoint(frame, mapPoint, spawnPoint, outputPtr);
            }
        }

        private static void AddMapPoint(Frame frame, MapPointData mapPoint, MapPointData spawnPoint, QListPtr<MapPointData> outputPtr)
        {
            var point = mapPoint;

            point.Position = spawnPoint.Position + spawnPoint.Rotation * point.Position;
            point.Rotation = spawnPoint.Rotation * point.Rotation;

            var output = frame.ResolveList(outputPtr);
            output.Add(point);
        }

        public static void ClearRuntimeData(Frame frame)
        {
            var runtimeData = frame.Global->RuntimeCustomData;

            frame.ResolveList(runtimeData.PlayerSpawnPoints).Clear();
            frame.ResolveList(runtimeData.MonsterSpawnPoints).Clear();
            frame.ResolveList(runtimeData.MonsterPatrolPoints).Clear();
            frame.ResolveList(runtimeData.ValuableSpawnPoints).Clear();

            EntityUtils.ClearEntityList(frame, runtimeData.ProceduralRoomEntities);
            EntityUtils.ClearEntityList(frame, runtimeData.ProceduralValuableEntities);

            frame.ResolveList(runtimeData.ProceduralRoomEntities).Clear();
            frame.ResolveList(runtimeData.ProceduralValuableEntities).Clear();

            frame.Global->RuntimeCustomData = runtimeData;
        }
    }
}