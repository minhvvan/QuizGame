using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int poolSize;

    private Queue<GameObject> _pool;
    private static ObjectPool _instance;

    public static ObjectPool Instance => _instance;

    private void Awake()
    {
        _instance = this;
        _pool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
    }

    private void CreateNewObject()
    {
        var newObject = Instantiate(cardPrefab);
        newObject.SetActive(false);
        _pool.Enqueue(newObject);
    }

    public GameObject GetObject()
    {
        if(_pool.Count == 0) CreateNewObject();
        
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
