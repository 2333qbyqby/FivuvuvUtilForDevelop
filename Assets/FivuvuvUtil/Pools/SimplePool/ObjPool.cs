using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjPool : MonoBehaviour
{
    public GameObject pfb;
    public float fillCountForOneTime = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        FillPool();
    }

    public void FillPool()
    {
        for (int i = 0; i < fillCountForOneTime; i++)
        {
            var newObj = Instantiate(pfb, transform);
            newObj.SetActive(false);
            pool.Enqueue(newObj);
        }
    }

    public GameObject GetFromPool()
    {
        if (pool.Count <= 1)
            FillPool();

        var obj = pool.Dequeue();
        if (obj != null)
            obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
