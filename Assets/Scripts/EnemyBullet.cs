using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{

    public int damage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Monster parent = GetComponentInParent<Monster>();
            Player player = other.GetComponentInParent<Player>(); // 플레이어 오브젝트가 이원화된 구조라서 상위 오브젝트(Character)를 통해 접근
        
            Vector3 originalVec = (other.transform.position - parent.transform.position);
            Vector3 tempVec = new Vector3(originalVec.x, 0, originalVec.z).normalized * 3f; // 공중으로 뜨지 않게 하기 위해 Y축(수직) 성분은 제외함

            GameManager.Instance.monster = parent; // 현재 플레이어를 공격중인 몬스터의 체력 상태를 UI 상단 체력바에 표시

            player.GetComponentInChildren<Rigidbody>().AddForce(tempVec, ForceMode.Impulse);
            player.ChangeHP(-5f);
        }
        
    }
}
