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
                filter.Player->EmoteConfig = emoteCommand.EmoteConfig;
                frame.Events.PlayerEmoteChanged(filter.Entity);
            }

            foreach (var colorCommand in frame.GetPlayerCommands<CommandSetPlayerColor>(filter.Player->PlayerRef))
            {
                filter.Player->ColorRGB = colorCommand.ColorRGB;
                frame.Events.PlayerColorChanged(filter.Entity);
            }
        }
    }
}
