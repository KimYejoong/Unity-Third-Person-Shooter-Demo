using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    bool IsFired;
    Vector3 moveVec;
    [SerializeField]
    float speed;

    TrailRenderer trailRenderer;
    Rigidbody rigid;

    const float lifeTime = 3.0f; // 어디에도 충돌하지 않아서 한없이 활성화 상태로 남아있는 경우를 방지하기 위하여 일정 시간이 지나면 풀로 회수
    
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        Initialize();
    }

    public void Initialize()
    {
        StartCoroutine(Lifetime());
    }

    public void Fire(Vector3 vec)
    {        
        trailRenderer.Clear(); ; // 재사용 시 이전에 남은 트레일이 연결되는 걸 방지하기 위해 트레일 렌더러 클리어        

        moveVec = vec.normalized;
        rigid.velocity = moveVec * speed;        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy") // 적에게 닿았을 경우
        {            
            Monster mob = other.GetComponent<Monster>();

            if (mob != null)
            {
                SoundManager.Instance.PlaySFX("ImpactBody");
                GameManager.Instance.monster = mob;
                Vector3 momentum = moveVec * 3f;
                mob.rigid.AddForce(momentum, ForceMode.Impulse); // 총알 진행 방향으로 넉백 부여
                mob.StartCoroutine(mob.OnDamage());
                mob.ChangeHP(-5f); // 순서에 주의! : 위아랫줄 바뀔 경우 몬스터가 죽더라도 그 이후에 표시 대상으로 할당되어 체력바 표시 비활성화가 되지 않을 수 있음
            }
            Disappear();
        } 
        else if (other.tag == "Terrain") // 지형에 닿았을 경우
        {
            SoundManager.Instance.PlaySFX("ImpactGround");
            Disappear();            
        }
    }

    IEnumerator Lifetime()
    {        
        yield return new WaitForSeconds(lifeTime);        
        Disappear();        
    }

    public void Disappear()
    {        
        BulletManager.ReturnObject(this); // 제거 시 오브젝트 풀로 반환    
    }
}

