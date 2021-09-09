using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject particle;
    public Rigidbody rigid;
    public float blastRadius;
    public float maximumForce;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Explosion());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);

        SoundManager.Instance.PlaySFX("Explosion");

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObj.SetActive(false);
        particle.SetActive(true);

        Collider[] collides = Physics.OverlapSphere(transform.position, blastRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider hit in collides)
        {
            var target = hit.transform.GetComponent<Monster>();

            if (target != null)
            {
                Vector3 vec = (hit.transform.position - transform.position);
                float force = (1 - (vec.magnitude / blastRadius)) * maximumForce; // 거리에 반비례하여 가속치 계산, 폭심지에 가까울수록 가속이 크고 최대 거리일 경우 0            
                target.rigid.AddForce(vec.normalized * force, ForceMode.Impulse);
                target.StartCoroutine(target.OnDamage());
                target.ChangeHP(-20 * (1 - (vec.magnitude / blastRadius))); // 폭심지에 가까울수록 큰 데미지
            }            
        }

        Destroy(this.gameObject, 5f);
    }
}
