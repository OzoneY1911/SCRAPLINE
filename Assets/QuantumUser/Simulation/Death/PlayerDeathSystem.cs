namespace Quantum
{

    public unsafe class PlayerDeathSystem : SystemSignalsOnly, ISignalOnEntityDeath
    {
        public void OnEntityDeath(Frame frame, EntityRef entity)
        {
            if (frame.Has<Player>(entity))
            {
                frame.Destroy(entity);
            }
        }
    }
}
