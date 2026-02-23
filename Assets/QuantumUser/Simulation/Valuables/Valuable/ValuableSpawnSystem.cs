namespace Quantum
{
    public unsafe class ValuableSpawnSystem : SystemSignalsOnly, ISignalOnMapChanged
    {
        public override void OnInit(Frame frame)
        {
            base.OnInit(frame);

            var mapCustomData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);

            if (frame.Map.name == "HubMap") mapCustomData.SpawnValuables(frame, true);
        }

        public void OnMapChanged(Frame frame, AssetRef<Map> previousMap)
        {
            OnInit(frame);
        }
    }
}
