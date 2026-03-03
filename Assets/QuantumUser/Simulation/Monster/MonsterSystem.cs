using Photon.Deterministic;

namespace Quantum
{
    public unsafe class MonsterSystem : SystemMainThreadFilter<MonsterSystem.Filter>, ISignalOnNavMeshWaypointReached, ISignalOnComponentAdded<NavMeshPathfinder>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Monster* Monster;
            public NavMeshPathfinder* Pathfinder;
        }

        public void OnAdded(Frame frame, EntityRef entity, NavMeshPathfinder* pathfinder)
        {
            frame.Unsafe.GetPointer<Monster>(entity)->State = MonsterState.Patrol;
            SetPatrolTarget(frame, pathfinder);
        }

        public void OnNavMeshWaypointReached(Frame frame, EntityRef entity, FPVector3 waypoint, Navigation.WaypointFlag waypointFlags, ref bool resetAgent)
        {
            if (!frame.Unsafe.TryGetPointer<NavMeshPathfinder>(entity, out var pathfinder)) return;

            resetAgent = false;
            SetPatrolTarget(frame, pathfinder);
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            switch (filter.Monster->State)
            {
                case MonsterState.Patrol:
                    break;
            }
        }

        private void SetPatrolTarget(Frame frame, NavMeshPathfinder* pathfinder)
        {
            var customData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);

            var randomPatrolPoint = customData.MonsterPatrolPoints[frame.RNG->Next(0, customData.MonsterPatrolPoints.Length)];
            pathfinder->SetTarget(frame, randomPatrolPoint.Position, frame.Map.NavMeshes["Navmesh"]);
        }
    }
}
