using UnityEngine;

namespace Quantum
{

    public unsafe class PlayerDeathSystem : SystemSignalsOnly, ISignalOnEntityDeath
    {
        public void OnEntityDeath(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer<Player>(entity, out var player))
            {
                var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

                frame.ResolveList<EntityRef>(frame.Global->AlivePlayers).Remove(activePlayers[player->PlayerRef]);

                frame.Destroy(entity);
            }
        }
    }
}
