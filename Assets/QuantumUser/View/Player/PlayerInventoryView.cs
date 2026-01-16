namespace Quantum
{
    public unsafe class PlayerInventoryView : QuantumEntityViewComponent
    {
        private PlayerInventoryUI _playerInventoryUI;

        public override void OnActivate(Frame frame)
        {
            if (!frame.Unsafe.TryGetPointer<Player>(EntityRef, out Player* player)) return;
            if (!Game.PlayerIsLocal(player->PlayerRef)) return;

            QuantumEvent.Subscribe<EventInventorySlotSelected>(this, OnEventInventorySlotSelected);
            QuantumEvent.Subscribe<EventValuableCollected>(this, OnEventValuableCollected);
        }

        private void OnEventInventorySlotSelected(EventInventorySlotSelected e)
        {
            if (e.Entity != EntityRef) return;

            if (_playerInventoryUI == null)
            {
                _playerInventoryUI = FindAnyObjectByType<PlayerInventoryUI>();
            }

            _playerInventoryUI.SetSelectedSlot((byte)e.SlotIndex);
        }

        private void OnEventValuableCollected(EventValuableCollected e)
        {
            if (e.PlayerEntity != EntityRef) return;

            if (_playerInventoryUI == null)
            {
                _playerInventoryUI = FindAnyObjectByType<PlayerInventoryUI>();
            }

            Frame frame = VerifiedFrame;

            if (!frame.Unsafe.TryGetPointer<Valuable>(e.ValuableEntity, out var valuable)) return;

            var valuableConfig = frame.FindAsset<ValuableConfig>(valuable->Config);

            _playerInventoryUI.SetSlotValuableName((byte)e.SlotIndex, valuableConfig.DisplayName);
        }
    }
}
