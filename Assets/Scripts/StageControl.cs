using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageControl : MonoBehaviour
{
    public static StageControl Instance;

    [SerializeField]
    float MobSpawnDelay;

    [SerializeField]
    int MaxMobPopulation; // To-do

    static int score;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(MobSpawn(MobSpawnDelay));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScore(int amount)
    {
        score = amount;
    }

    public static void AddScore(int amount)
    {
        score = Mathf.Clamp(0, score + amount, int.MaxValue);
    }

    public static int GetScore()
    {
        return score;
    }

    IEnumerator MobSpawn(float time)
    {
        yield return new WaitForSeconds(time);

        var monster = MonsterManager.GetObject();
        monster.Initialize(); // HP 최대치로 초기화
                
        monster.transform.position = new Vector3(Random.Range(-50, 50), 2, Random.Range(-50, 50));       

        StartCoroutine(MobSpawn(MobSpawnDelay)); // 코루틴 내에서 다시 코루틴 시작해서 주기적으로 반복 호출        
    }
}
