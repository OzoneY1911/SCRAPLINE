namespace Quantum
{

    public unsafe class PlayerDeathSystem : SystemSignalsOnly, ISignalOnEntityDeath
    {
        public void OnEntityDeath(Frame frame, EntityRef entity)
        {
            if (frame.Has<Player>(entity))
            {
                frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->AlivePlayers).Remove(frame.Unsafe.GetPointer<Player>(entity)->PlayerRef);

                frame.Destroy(entity);
            }
        }
    }
}
