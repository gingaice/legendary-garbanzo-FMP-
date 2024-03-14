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
    public Transform bulletSpawn;

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

        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
        ammo = ammo - 1;

    }
}
