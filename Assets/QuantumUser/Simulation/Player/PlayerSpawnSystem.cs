using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame frame, PlayerRef playerRef, bool firstTime)
        {
            SpawnPlayer(frame, playerRef);
        }

        private void SpawnPlayer(Frame frame, PlayerRef playerRef)
        {
            var data = frame.GetPlayerData(playerRef);

            var entityPrototypeAsset = frame.FindAsset<EntityPrototype>(data.PlayerAvatar);

            var playerEntity = frame.Create(entityPrototypeAsset);

            frame.Unsafe.GetPointer<Player>(playerEntity)->PlayerRef = playerRef;
        }
    }
}
