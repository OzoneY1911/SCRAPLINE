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
            foreach (var emoteCommand in frame.GetPlayerCommands<CommandSetEmote>(filter.Player->PlayerRef))
            {
                frame.Events.PlayerEmoteChanged(filter.Entity, emoteCommand.EmoteConfig);
            }

            foreach (var colorCommand in frame.GetPlayerCommands<CommandSetPlayerColor>(filter.Player->PlayerRef))
            {
                frame.Events.PlayerColorChanged(filter.Entity, colorCommand.ColorRGB);
            }
        }
    }
}
