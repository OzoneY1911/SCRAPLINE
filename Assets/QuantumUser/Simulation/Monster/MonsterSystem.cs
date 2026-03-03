using Photon.Deterministic;

namespace Quantum
{
    public unsafe class MonsterSystem : SystemSignalsOnly, ISignalOnNavMeshWaypointReached, ISignalOnComponentAdded<NavMeshPathfinder>
    {
        public void OnAdded(Frame frame, EntityRef entity, NavMeshPathfinder* pathfinder)
        {
            SetPatrolTarget(frame, pathfinder);
        }

        public void OnNavMeshWaypointReached(Frame frame, EntityRef entity, FPVector3 waypoint, Navigation.WaypointFlag waypointFlags, ref bool resetAgent)
        {
            if (!frame.Unsafe.TryGetPointer<NavMeshPathfinder>(entity, out var pathfinder)) return;

            resetAgent = false;
            SetPatrolTarget(frame, pathfinder);
        }

        private void SetPatrolTarget(Frame frame, NavMeshPathfinder* pathfinder)
        {
            var customData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);

            var randomPatrolPoint = customData.MonsterPatrolPoints[frame.RNG->Next(0, customData.MonsterPatrolPoints.Length)];
            pathfinder->SetTarget(frame, randomPatrolPoint.Position, frame.Map.NavMeshes["Navmesh"]);
        }
    }
}
