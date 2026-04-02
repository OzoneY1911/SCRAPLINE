using Photon.Deterministic;
using System.Threading;

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
            var config = frame.FindAsset<MonsterConfig>(filter.Monster->Config);

            switch (filter.Monster->State)
            {
                case MonsterState.Patrol:
                    UpdatePatrol(frame, ref filter, config);
                    break;
                case MonsterState.Chase:
                    UpdateChase(frame, ref filter, config);
                    break;
                case MonsterState.Attack:
                    UpdateAttack(frame, ref filter, config);
                    break;
            }
        }

        private void UpdatePatrol(Frame frame, ref Filter filter, MonsterConfig config)
        {
            if (!frame.Unsafe.TryGetPointer<Transform3D>(filter.Entity, out var transform)) return;

            if (config.CanChase && PhysicsUtils.PlayerIsInRange(frame, transform->Position, out var playerEntity, config.ChaseStartDistance))
            {
                filter.Monster->ChaseTarget = playerEntity;
                EnterState(frame, filter.Entity, MonsterState.Chase);
            }
        }

        private void UpdateChase(Frame frame, ref Filter filter, MonsterConfig config)
        {
            if (!frame.Unsafe.TryGetPointer<Transform3D>(filter.Entity, out var monsterTransform)) return;
            if (!frame.Unsafe.TryGetPointer<Transform3D>(filter.Monster->ChaseTarget, out var targetTransform)) return;

            FP distance = FPVector3.Distance(monsterTransform->Position, targetTransform->Position);

            if (distance <= config.AttackDistance)
            {
                if (config.CanAttack)
                {
                    EnterState(frame, filter.Entity, MonsterState.Attack);
                }
                else
                {

                }
                return;
            }
            else if (distance >= config.ChaseStopDistance)
            {
                EnterState(frame, filter.Entity, MonsterState.Patrol);
            }

            if ((frame.Number & 3) == 0) SetChaseTarget(frame, filter.Entity);
        }

        private void UpdateAttack(Frame frame, ref Filter filter, MonsterConfig config)
        {
            var monster = filter.Monster;

            if (!frame.Unsafe.TryGetPointer<Transform3D>(filter.Entity, out var transform)) return;
            if (!frame.Unsafe.TryGetPointer<Transform3D>(monster->ChaseTarget, out var targetTransform)) return;

            FP distance = FPVector3.Distance(transform->Position, targetTransform->Position);

            // If player moved away → chase again
            if (distance > config.AttackDistance)
            {
                EnterState(frame, filter.Entity, MonsterState.Chase);
                return;
            }

            // Rotate towards player (important part)
            FPVector3 direction = targetTransform->Position - transform->Position;
            direction.Y = 0;

            if (direction.SqrMagnitude > FP._0)
            {
                transform->Rotation = FPQuaternion.LookRotation(direction);
            }
        }

        private void EnterState(Frame frame, EntityRef entity, MonsterState targetState)
        {
            if (!frame.Unsafe.TryGetPointer<NavMeshPathfinder>(entity, out var pathfinder)) return;
            if (!frame.Unsafe.TryGetPointer<Monster>(entity, out var monster)) return;

            monster->State = targetState;

            switch (targetState)
            {
                case MonsterState.Patrol:
                    SetPatrolTarget(frame, pathfinder);
                    break;
                case MonsterState.Chase:
                    SetChaseTarget(frame, entity);
                    break;
                case MonsterState.Attack:
                    pathfinder->Stop(frame, entity);
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
