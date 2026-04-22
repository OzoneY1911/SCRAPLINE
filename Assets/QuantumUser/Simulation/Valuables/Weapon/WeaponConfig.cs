using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class WeaponConfig : ValuableConfig
    {
        [Header("Weapon Settings")]
        public WeaponType WeaponType;
        public FP Damage;
        public FP FireRate;
        public FP Range;

        [Header("Gun Settings")]
        public ushort MagSize;
        public FP ReloadTime;
    }
}