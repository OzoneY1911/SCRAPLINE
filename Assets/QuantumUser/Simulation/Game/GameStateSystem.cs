namespace Quantum
{
    public unsafe class GameStateSystem : SystemSignalsOnly, ISignalOnMapChanged
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

            frame.Global->DayCount = 1;
            frame.Global->MaxQuotaZoneCount = 6;

            frame.Global->PlayerMoney = 700;
            frame.Events.PlayerMoneyUpdated();
        }

        public void OnMapChanged(Frame frame, AssetRef<Map> previousMap)
        {
            if (frame.FindAsset<Map>(previousMap).name == "HubMap")
            {
                frame.Events.MapChangedToProcedural();
            }
            else
            {
                if (frame.Global->DayCount <= frame.Global->MaxQuotaZoneCount)
                {
                    frame.Global->DayCount += 1;
                }
                frame.Events.MapChangedToHub();
            }
        }
    }
}