namespace Quantum
{
    public unsafe class PlayerInventorySystem : SystemMainThreadFilter<PlayerInventorySystem.Filter>, ISignalOnValuableCollectAttempted, ISignalOnValuableDropRequested
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
                if (input->SelectedInventorySlotIndex == filter.PlayerInventory->SelectedSlotIndex)
                {
                    SelectSlot(frame, filter.Entity, SlotIndex.None);
                }
                else
                {
                    SelectSlot(frame, filter.Entity, input->SelectedInventorySlotIndex);
                }
            }

            if (input->DropValuable.WasPressed && PlayerInventoryUtils.IsSlotSelected(filter.PlayerInventory) && !PlayerInventoryUtils.IsSelectedSlotEmpty(filter.PlayerInventory))
            {
                DropValuable(frame, filter.Entity);
            }
        }

        public void OnValuableCollectAttempted(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            var playerInventory = frame.Unsafe.GetPointer<PlayerInventory>(playerEntity);

            if (!PlayerInventoryUtils.IsInventoryFull(playerInventory))
            {
                SlotIndex targetSlotIndex = SlotIndex.None;

                if (PlayerInventoryUtils.IsSlotSelected(playerInventory) && PlayerInventoryUtils.IsSelectedSlotEmpty(playerInventory))
                {
                    targetSlotIndex = playerInventory->SelectedSlotIndex;
                }
                else
                {
                    if (PlayerInventoryUtils.TryGetFirstEmptySlotIndex(playerInventory, out SlotIndex slotIndex))
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
                frame.Signals.OnValuableCollected(playerEntity, valuableEntity);

                var valuable = frame.Unsafe.GetPointer<Valuable>(valuableEntity);
                if (valuable->TrackedZoneEntity != EntityRef.None)
                {
                    frame.Signals.OnInZoneValuableCollectedByPlayer(valuableEntity);
                }
            }
        }

        public void OnValuableDropRequested(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            DropValuable(frame, playerEntity);
        }

        private void SelectSlot(Frame frame, EntityRef playerEntity, SlotIndex selectedSlotIndex)
        {
            if (!frame.Unsafe.TryGetPointer<PlayerInventory>(playerEntity, out var playerInventory)) return;

            if (playerInventory->SelectedSlotIndex != SlotIndex.None)
            {
                frame.Signals.OnInventorySlotDeselected(playerInventory->Slots[(int)playerInventory->SelectedSlotIndex]);
            }

            playerInventory->SelectedSlotIndex = selectedSlotIndex;
            frame.Events.InventorySlotSelected(playerEntity, selectedSlotIndex);
        }

        private void DropValuable(Frame frame, EntityRef playerEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Player>(playerEntity, out var player)) return;
            if (!frame.Unsafe.TryGetPointer<PlayerInventory>(playerEntity, out var playerInventory)) return;

            var input = frame.GetPlayerInput(player->PlayerRef);

            var selectedSlotIndex = playerInventory->SelectedSlotIndex;

            var selectedValuableEntity = playerInventory->Slots[(int)selectedSlotIndex];
            var valuableBody = frame.Unsafe.GetPointer<PhysicsBody3D>(selectedValuableEntity);
            var valuableCollider = frame.Unsafe.GetPointer<PhysicsCollider3D>(selectedValuableEntity);

            var playerBody = frame.Unsafe.GetPointer<PhysicsBody3D>(playerEntity);
            var valuableTransform = frame.Unsafe.GetPointer<Transform3D>(selectedValuableEntity);

            var dropPosition = input->CameraPosition + input->CameraForward * player->InteractionDistance;

            valuableTransform->Teleport(frame, dropPosition);

            valuableBody->Velocity = playerBody->Velocity;
            valuableBody->AngularVelocity = playerBody->AngularVelocity;
            valuableBody->Enabled = true;
            valuableCollider->Enabled = true;

            playerInventory->Slots[(int)selectedSlotIndex] = EntityRef.None;
            frame.Events.ValuableDropped(playerEntity, selectedValuableEntity, selectedSlotIndex);
            SelectSlot(frame, playerEntity, SlotIndex.None);
        }
    }
}
