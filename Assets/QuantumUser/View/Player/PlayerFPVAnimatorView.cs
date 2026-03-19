using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerFPVAnimatorView : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;

        public override void OnUpdateView()
        {
            if (!VerifiedFrame.Unsafe.TryGetPointer(EntityRef, out PlayerInventory* playerInventory)) return;

            bool isHolding =
                PlayerInventoryUtils.IsSlotSelected(playerInventory) &&
                !PlayerInventoryUtils.IsSelectedSlotEmpty(playerInventory);

            _animator.SetBool("IsHolding", isHolding);
        }

        public override void OnLateUpdateView()
        {
            if (VerifiedFrame.TryGet(EntityRef, out Player player) == false) return;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            Frame frame = isLocal ? PredictedFrame : VerifiedFrame;

            if (frame.Exists(EntityRef) == false) return;

            var dragging = frame.Get<PlayerDragging>(EntityRef);

            _animator.SetBool("IsDragging", dragging.IsDragging);
        }

        private void OnEventInventorySlotSelected(EventInventorySlotSelected e)
        {
            if (EntityRef != e.PlayerEntity) return;

            if (!VerifiedFrame.Unsafe.TryGetPointer(EntityRef, out PlayerInventory* playerInventory)) return;

            _animator.SetBool(
                "IsHolding", 
                PlayerInventoryUtils.IsSlotSelected(playerInventory) 
                && !PlayerInventoryUtils.IsSelectedSlotEmpty(playerInventory));
        }

        private void OnEventValuableCollected(EventValuableCollected e)
        {
            if (EntityRef != e.PlayerEntity) return;

            if (!VerifiedFrame.Unsafe.TryGetPointer(EntityRef, out PlayerInventory* playerInventory)) return;

            if (playerInventory->SelectedSlotIndex != e.SlotIndex) return;

            _animator.SetBool("IsHolding", true);
        }

        private void OnEventValuableDropped(EventValuableDropped e)
        {
            if (EntityRef != e.PlayerEntity) return;

            if (!VerifiedFrame.Unsafe.TryGetPointer(EntityRef, out PlayerInventory* playerInventory)) return;

            _animator.SetBool("IsHolding", false);
        }
    }
}
