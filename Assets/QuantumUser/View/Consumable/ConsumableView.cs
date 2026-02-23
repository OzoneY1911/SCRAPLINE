using UnityEngine;

namespace Quantum
{
    public unsafe class ConsumableView : QuantumEntityViewComponent
    {
        [SerializeField] private Renderer _renderer;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventOnConsumableUsed>(this, OnEventConsumableUsed);

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

            _renderer.material.SetColor("_EmissiveColor", _renderer.material.color * 0f);
        }
    }
}
