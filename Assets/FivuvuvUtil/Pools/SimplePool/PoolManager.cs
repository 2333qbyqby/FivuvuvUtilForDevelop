using System;
using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil.SimplePool
{
    public class PoolManager : MonoSingleton<PoolManager>
{

    public void Initialize()
    {
        foreach (var prefab in prefabs)
        {
            if (poolDictionary.ContainsKey(prefab.name + "Manager")) continue;
            var poolManager = new GameObject(prefab.name + "Manager");
            poolManager.transform.SetParent(transform);
            var pool = poolManager.AddComponent<ObjPool>();
            pool.pfb = prefab;
            pool.fillCountForOneTime = defaultFillCountOneTime;

            poolDictionary.Add(poolManager.name, pool);
        }
    }

    public GameObject[] prefabs;
    public float defaultFillCountOneTime = 10;
    private Dictionary<string, ObjPool> poolDictionary = new Dictionary<string, ObjPool>();

    private void OnEnable()
    {
        //这边是调用事件，具体得另外写
        //EventHandler.ReturnToPool += ReturnObjToPool;
    }

    private void OnDisable()
    {
        //这边是注销事件，具体得另外写
        //EventHandler.ReturnToPool -= ReturnObjToPool;
    }

    public ObjPool GetPool(string poolName)
    {
        if (poolDictionary.ContainsKey(poolName))
            return poolDictionary[poolName];
        return null;
    }

    public void ReturnObjToPool(string pfbName, GameObject pfb)
    {
        var pool = GetPool(pfbName + "Manager");
        pool.ReturnToPool(pfb);
    }


}
}
