using UnityEngine;

namespace Quantum
{
    public class MonsterConfig : AssetObject
    {
        [Header("Monster Behaviour")]
        public bool CanChase;
        public bool CanAttack;

        [Header("Monster Settings")]
        public ushort AttackDistance;
    }
}