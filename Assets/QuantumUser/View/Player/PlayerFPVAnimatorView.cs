using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerFPVAnimatorView : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventInventorySlotSelected>(this, OnEventInventorySlotSelected);
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

            _animator.SetBool("IsHolding", playerInventory->Slots[(int)e.SlotIndex] != EntityRef.None);
        }
    }
}
