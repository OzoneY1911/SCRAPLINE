using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame frame, PlayerRef playerRef, bool firstTime)
        {
            var playerEntity = SpawnPlayer(frame, playerRef);

            if (frame.IsPlayerVerifiedOrLocal(playerRef))
            {
                SetLocalLayer(frame, playerRef, playerEntity);
            }
        }

        private EntityRef SpawnPlayer(Frame frame, PlayerRef playerRef)
        {
            var data = frame.GetPlayerData(playerRef);

            var entityPrototypeAsset = frame.FindAsset<EntityPrototype>(data.PlayerAvatar);

            var playerEntity = frame.Create(entityPrototypeAsset);

            frame.Unsafe.GetPointer<Player>(playerEntity)->PlayerRef = playerRef;

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
