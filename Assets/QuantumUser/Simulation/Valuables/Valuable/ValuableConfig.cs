using UnityEngine;

namespace Quantum
{
    public class ValuableConfig : AssetObject
    {
        [Header("Core Settings")]
        public string DisplayName;

        public bool IsPocketValuable;
        public bool IsBackDevice;

        public UseMode UseMode;

        [Header("Value Settings")]
        public ushort DefaultValue;
        public ushort DefaultFragility;

        [Header("Presentation Prefabs")]
        public GameObject FPVPrefab;
        public GameObject TPVPrefab;
    }
}