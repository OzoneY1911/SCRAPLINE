using Photon.Deterministic;
using Quantum;
using Quantum.Collections;

public unsafe static class NavMeshUtils
{
    public static void AddNavMeshData(Frame frame, Map roomMap, MapPointData spawnPoint, QList<FPVector3> vertices, QList<CustomNavMeshTriangle> triangles)
    {
        if (roomMap.NavMeshAssets == null || roomMap.NavMeshAssets.Length == 0) return;

        var navMesh = frame.FindAsset<NavMesh>(roomMap.NavMeshAssets[0]);

        int baseIndex = vertices.Count;

        foreach (var v in navMesh.Vertices)
        {
            vertices.Add(spawnPoint.Position + (spawnPoint.Rotation * v.Point));
        }

        foreach (var t in navMesh.Triangles)
        {
            triangles.Add(new CustomNavMeshTriangle
            {
                V0 = t.Vertex0 + baseIndex,
                V1 = t.Vertex1 + baseIndex,
                V2 = t.Vertex2 + baseIndex,
                Cost = t.Cost
            });
        }
    }

    public static NavMesh WeldAndBakeNavMesh(Frame frame, DynamicMap map, QList<FPVector3> vertices, QList<CustomNavMeshTriangle> triangles, FP weldEpsilon)
    {
        WeldVertices(frame, vertices, triangles, weldEpsilon);
        RemoveDegenerateTriangles(triangles);
        RemoveDuplicateTriangles(triangles);
        return Bake(frame, map, vertices, triangles);
    }

    private static void WeldVertices(Frame frame, QList<FPVector3> vertices, QList<CustomNavMeshTriangle> triangles, FP epsilon)
    {
        FP epsilonSqr = epsilon * epsilon;

        QList<FPVector3> weldedVertices = frame.AllocateList<FPVector3>();
        QList<int> remap = frame.AllocateList<int>();

        for (int i = 0; i < vertices.Count; i++)
        {
            FPVector3 vertex = vertices[i];
            int foundIndex = -1;

            for (int j = 0; j < weldedVertices.Count; j++)
            {
                FPVector3 diff = weldedVertices[j] - vertex;

                if (FPVector3.Dot(diff, diff) <= epsilonSqr)
                {
                    foundIndex = j;
                    break;
                }
            }

            if (foundIndex == -1)
            {
                foundIndex = weldedVertices.Count;
                weldedVertices.Add(vertex);
            }

            remap.Add(foundIndex);
        }

        for (int i = 0; i < triangles.Count; i++)
        {
            var triangle = triangles[i];

            triangle.V0 = remap[triangle.V0];
            triangle.V1 = remap[triangle.V1];
            triangle.V2 = remap[triangle.V2];

            triangles[i] = triangle;
        }

        vertices.Clear();

        foreach (var vertex in weldedVertices)
        {
            vertices.Add(vertex);
        }
    }

    private static void RemoveDegenerateTriangles(QList<CustomNavMeshTriangle> triangles)
    {
        for (int i = triangles.Count - 1; i >= 0; i--)
        {
            var triangle = triangles[i];

            if (triangle.V0 == triangle.V1 ||
                triangle.V0 == triangle.V2 ||
                triangle.V1 == triangle.V2)
            {
                triangles.RemoveAt(i);
            }
        }
    }

    private static void RemoveDuplicateTriangles(QList<CustomNavMeshTriangle> triangles)
    {
        for (int i = triangles.Count - 1; i >= 0; i--)
        {
            var a = triangles[i];

            for (int j = i - 1; j >= 0; j--)
            {
                var b = triangles[j];

                bool same =
                    (a.V0 == b.V0 || a.V0 == b.V1 || a.V0 == b.V2) &&
                    (a.V1 == b.V0 || a.V1 == b.V1 || a.V1 == b.V2) &&
                    (a.V2 == b.V0 || a.V2 == b.V1 || a.V2 == b.V2);

                if (same)
                {
                    triangles.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private static NavMesh Bake(Frame frame, DynamicMap map, QList<FPVector3> vertices, QList<CustomNavMeshTriangle> triangles)
    {
        var bakeVertices = new NavMeshBakeDataVertex[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            bakeVertices[i] = new NavMeshBakeDataVertex
            {
                Position = vertices[i]
            };
        }

        var bakeTriangles = new NavMeshBakeDataTriangle[triangles.Count];

        for (int i = 0; i < triangles.Count; i++)
        {
            var triangle = triangles[i];

            bakeTriangles[i] = new NavMeshBakeDataTriangle
            {
                VertexIds = new int[3]
                {
                    triangle.V0,
                    triangle.V1,
                    triangle.V2
                },
                Cost = triangle.Cost,
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
            Links = new NavMeshBakeDataLink[0]
        };

        return NavMeshBaker.BakeNavMesh(map, bakeData);
    }
}