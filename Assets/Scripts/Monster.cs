using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    [SerializeField]
    public float MaxHP = 20;
    public float HP;

    public GameObject ammoBox;
    public GameObject grenadeBox;

    private Transform targetTransform;
    private NavMeshAgent navAgent;

    [HideInInspector]
    public Rigidbody rigid; // 외부에서 접근할 필요는 있지만 에디터에서 할당할 필요는 없으므로 숨김
 
    public enum MONSTER_STATE { Idle, Trace, Attack, Dead };
    public MONSTER_STATE currentState = MONSTER_STATE.Idle;

    public BoxCollider Melee;

    public float traceDistance; // 추적 시작 거리
    public float attackDistance; // 공격 시작 거리

    private bool IsDead = false;
    public bool IsChasing;
    public bool IsAttacking;

    MeshRenderer[] meshes;

    // Start is called before the first frame update
    void Awake()
    {       
        Initialize();        
    }

    public void Initialize()
    {
        IsDead = false;
        SetHP(MaxHP);

        targetTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        navAgent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        meshes = GetComponents<MeshRenderer>();

        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.white;
        }

        StartCoroutine(Chase()); // 배치되고나서 잠시 후 추적 시작
    }

    IEnumerator Chase()
    {
        yield return new WaitForSeconds(1f); // 1초 대기 후 추적 시작
        IsChasing = true;
    }

    private void Update()
    {
        if (IsDead)
            return;

        if (navAgent.enabled) {
            navAgent.SetDestination(targetTransform.position);
            navAgent.isStopped = !IsChasing;            
        }

        Targeting();
    }

    void Targeting()
    {
        if (Vector3.Distance(targetTransform.position, transform.position) <= attackDistance && !IsAttacking)
        {
            StartCoroutine(MeleeAttack());
        }
    }

    public void SetHP(float hp)
    {
        HP = hp;
        // Debug.Log(HP);
    }

    public void ChangeHP(float amount)
    {
        HP = Mathf.Clamp(0, HP + amount, MaxHP);
        //Debug.Log(HP);

        if (HP == 0) // HP 증감 결과 0이 되면 제거(오브젝트 풀에 반환)
        {
            Disappear();
        }
    }

    public void Disappear()
    {
        if (GameManager.Instance.monster == this.GetComponent<Monster>()) // 만약 현재 이 개체의 HP가 상단 UI에 표시 중이라면, 더 이상 표시하지 않도록 게임 매니저의 참조값을 null로 초기화
            GameManager.Instance.monster = null;
        StageControl.AddScore(5);

        if (Random.Range(1, 5) <= 3)
        {
            Instantiate(ammoBox, transform.position + Vector3.up * 0.8f, Quaternion.identity, null);
        } else
        {
            Instantiate(grenadeBox, transform.position + Vector3.up * 0.8f, Quaternion.identity, null);
        }
        IsDead = true;
        StopAllCoroutines(); // 강제로 공격 등 코루틴 중단
        MonsterManager.ReturnObject(this);        
    }

    IEnumerator MeleeAttack()
    {
        IsChasing = false;
        IsAttacking = true;

        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.blue;
        }

        Melee.enabled = true;
        yield return new WaitForSeconds(0.3f);

        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.white;
        }

        Melee.enabled = false;

        yield return new WaitForSeconds(1f);

        IsChasing = true;
        IsAttacking = false;        
    }

    public IEnumerator OnDamage()
    {
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
        }

        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.white;
        }
    }
}
