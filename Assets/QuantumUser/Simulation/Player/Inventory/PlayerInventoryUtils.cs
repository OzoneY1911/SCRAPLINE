namespace Quantum
{
    public static unsafe class PlayerInventoryUtils
    {
        public static bool IsInventoryFull(PlayerInventory* playerInventory)
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

        public static bool IsSelectedSlotEmpty(PlayerInventory* playerInventory)
        {
            return playerInventory->Slots[(int)playerInventory->SelectedSlotIndex] == EntityRef.None;
        }

        public static bool IsBackDeviceSlotEmpty(PlayerInventory* playerInventory)
        {
            return playerInventory->BackDeviceSlot == EntityRef.None;
        }

        public static bool IsSlotSelected(PlayerInventory* playerInventory)
        {
            return playerInventory->SelectedSlotIndex != SlotIndex.None;
        }

        public static bool TryGetFirstEmptySlotIndex(PlayerInventory* playerInventory, out SlotIndex slotIndex)
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
    }
}
