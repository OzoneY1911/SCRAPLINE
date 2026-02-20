using UnityEngine;

namespace Quantum
{
    public class FlashlightConfig : ValuableConfig
    {
        [Header("Flashlight Settings")]
        public ushort MaxCharge;
        public ushort DischargePerSecond;
    }
}