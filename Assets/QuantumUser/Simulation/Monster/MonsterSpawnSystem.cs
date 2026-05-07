namespace Quantum
{
    public unsafe class MonsterSpawnSystem : SystemSignalsOnly, ISignalOnMapChanged
    {
        public override void OnInit(Frame frame)
        {
            base.OnInit(frame);

            var customData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);

            if (customData.MonsterPrototypes.Length == 0) return;
            var monsterIndex = frame.RNG->Next(0, customData.MonsterPrototypes.Length); 
            var monsterEntity = frame.Create(customData.MonsterPrototypes[monsterIndex]);

            MapCustomDataUtils.SetMonsterToRandomSpawnPoint(frame, monsterEntity);
        }

        public void OnMapChanged(Frame frame, AssetRef<Map> previousMap)
        {
            var entitiesToDestroy = frame.AllocateList<EntityRef>();

            foreach (var (entity, monster) in frame.Unsafe.GetComponentBlockIterator<Monster>())
            {
                entitiesToDestroy.Add(entity);
            }

            foreach (var entity in entitiesToDestroy)
            {
                frame.Destroy(entity);
            }

            var customData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);

            if (customData.MonsterPrototypes.Length == 0) return;
            var monsterIndex = frame.RNG->Next(0, customData.MonsterPrototypes.Length);
            var monsterEntity = frame.Create(customData.MonsterPrototypes[monsterIndex]);

            MapCustomDataUtils.SetMonsterToRandomSpawnPoint(frame, monsterEntity);
        }
    }
}
