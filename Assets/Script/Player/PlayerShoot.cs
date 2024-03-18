using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
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
        if(!IsOwner) return;
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if(ammo > 0)
            {
                RequestFireServerRpc();
                Fire();
            }
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
        if (!IsOwner)
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
