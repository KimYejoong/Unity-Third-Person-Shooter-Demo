using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    void Awake()
    {
        Instance = this;      
    }


    public Player player;

    // 이하 4개 항목은 외부에서 접근은 해야하지만 에디터 상에서 할 이유는 없기 때문에 숨김 처리
    [HideInInspector]
    public Monster monster;
    [HideInInspector]
    public float monsterHP;
    [HideInInspector]
    public float monsterMHP;    
    [HideInInspector]
    public float playTime;

    public GameObject menuPanel;
    public GameObject gamePanel;    
    public Text scoreText;
    public Text playTimeText;
    public Text playerHealthText;
    public Text playerAmmoText;
    public Text playerGrenadeText;

    public RectTransform enemyHealthGroup;
    public RectTransform enemyHealthBar;

    public bool IsPaused;

    // Start is called before the first frame update


    void Start()
    {
        IsPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused == false)
            {
                Time.timeScale = 0;
                IsPaused = true;
                return;
            }
            else
            {
                Time.timeScale = 1;
                IsPaused = false;
                return;
            }
        }

        playTime += Time.deltaTime;
    }

    private void LateUpdate()
    {
        scoreText.text = string.Format("Score : {0:n0}", StageControl.GetScore());
        playerHealthText.text = player.health + " / " + player.maxHealth;
        playerAmmoText.text = player.ammo + " / " + player.maxAmmo;
        playerGrenadeText.text = player.grenades + " / " + player.maxGrenades;

        int hour = (int)(playTime / 3600);
        int min = (int)(playTime - hour * 3600) / 60;
        int sec = (int)(playTime % 60);
        playTimeText.text = "Playtime : " + string.Format("{0:00}", hour) + " : " + string.Format("{0:00}", min) + " : " + string.Format("{0:00}", sec);

        if (monster != null) // 현재 나와 전투 중인(내가 공격하거나, 나를 공격한) 몬스터가 있을 경우, 해당 몬스터의 잔여 체력 표시
        {
            enemyHealthGroup.anchoredPosition = Vector3.down * 50;
            enemyHealthBar.localScale = new Vector3(monster.HP / monster.MaxHP, 1, 1);            
        }
        else // 나와 전투 중인 몬스터가 없는 경우(혹은 잡아서 사라졌을 경우), 체력바 표시하지 않도록 화면 밖으로 치워버림
            enemyHealthGroup.anchoredPosition = Vector3.up * 200;
    }
}
