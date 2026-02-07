using UnityEngine;

namespace Quantum
{
    public class ValuableConfig : AssetObject
    {
        public string DisplayName;

        public bool IsPocketValuable;

        public ushort DefaultValue;
        public ushort DefaultFragility;

        public GameObject FPSPrefab;
    }
}
