namespace Quantum
{
    public unsafe class PlayerInventorySystem : SystemMainThreadFilter<PlayerInventorySystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public PlayerInventory* PlayerInventory;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(player->PlayerRef);

            if (input->SelectInventorySlot.WasPressed)
            {
                SelectSlot(frame, ref filter, input->SelectedInventorySlotIndex);
            }
        }

        private void SelectSlot(Frame frame, ref Filter filter, byte selectedSlotIndex)
        {
            filter.PlayerInventory->SelectedSlotIndex =
                    selectedSlotIndex;
            frame.Events.InventorySlotSelected(filter.Entity, selectedSlotIndex);
        }
    }
}
