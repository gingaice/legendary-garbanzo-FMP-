using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField]
    public GameObject Gun;
    [SerializeField]
    public int ammo;

    [SerializeField]
    private GameObject bullet;
    public Transform bulletSpawn;

    private const int maxAmmoCount = 12;

    private void Update()
    {
        if(!IsOwner) return;
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if(ammo > 0)
            {
                RequestFireServerRpc(); //sends a request to the server too spawn in the projectile so that others can see the projectiles
                Fire();
            }
        }

        if(!IsOwner) return;
        if(Input.GetKeyUp (KeyCode.Mouse1))
        {
            ammo = maxAmmoCount;
        }
    }

    [ServerRpc]
    void RequestFireServerRpc()
    {
        FireClientRpc();
    }

    [ClientRpc]
    void FireClientRpc() //this is client side as it is the player telling the server that a bullet has been fired from this owner, it will then spawn the proj in on each game saving sending over more data
    {
        if (!IsOwner) //since i shoot locally in update this removes the feel of delay after shot
        {
            Fire();
        }
    }
    public void Fire()
    {

        Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
        ammo = ammo - 1;

    }
}
