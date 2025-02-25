using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private int poolSize;
    [SerializeField] private GameObject parentGameObject;

    private Queue<GameObject> _pool;

    private void Awake()
    {
        _pool = new Queue<GameObject>();
    }

    private void CreateNewObject(GameObject prefab)
    {
        var newObject = Instantiate(prefab, parentGameObject.transform);
        newObject.SetActive(false);
        _pool.Enqueue(newObject);
    }

    public GameObject GetObject(GameObject prefab)
    {
        if(_pool.Count == 0) CreateNewObject(prefab);
        
        var obj = _pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }
}
