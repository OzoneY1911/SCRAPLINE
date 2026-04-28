namespace Quantum
{
    public unsafe class PlayerEmoteSystem : SystemMainThreadFilter<PlayerEmoteSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (frame.TryGetPlayerCommand<CommandSetEmote>(filter.Player->PlayerRef, out var command))
            {
                frame.Events.PlayerEmoteChanged(filter.Entity, command.EmoteConfig);
            }
        }
    }
}
