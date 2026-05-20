using TMPro;
using UnityEngine;

namespace Quantum
{
    public unsafe class ValuableView : QuantumEntityViewComponent
    {
        [SerializeField] private GameObject _damagePrefab;
        [SerializeField] private bool _isBackDevice;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventValuableHit>(this, OnEventValuableHit);

            if (!_isBackDevice)
            {
                QuantumEvent.Subscribe<EventValuableCollected>(this, OnEventValuableCollected);
                QuantumEvent.Subscribe<EventValuableDropped>(this, OnEventValuableDropped);
            }
            else
            {
                QuantumEvent.Subscribe<EventBackDeviceCollected>(this, OnEventBackDeviceCollected);
                QuantumEvent.Subscribe<EventBackDeviceDropped>(this, OnEventBackDeviceDropped);
            }
            
            if (!VerifiedFrame.Unsafe.TryGetPointer<PhysicsBody3D>(EntityRef, out var physicsBody)) return;

            if (!physicsBody->Enabled)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnEventValuableHit(EventValuableHit e)
        {
            if (EntityRef != e.Entity) return;

            var damageObj = Instantiate(_damagePrefab, e.Position.ToUnityVector3(), Quaternion.identity);
            
            damageObj.GetComponent<TextMeshPro>().text = $"-{e.HitDamage.AsInt.ToString()}";

            Destroy(damageObj, 2f);
        }

        private void OnEventValuableCollected(EventValuableCollected e)
        {
            if (EntityRef != e.ValuableEntity) return;

            gameObject.SetActive(false);
        }

        private void OnEventValuableDropped(EventValuableDropped e)
        {
            if (EntityRef != e.ValuableEntity) return;

            gameObject.SetActive(true);
        }

        private void OnEventBackDeviceCollected(EventBackDeviceCollected e)
        {
            if (EntityRef != e.BackDeviceEntity) return;

            gameObject.SetActive(false);
        }

        private void OnEventBackDeviceDropped(EventBackDeviceDropped e)
        {
            if (EntityRef != e.BackDeviceEntity) return;

            gameObject.SetActive(true);
        }
    }
}
