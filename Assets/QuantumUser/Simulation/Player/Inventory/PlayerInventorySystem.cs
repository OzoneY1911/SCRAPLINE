using Photon.Deterministic;

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

        private bool IsSlotSelected(PlayerInventory* playerInventory)
        {
            return playerInventory->SelectedSlotIndex != SlotIndex.None;
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

            if (input->DropValuable.WasPressed && IsSlotSelected(filter.PlayerInventory) && !IsSelectedSlotEmpty(filter.PlayerInventory))
            {
                var playerInventory = filter.PlayerInventory;
                var selectedSlotIndex = playerInventory->SelectedSlotIndex;

                var selectedValuableEntity = playerInventory->Slots[(int)selectedSlotIndex];
                var valuableBody = frame.Unsafe.GetPointer<PhysicsBody3D>(selectedValuableEntity);
                var valuableCollider = frame.Unsafe.GetPointer<PhysicsCollider3D>(selectedValuableEntity);

                var playerBody = frame.Unsafe.GetPointer<PhysicsBody3D>(filter.Entity);
                var valuableTransform = frame.Unsafe.GetPointer<Transform3D>(selectedValuableEntity);

                var dropPosition = input->CameraPosition + input->CameraForward * player->InteractionDistance;

                valuableTransform->Teleport(frame, dropPosition);

                valuableBody->Velocity = playerBody->Velocity;
                valuableBody->AngularVelocity = playerBody->AngularVelocity;
                valuableBody->Enabled = true;
                valuableCollider->Enabled = true;

                playerInventory->Slots[(int)selectedSlotIndex] = EntityRef.None;
                frame.Events.ValuableDropped(filter.Entity, selectedValuableEntity, selectedSlotIndex);
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

                if (IsSlotSelected(playerInventory) && IsSelectedSlotEmpty(playerInventory))
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

                var valuableBody = frame.Unsafe.GetPointer<PhysicsBody3D>(valuableEntity);
                var valuableCollider = frame.Unsafe.GetPointer<PhysicsCollider3D>(valuableEntity);
                valuableBody->Enabled = false;
                valuableCollider->Enabled = false;

                playerInventory->Slots[(int)targetSlotIndex] = valuableEntity;
                frame.Events.ValuableCollected(playerEntity, valuableEntity, targetSlotIndex);
                frame.Signals.OnValuableCollected(playerEntity);

                var valuable = frame.Unsafe.GetPointer<Valuable>(valuableEntity);
                if (valuable->TrackedZoneEntity != EntityRef.None)
                {
                    frame.Signals.OnInZoneValuableCollectedByPlayer(valuableEntity);
                }
            }
        }
    }
}
