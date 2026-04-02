using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class WeaponConfig : ValuableConfig
    {
        [Header("Weapon Settings")]
        public ushort Damage;
        public ushort FireRate;
        public FP Range;
    }
}