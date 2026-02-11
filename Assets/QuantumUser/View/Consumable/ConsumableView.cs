using UnityEngine;

namespace Quantum
{
    public class ConsumableView : QuantumEntityViewComponent
    {
        [SerializeField] private Renderer _renderer;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventOnConsumableUsed>(this, OnEventConsumableUsed);
        }

        private void OnEventConsumableUsed(EventOnConsumableUsed e)
        {
            if (EntityRef != e.ConsumableEntity) return;

            _renderer.material.SetColor("_EmissiveColor", _renderer.material.color * 0f);
        }
    }
}
