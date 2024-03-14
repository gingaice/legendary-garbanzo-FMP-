using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField]
    public GameObject Gun;
    [SerializeField]
    private int ammo;
    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    public float bulletForce;
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if(ammo > 0)
            {
                Fire();
            }
        }
    }
    public void Fire()
    {

        Instantiate(bullet, Gun.transform.position, Quaternion.identity);

    }
}
