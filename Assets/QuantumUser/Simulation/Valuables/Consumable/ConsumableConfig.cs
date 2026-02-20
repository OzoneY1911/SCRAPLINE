using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class ConsumableConfig : ValuableConfig
    {
        [Header("Consumable Settings")]
        public FP HealthDelta;
        public FP StaminaDelta;
    }
}
