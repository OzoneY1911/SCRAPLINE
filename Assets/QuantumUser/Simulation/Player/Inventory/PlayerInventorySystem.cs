namespace Quantum
{
    public unsafe class PlayerInventorySystem : SystemMainThreadFilter<PlayerInventorySystem.Filter>, ISignalOnValuableCollectAttempted, ISignalOnShopValuableDestroyed
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

            if (input->DropBackDevice.WasPressed && !PlayerInventoryUtils.IsBackDeviceSlotEmpty(filter.PlayerInventory))
            {
                DropBackDevice(frame, filter.Entity);
            }
        }

        public void OnValuableCollectAttempted(Frame frame, EntityRef playerEntity, EntityRef valuableEntity, ValuableConfig valuableConfig)
        {
            var playerInventory = frame.Unsafe.GetPointer<PlayerInventory>(playerEntity);

            if (valuableConfig.IsBackDevice)
            {
                if (!PlayerInventoryUtils.IsBackDeviceSlotEmpty(playerInventory)) return;
                playerInventory->BackDeviceSlot = valuableEntity;
                frame.Events.BackDeviceCollected(playerEntity, valuableEntity);
            }
            else if (valuableConfig.IsPocketValuable)
            {
                if (PlayerInventoryUtils.IsInventoryFull(playerInventory)) return;

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

                playerInventory->Slots[(int)targetSlotIndex] = valuableEntity;
                frame.Events.ValuableCollected(playerEntity, valuableEntity, targetSlotIndex);
            }

            EntityUtils.DisableEntityPhysics(frame, valuableEntity);

            frame.Signals.OnValuableCollected(playerEntity, valuableEntity);

            var valuable = frame.Unsafe.GetPointer<Valuable>(valuableEntity);
            if (valuable->TrackedZoneEntity != EntityRef.None)
            {
                frame.Signals.OnInZoneValuableCollectedByPlayer(valuableEntity);
            }
        }

        public void OnShopValuableDestroyed(Frame frame, EntityRef valuableEntity)
        {
            var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

            foreach (var activePlayer in activePlayers)
            {
                if (!frame.Unsafe.TryGetPointer<PlayerInventory>(activePlayer.Value, out var playerInventory)) return;

                for (int i = 0; i < playerInventory->Slots.Length; i++)
                {
                    if (playerInventory->Slots[i] == valuableEntity)
                    {
                        playerInventory->Slots[i] = EntityRef.None;
                        frame.Events.ValuableDropped(activePlayer.Value, valuableEntity, (SlotIndex)i);
                    }
                }
            }
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

            var playerKCC = frame.Unsafe.GetPointer<KCC>(playerEntity);

            var selectedSlotIndex = playerInventory->SelectedSlotIndex;
            var selectedValuableEntity = playerInventory->Slots[(int)selectedSlotIndex];
            var valuableTransform = frame.Unsafe.GetPointer<Transform3D>(selectedValuableEntity);

            var dropDistance = player->InteractionDistance;
            var hit = PlayerPhysicsUtils.PlayerInteractionHitscan(frame, player);
            if (hit.HasValue)
            {
                dropDistance = hit.Value.CastDistanceNormalized * player->InteractionDistance;
            }
            var input = frame.GetPlayerInput(player->PlayerRef);
            var dropPosition = input->CameraPosition + input->CameraForward * dropDistance;

            valuableTransform->Teleport(frame, dropPosition);

            EntityUtils.RestoryEntityPhysicsAndVelocity(frame, selectedValuableEntity, playerKCC);

            playerInventory->Slots[(int)selectedSlotIndex] = EntityRef.None;
            frame.Events.ValuableDropped(playerEntity, selectedValuableEntity, selectedSlotIndex);
            SelectSlot(frame, playerEntity, SlotIndex.None);
        }

        public void DropBackDevice(Frame frame, EntityRef playerEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Player>(playerEntity, out var player)) return;
            if (!frame.Unsafe.TryGetPointer<PlayerInventory>(playerEntity, out var playerInventory)) return;

            var playerKCC = frame.Unsafe.GetPointer<KCC>(playerEntity);

            var backDeviceEntity = playerInventory->BackDeviceSlot;
            var backDeviceTransform = frame.Unsafe.GetPointer<Transform3D>(backDeviceEntity);

            var dropDistance = player->InteractionDistance;
            var hit = PlayerPhysicsUtils.PlayerInteractionHitscan(frame, player);
            if (hit.HasValue)
            {
                dropDistance = hit.Value.CastDistanceNormalized * player->InteractionDistance;
            }
            var input = frame.GetPlayerInput(player->PlayerRef);
            var dropPosition = input->CameraPosition + input->CameraForward * dropDistance;

            backDeviceTransform->Teleport(frame, dropPosition);

            EntityUtils.RestoryEntityPhysicsAndVelocity(frame, backDeviceEntity, playerKCC);

            playerInventory->BackDeviceSlot = EntityRef.None;
            frame.Events.BackDeviceDropped(playerEntity, backDeviceEntity);
        }
    }
}