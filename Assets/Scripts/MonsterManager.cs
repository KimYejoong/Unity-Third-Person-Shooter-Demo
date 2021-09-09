using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;

    [SerializeField]
    private GameObject Prefab;

    [SerializeField]
    private int poolSize;    

    Queue<Monster> pool = new Queue<Monster>();

    private void Awake()
    {
        Instance = this;
        Initialize(poolSize);
    }

    private void Initialize(int initCount)
    {
        for (int i = initCount; i > 0; i--)
        {
            pool.Enqueue(CreateNewObject());
        }
    }

    private Monster CreateNewObject()
    {
        var newObj = Instantiate(Prefab).GetComponent<Monster>();
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public static Monster GetObject()
    {
        if (Instance.pool.Count > 0)
        {
            var obj = Instance.pool.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else // 풀 제한치를 초과할 경우 경고 표시
        {
            Debug.LogError("Exceed pool limitation!");

            var newObj = Instance.CreateNewObject();
            newObj.transform.SetParent(null);
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public static void ReturnObject(Monster obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        Instance.pool.Enqueue(obj);
    }
}
