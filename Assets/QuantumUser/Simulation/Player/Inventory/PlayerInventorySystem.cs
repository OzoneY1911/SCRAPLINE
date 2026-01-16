namespace Quantum
{
    public unsafe class PlayerInventorySystem : SystemMainThreadFilter<PlayerInventorySystem.Filter>, ISignalOnValuableCollectAttempted
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public PlayerInventory* PlayerInventory;
        }

        private bool IsInventoryFull(PlayerInventory* playerInventory)
        {
            for (var i = 0; i < playerInventory->Slots.Length; i++)
            {
                if (playerInventory->Slots[i] == EntityRef.None)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsSelectedSlotEmpty(PlayerInventory* playerInventory)
        {
            return playerInventory->Slots[(int)playerInventory->SelectedSlotIndex] == EntityRef.None;
        }

        private bool TryGetFirstEmptySlotIndex(PlayerInventory* playerInventory, out SlotIndex slotIndex)
        {
            for (var i = 0; i < playerInventory->Slots.Length; i++)
            {
                if (playerInventory->Slots[i] == EntityRef.None)
                {
                    slotIndex = (SlotIndex)i;
                    return true;
                }
            }
            slotIndex = 0;
            return false;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(player->PlayerRef);

            if (input->SelectInventorySlot.WasPressed)
            {
                if (input->SelectedInventorySlotIndex == filter.PlayerInventory->SelectedSlotIndex)
                {
                    SelectSlot(frame, ref filter, SlotIndex.None);
                }
                else
                {
                    SelectSlot(frame, ref filter, input->SelectedInventorySlotIndex);
                }
            }
        }

        private void SelectSlot(Frame frame, ref Filter filter, SlotIndex selectedSlotIndex)
        {
            filter.PlayerInventory->SelectedSlotIndex = selectedSlotIndex;
            frame.Events.InventorySlotSelected(filter.Entity, selectedSlotIndex);
        }

        public void OnValuableCollectAttempted(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            var playerInventory = frame.Unsafe.GetPointer<PlayerInventory>(playerEntity);

            if (!IsInventoryFull(playerInventory))
            {
                SlotIndex targetSlotIndex = SlotIndex.None;

                if (playerInventory->SelectedSlotIndex != SlotIndex.None && IsSelectedSlotEmpty(playerInventory))
                {
                    targetSlotIndex = playerInventory->SelectedSlotIndex;
                }
                else
                {
                    if (TryGetFirstEmptySlotIndex(playerInventory, out SlotIndex slotIndex))
                    {
                        targetSlotIndex = slotIndex;
                    }
                }
                playerInventory->Slots[(int)targetSlotIndex] = valuableEntity;
                frame.Events.ValuableCollected(playerEntity, valuableEntity, targetSlotIndex);
            }
        }
    }
}
