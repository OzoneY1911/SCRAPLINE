using Quantum.Collections;

namespace Quantum
{
    public partial struct RuntimeMapCustomData
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
    }
}