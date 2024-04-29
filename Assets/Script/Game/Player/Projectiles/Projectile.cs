using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody bulletRigid;
    public float bulletSpeed;
    public float despawnTime;

    void Start()
    {
        bulletRigid.AddForce(transform.forward * bulletSpeed * Time.deltaTime, ForceMode.Impulse);
        StartCoroutine(deleteBullet());
    }
    IEnumerator deleteBullet()
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(this.gameObject);
    }
}
