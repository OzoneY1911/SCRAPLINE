using UnityEngine;

namespace Quantum
{
    public class FlashlightView : QuantumEntityViewComponent
    {
        [SerializeField] private GameObject _lightObject;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventFlashlightToggled>(this, OnEventFlashlightToggled);
        }

        private void OnEventFlashlightToggled(EventFlashlightToggled e)
        {
            if (EntityRef != e.Entity) return;

            _lightObject.SetActive(e.IsOn);
        }
    }
}
