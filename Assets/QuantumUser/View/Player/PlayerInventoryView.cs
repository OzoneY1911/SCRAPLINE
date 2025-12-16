namespace Quantum
{
    public class PlayerInventoryView : QuantumEntityViewComponent
    {
        private PlayerInventoryUI _playerInventoryUI;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventInventorySlotSelected>(this, OnEventInventorySlotSelected);
        }

        private void OnEventInventorySlotSelected(EventInventorySlotSelected e)
        {
            if (e.Entity != EntityRef) return;

            if (_playerInventoryUI == null)
            {
                _playerInventoryUI = FindAnyObjectByType<PlayerInventoryUI>();
            }

            _playerInventoryUI.SetSelectedSlot(e.SlotIndex);
        }
    }
}
