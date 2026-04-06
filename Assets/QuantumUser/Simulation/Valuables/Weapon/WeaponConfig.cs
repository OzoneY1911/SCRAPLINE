using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class WeaponConfig : ValuableConfig
    {
        [Header("Weapon Settings")]
        public FP Damage;
        public FP FireRate;
        public FP Range;
    }
}