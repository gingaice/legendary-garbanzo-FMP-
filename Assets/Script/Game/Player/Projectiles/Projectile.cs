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
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Objects")
        {
            //Debug.Log("hit COL");
            if(other.gameObject.layer == 7) //7 is target layer
            {
                other.gameObject.GetComponent<PlayerShoot>().takeDmg();
            }
        }
        Destroy(this.gameObject);
    }
    IEnumerator deleteBullet()
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(this.gameObject);
    }
}
