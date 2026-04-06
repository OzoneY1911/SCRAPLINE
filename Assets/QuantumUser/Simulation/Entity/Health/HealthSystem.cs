using Photon.Deterministic;

namespace Quantum
{
    public unsafe class HealthSystem : SystemSignalsOnly, ISignalOnComponentAdded<Health>, ISignalOnHealthChanged, ISignalOnEntityDeath
    {
        public void OnAdded(Frame frame, EntityRef entity, Health* health)
        {
            health->Current = health->Max;
        }

        public void OnHealthChanged(Frame frame, EntityRef entity, FP changeDelta)
        {
            if (!frame.Unsafe.TryGetPointer<Health>(entity, out var health)) return;

            health->Current += changeDelta;

            if (health->Current <= 0)
            {
                frame.Signals.OnEntityDeath(entity);
                frame.Events.EntityDeath(entity);
            }

            if (health->Current > health->Max) health->Current = health->Max;
        }

        public void OnEntityDeath(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer<Player>(entity, out var player))
            {
                var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

                frame.ResolveList<EntityRef>(frame.Global->AlivePlayers).Remove(activePlayers[player->PlayerRef]);
            }

            frame.Destroy(entity);
        }
    }
}
