namespace Quantum
{
    public unsafe class ValuableSpawnSystem : SystemSignalsOnly, ISignalOnMapChanged
    {
        public override void OnInit(Frame frame)
        {
            var mapCustomData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);

            mapCustomData.SpawnValuables(frame, true);
        }

        public void OnMapChanged(Frame frame, AssetRef<Map> previousMap)
        {
            if (frame.FindAsset<Map>(previousMap).name == "HubMap")
            {
                var entitiesToDestroy = frame.AllocateList<EntityRef>();

                foreach (var (entity, valuable) in frame.Unsafe.GetComponentBlockIterator<Valuable>())
                {
                    if (valuable->IsShopValuable) entitiesToDestroy.Add(entity);
                }

                foreach (var entity in entitiesToDestroy)
                {
                    frame.Signals.OnShopValuableDestroyed(entity);
                    frame.Destroy(entity);
                }
            }

            OnInit(frame);
        }
    }
}
