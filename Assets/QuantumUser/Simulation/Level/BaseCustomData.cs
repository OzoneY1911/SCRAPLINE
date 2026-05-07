namespace Quantum
{
    public abstract class BaseCustomData : AssetObject
    {
        public MapPointData[] PlayerSpawnPoints;
        public MapPointData[] MonsterSpawnPoints;
        public MapPointData[] MonsterPatrolPoints;
    }
}