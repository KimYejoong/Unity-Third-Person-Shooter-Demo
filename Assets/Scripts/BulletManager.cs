using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance;
        
    [SerializeField]
    private GameObject Prefab;
        
    [SerializeField]
    private int poolSize;

    Queue<Bullet> pool = new Queue<Bullet>();

    private void Awake() {
        Instance = this;
        Initialize(poolSize);
    }

    private void Initialize(int initCount) 
    {
        for (int i = 0; i < initCount; i++) 
        {
            pool.Enqueue(CreateNewObject());
        }
    }

    private Bullet CreateNewObject() {
        var newObj = Instantiate(Prefab).GetComponent<Bullet>(); 
        newObj.gameObject.SetActive(false); 
        newObj.transform.SetParent(transform); 
        return newObj;
    }

    public static Bullet GetObject() { 
        if (Instance.pool.Count > 0) 
        {
            var obj = Instance.pool.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        } 
        else // 오브젝트 풀의 제한치를 초과할 경우, 작동을 위해 일단 만들지만 디버그를 위해 경고 표시
        {
            Debug.LogError("Exceed pool limitation!");

            var newObj = Instance.CreateNewObject();
            newObj.transform.SetParent(null);
            newObj.gameObject.SetActive(true);             
            return newObj;            
        }
    }

    public static void ReturnObject(Bullet obj) 
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        Instance.pool.Enqueue(obj);        
    }    
}
