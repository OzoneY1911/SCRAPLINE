using UnityEngine;

namespace Quantum
{
    public class MonsterConfig : AssetObject
    {
        [Header("Monster Settings")]
        public bool CanPatrol;
        public bool CanChase;
    }
}