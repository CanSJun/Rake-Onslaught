using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;


class PoolInfo
{
    public ObjectPool<GameObject> pool;
    public AsyncOperationHandle<GameObject> handle;
    public Transform parent;
    public AssetReferenceGameObject prefabRef;
}

public class PoolManager : MonoBehaviour
{
    private readonly Dictionary<string, PoolInfo> _pools = new();

    public async UniTask CreatePoolAsync(AssetReferenceGameObject prefabRef, Transform parent, int defaultCount, int maxCount, AsyncOperationHandle<GameObject> handle = default)
    {

        if (!handle.IsValid())
        {
            handle = prefabRef.LoadAssetAsync<GameObject>();
            await handle.Task;
        }
        else
        {
            if (!handle.IsDone) await handle.Task;
        }

        var prefab = handle.Result;
        var identity = prefab.GetComponent<PoolIdentity>();

        if (identity == null)
        {
            Debug.LogError($"[PoolManager] PoolIdentity missing on prefab: {prefab.name}");
            return;

        }
        var key = identity.Id;
        if (_pools.ContainsKey(key)) return;

        var info = new PoolInfo
        {
            handle = handle,
            parent = parent,
            prefabRef = prefabRef
        };

        PoolKeys.Register(key);
        info.pool = new ObjectPool<GameObject>(
            createFunc: () => { 
                var obj = Instantiate(prefab, parent);
                var agent = obj.GetComponent<NavMeshAgent>();
                if (agent) agent.enabled = false;
                obj.SetActive(false);
                return obj;
                },
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: defaultCount,
            maxSize: maxCount
        );

        // for (int i = 0; i < defaultCount; i++) info.pool.Release(info.pool.Get());

        //info.pool.Release(info.pool.Get()); 방식은 프리워밍 중에 SetActive(true)가 먼저 타서 부담이 생김, 그래서 프리워밍때 OnGet경로를 안타게!
        for (int i = 0; i < defaultCount; i++)
        {
            var obj = Instantiate(prefab, parent);
            obj.SetActive(false); // 풀에 들어갈땐 비활성 상태
            info.pool.Release(obj); // 만약을 위해서 <- actionOnRelease가 한번 더 SetActive(false)를 하긴 함! 

        }

        _pools.Add(key, info);
    }
    public async UniTask InitializeAsync(PoolConfig config, Transform root)
    {
        foreach (var entry in config.prefab)
        {
            var handle = entry.prefab.LoadAssetAsync<GameObject>();
            await handle.Task;

            var identity = handle.Result.GetComponent<PoolIdentity>();
            if (identity == null)
            {
                Debug.LogError($"[PoolManager] PoolIdentity missing: {handle.Result.name}");
                entry.prefab.ReleaseAsset(); // 정리
                continue;
            }

            var parent = new GameObject(identity.Id).transform;
            parent.SetParent(root);

            await CreatePoolAsync(entry.prefab, parent, entry.defaultCount, entry.maxCount, handle);
        }
    }
    public bool HasPool(string key) => _pools.ContainsKey(key);
    public GameObject Spawn(string poolKey)
    {
        if (!_pools.TryGetValue(poolKey, out var info))
        {
            Debug.LogError($"[PoolManager] Pool not found: {poolKey}");
            return null;
        }
        return info.pool.Get();
    }
    public void Release(GameObject obj)
    {
        if (!obj) return;

        var id = obj.GetComponent<PoolIdentity>();
        if (id == null || !_pools.TryGetValue(id.Id, out var info))
        {
            Debug.LogWarning($"[PoolManager] Invalid pooled object: {obj.name}");
            Destroy(obj);
            return;
        }

        info.pool.Release(obj);
    }
    public void DestroyPool(string key)
    {
        if (!_pools.TryGetValue(key, out var info)) return;
        info.pool.Clear();
        if (info.handle.IsValid())info.prefabRef.ReleaseAsset();
        _pools.Remove(key);
    }

    public void DestroyAllPools()
    {
        var keys = new List<string>(_pools.Keys);
        foreach (var key in keys) DestroyPool(key);
        
    }

}
