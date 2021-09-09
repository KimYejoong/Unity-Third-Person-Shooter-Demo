using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeBox : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime); // 아이템 회전 시킴
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Player player = other.GetComponentInParent<Player>();
            player.grenades = Mathf.Min(player.grenades + 1, player.maxGrenades);
            SoundManager.Instance.PlaySFX("GetAmmo");

            Destroy(this.gameObject);
        }
    }
}
