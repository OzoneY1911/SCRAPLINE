namespace Quantum
{
    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved, ISignalOnMapChanged
    {
        public override void OnInit(Frame frame)
        {
            base.OnInit(frame);

            frame.Global->ActivePlayers = frame.AllocateDictionary<PlayerRef, EntityRef>(frame.MaxPlayerCount);
            frame.Global->AlivePlayers = frame.AllocateList<EntityRef>(frame.MaxPlayerCount);
        }

        public void OnPlayerAdded(Frame frame, PlayerRef playerRef, bool firstTime)
        {
            var playerEntity = SpawnPlayer(frame, playerRef);
            frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers).Add(playerRef, playerEntity);
            frame.ResolveList<EntityRef>(frame.Global->AlivePlayers).Add(playerEntity);
        }

        public void OnPlayerRemoved(Frame frame, PlayerRef playerRef)
        {
            var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

            frame.ResolveList<EntityRef>(frame.Global->AlivePlayers).Remove(activePlayers[playerRef]);

            activePlayers.Remove(playerRef);
        }

        public void OnMapChanged(Frame frame, AssetRef<Map> previousMap)
        {
            var alivePlayers = frame.ResolveList<EntityRef>(frame.Global->AlivePlayers);

            foreach (var playerEntity in alivePlayers)
            {
                var mapCustomData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);
                mapCustomData.SetPlayerToRandomSpawnPoint(frame, playerEntity);
            }
        }

        private EntityRef SpawnPlayer(Frame frame, PlayerRef playerRef)
        {
            var playerData = frame.GetPlayerData(playerRef);
            var entityPrototypeAsset = frame.FindAsset<EntityPrototype>(playerData.PlayerAvatar);

            var playerEntity = frame.Create(entityPrototypeAsset);

            var mapCustomData = frame.FindAsset<MapCustomData>(frame.Map.UserAsset);
            frame.Unsafe.GetPointer<Player>(playerEntity)->PlayerRef = playerRef;

            mapCustomData.SetPlayerToRandomSpawnPoint(frame, playerEntity);

            if (frame.IsPlayerVerifiedOrLocal(playerRef))
            {
                SetLocalLayer(frame, playerRef, playerEntity);
            }
            SetInventory(frame, playerEntity);

            return playerEntity;
        }

        private void SetLocalLayer(Frame frame, PlayerRef playerRef, EntityRef playerEntity)
        {
            var player = frame.Unsafe.GetPointer<Player>(playerEntity);
            var collider = frame.Unsafe.GetPointer<PhysicsCollider3D>(playerEntity);

            collider->Layer = 11;
        }

        private void SetInventory(Frame frame, EntityRef playerEntity)
        {
            var playerInventory = frame.Unsafe.GetPointer<PlayerInventory>(playerEntity);

            playerInventory->SelectedSlotIndex = SlotIndex.None;
        }
    }
}
