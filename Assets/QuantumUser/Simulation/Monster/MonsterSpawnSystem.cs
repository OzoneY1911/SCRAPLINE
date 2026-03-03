namespace Quantum
{
    public unsafe class MonsterSpawnSystem : SystemSignalsOnly
    {
        public override void OnInit(Frame frame)
        {
            base.OnInit(frame);

            var customData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);

            var monsterIndex = frame.RNG->Next(0, customData.MonsterPrototypes.Length); 
            var monsterEntity = frame.Create(customData.MonsterPrototypes[monsterIndex]);

            customData.SetMonsterToRandomSpawnPoint(frame, monsterEntity);

            if (!frame.Unsafe.TryGetPointer<Monster>(monsterEntity, out var monster)) return;

            monster->State = MonsterState.Patrol;
        }
    }
}
