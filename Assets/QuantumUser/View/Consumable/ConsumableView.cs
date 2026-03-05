using System;
using UnityEngine;

namespace Quantum
{
    public unsafe class ConsumableView : QuantumEntityViewComponent
    {
        private Animator _animator;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventOnConsumableUsed>(this, OnEventConsumableUsed);

            _animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            if (VerifiedFrame == null) return;
            if (!VerifiedFrame.Unsafe.TryGetPointer<Consumable>(EntityRef, out var consumable)) return;

            if (consumable->IsUsed)
            {
                SetUsedVisuals(EntityRef);
            }
        }

        private void OnEventConsumableUsed(EventOnConsumableUsed e)
        {
            SetUsedVisuals(e.ConsumableEntity);
        }

        private void SetUsedVisuals(EntityRef consumableEntity)
        {
            if (EntityRef != consumableEntity) return;

            _animator.SetBool("IsUsed", true);
        }
    }
}
