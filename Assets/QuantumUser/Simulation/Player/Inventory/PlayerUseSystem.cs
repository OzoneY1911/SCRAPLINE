namespace Quantum
{
    public unsafe class PlayerUseSystem : SystemMainThreadFilter<PlayerUseSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public PlayerInventory* PlayerInventory;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

            if (input->Use.WasPressed && PlayerInventoryUtils.IsSlotSelected(filter.PlayerInventory) && !PlayerInventoryUtils.IsSelectedSlotEmpty(filter.PlayerInventory))
            {
                var valuableEntity = filter.PlayerInventory->Slots[(int)filter.PlayerInventory->SelectedSlotIndex];

                frame.Signals.OnValuableUseRequested(filter.Entity, valuableEntity);
            }
        }
    }
}
