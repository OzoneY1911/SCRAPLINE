using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
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

        private EntityRef SpawnPlayer(Frame frame, PlayerRef playerRef)
        {
            var data = frame.GetPlayerData(playerRef);

            var entityPrototypeAsset = frame.FindAsset<EntityPrototype>(data.PlayerAvatar);

            var playerEntity = frame.Create(entityPrototypeAsset);

            frame.Unsafe.GetPointer<Player>(playerEntity)->PlayerRef = playerRef;

            if (frame.IsPlayerVerifiedOrLocal(playerRef))
            {
                SetLocalLayer(frame, playerRef, playerEntity);
            }

            return playerEntity;
        }

        private void SetLocalLayer(Frame frame, PlayerRef playerRef, EntityRef playerEntity)
        {
            var player = frame.Unsafe.GetPointer<Player>(playerEntity);
            var collider = frame.Unsafe.GetPointer<PhysicsCollider3D>(playerEntity);

            collider->Layer = player->LocalMask;
        }
    }
}
