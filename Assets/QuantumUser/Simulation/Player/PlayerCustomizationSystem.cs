namespace Quantum
{
    public unsafe class PlayerCustomizationSystem : SystemMainThreadFilter<PlayerCustomizationSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (frame.TryGetPlayerCommand<CommandSetEmote>(filter.Player->PlayerRef, out var emoteCommand))
            {
                frame.Events.PlayerEmoteChanged(filter.Entity, emoteCommand.EmoteConfig);
            }

            if (frame.TryGetPlayerCommand<CommandSetPlayerColor>(filter.Player->PlayerRef, out var colorCommand))
            {
                frame.Events.PlayerColorChanged(filter.Entity, colorCommand.ColorRGB);
            }
        }
    }
}
