using System.Collections.Generic;
using UnityEngine;

namespace Quantum
{
    public class MapCustomData : BaseCustomData
    {
        [Header("Monster Settings")]
        public AssetRef<EntityPrototype>[] MonsterPrototypes;

        [Header("Valuable Settings")]
        public AssetRef<EntityPrototype>[] PossibleValuables;

        public List<ValuableSpawnPoolData> ValuableSpawnPools = new();
    }
}
