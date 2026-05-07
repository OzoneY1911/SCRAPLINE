using Quantum;
using System;
using System.Collections.Generic;

[Serializable]
public struct ValuableSpawnPoolData
{
    public MapPointData Point;
    public List<AssetRef<EntityPrototype>> PossibleValuables;
}