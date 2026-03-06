using Photon.Deterministic;

namespace Quantum
{
    public unsafe class MonsterSystem : SystemMainThreadFilter<MonsterSystem.Filter>, ISignalOnNavMeshWaypointReached, ISignalOnComponentAdded<Monster>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Monster* Monster;
            public NavMeshPathfinder* Pathfinder;
        }

        public void OnAdded(Frame frame, EntityRef entity, Monster* monster)
        {
            EnterState(frame, entity, MonsterState.Patrol);
        }

        public void OnNavMeshWaypointReached(Frame frame, EntityRef entity, FPVector3 waypoint, Navigation.WaypointFlag waypointFlags, ref bool resetAgent)
        {
            if (waypointFlags != Navigation.WaypointFlag.Target) return;
            resetAgent = false;
            
            if (!frame.Unsafe.TryGetPointer<NavMeshPathfinder>(entity, out var pathfinder)) return;
            if (!frame.Unsafe.TryGetPointer<Monster>(entity, out var monster)) return;

            switch (monster->State)
            {
                case MonsterState.Patrol:
                    SetPatrolTarget(frame, pathfinder);
                    break;
            }
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            switch (filter.Monster->State)
            {
                case MonsterState.Patrol:
                    if (!frame.Unsafe.TryGetPointer<Transform3D>(filter.Entity, out var transform)) return;

                    var monsterConfig = frame.FindAsset<MonsterConfig>(filter.Monster->Config);

                    if (monsterConfig.CanChase && PhysicsUtils.PlayerIsInRange(frame, transform->Position, out var playerEntity))
                    {
                        filter.Monster->ChaseTarget = playerEntity;
                        EnterState(frame, filter.Entity, MonsterState.Chase);
                    }
                    break;
                case MonsterState.Chase:
                    if ((frame.Number & 3) == 0) SetChaseTarget(frame, filter.Entity);
                    break;
            }
        }

        private void EnterState(Frame frame, EntityRef entity, MonsterState targetState)
        {
            if (!frame.Unsafe.TryGetPointer<NavMeshPathfinder>(entity, out var pathfinder)) return;
            if (!frame.Unsafe.TryGetPointer<Monster>(entity, out var monster)) return;

            monster->State = targetState;

            switch (monster->State)
            {
                case MonsterState.Patrol:
                    SetPatrolTarget(frame, pathfinder);
                    break;
            }
        }

        private void SetPatrolTarget(Frame frame, NavMeshPathfinder* pathfinder)
        {
            var customData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);

            var randomPatrolPoint = customData.MonsterPatrolPoints[frame.RNG->Next(0, customData.MonsterPatrolPoints.Length)];
            pathfinder->SetTarget(frame, randomPatrolPoint.Position, frame.Map.NavMeshes["Navmesh"]);
        }

        private void SetChaseTarget(Frame frame, EntityRef entity)
        {
            if (!frame.Unsafe.TryGetPointer<NavMeshPathfinder>(entity, out var pathfinder)) return;
            if (!frame.Unsafe.TryGetPointer<Monster>(entity, out var monster)) return;
            if (!frame.Unsafe.TryGetPointer<Transform3D>(monster->ChaseTarget, out var targetTransform)) return;
            
            pathfinder->SetTarget(frame, targetTransform->Position, frame.Map.NavMeshes["Navmesh"]);
        }
    }
}
