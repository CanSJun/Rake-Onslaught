using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


[Serializable]
public class PoolEntry
{
    public AssetReferenceGameObject prefab;
    public int defaultCount;
    public int maxCount;
}

[CreateAssetMenu(menuName = "Config/PoolConfig")]
public class PoolConfig : ScriptableObject
{
    public List<PoolEntry> prefab;
}
