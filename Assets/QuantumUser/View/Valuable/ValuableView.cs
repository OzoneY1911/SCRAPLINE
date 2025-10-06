using TMPro;
using UnityEngine;

namespace Quantum
{
    public class ValuableView : QuantumEntityViewComponent
    {
        [SerializeField] private GameObject _damagePrefab;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventValuableHit>(this, OnEventValuableHit);
        }

        private void OnEventValuableHit(EventValuableHit e)
        {
            if (EntityRef != e.Entity) return;

            var damageObj = Instantiate(_damagePrefab, e.Position.ToUnityVector3(), Quaternion.identity);
            
            damageObj.GetComponent<TextMeshPro>().text = $"-{e.HitDamage.AsInt.ToString()}";

            Destroy(damageObj, 2f);
        }
    }
}
