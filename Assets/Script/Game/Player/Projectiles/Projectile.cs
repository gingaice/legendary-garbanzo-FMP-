using ParrelSync.NonCore;
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
            Debug.Log("hit COL");
            if(other.gameObject.layer == 7)
            {
                //PlayerShoot.Instance.health = PlayerShoot.Instance.health - 1;
                other.gameObject.GetComponent<PlayerShoot>().takeDmg();
                //GetComponent<PlayerShoot>
                //PlayerShoot.Instance.takeDmg();
                Debug.Log("player hit");
                Destroy(this.gameObject);
            }
        }
    }
    IEnumerator deleteBullet()
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(this.gameObject);
    }
}
