using UnityEngine;

namespace Quantum
{
    public class ValuableConfig : AssetObject
    {
        [Header("Valuable Settings")]
        public string DisplayName;

        public bool IsPocketValuable;

        public ushort DefaultValue;
        public ushort DefaultFragility;

        public GameObject FPSPrefab;
    }
}