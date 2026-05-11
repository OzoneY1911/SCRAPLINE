namespace Quantum
{
    public unsafe class GameStateSystem : SystemSignalsOnly
    {
        public override void OnInit(Frame frame)
        {
            frame.Global->RuntimeCustomData = new RuntimeMapCustomData()
            {
                PlayerSpawnPoints = frame.AllocateList<MapPointData>(),
                MonsterSpawnPoints = frame.AllocateList<MapPointData>(),
                MonsterPatrolPoints = frame.AllocateList<MapPointData>(),
                ValuableSpawnPoints = frame.AllocateList<MapPointData>(),
                ProceduralRoomEntities = frame.AllocateList<EntityRef>(),
                ProceduralValuableEntities = frame.AllocateList<EntityRef>(),
            };
        }
    }
}