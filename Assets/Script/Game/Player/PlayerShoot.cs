using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : NetworkBehaviour
{
    public static PlayerShoot Instance { get; private set; }

    [SerializeField]
    public GameObject Gun;

    public int ammo;

    [SerializeField]
    private GameObject bullet;
    public Transform bulletSpawn;

    private const int maxAmmoCount = 12;
    private bool isReloading = false;
    public bool CanShoot = true;
    private float reloadSpeed = 2;

    [SerializeField]
    private int health;
    private const int _maxHealth = 5;

    private void Update()
    {
        if(!IsOwner) return;
        GameManager.Instance._AmmoCount.text = ammo.ToString();        
        if(!IsOwner) return;
        GameManager.Instance._healthCount.text = health.ToString();

        if (GameManager.Instance.isPlayerPaused)
        {
            CanShoot = false;
        }

        if (!IsOwner) return;
        if (Input.GetKeyUp(KeyCode.Mouse0) && CanShoot)
        {
            if(ammo > 0)
            {
                RequestFireServerRpc(); //sends a request to the server too spawn in the projectile so that others can see the projectiles
                Fire();
            }
        }
        if (!IsOwner) return;
        if(Input.GetKeyDown (KeyCode.R) && !isReloading)
        {
            RequestReloadServerRpc();
            //ReloadCoroutine(reloadSpeed);
            GameManager.Instance.ReloadSlider.gameObject.SetActive(true);
            GameManager.Instance.ReloadSlider.maxValue = reloadSpeed;
            StartCoroutine(ReloadCoroutine(reloadSpeed));
        }
    }
    public void takeDmg()
    {
        health = health - 1;

        if(health < 1)
        {
            Respawn();
        }
    }
    private void Respawn()
    {
        transform.position = Spawnpoints.Instance.getRandomPoint();
        health = _maxHealth;
    }

    #region reloading
    [ServerRpc]
    void RequestReloadServerRpc()
    {
        ReloadClientRpc();
    }

    [ClientRpc]
    void ReloadClientRpc()
    {
        if (!IsOwner)
        {
            //ReloadCoroutine(reloadSpeed);
            StartCoroutine(ReloadCoroutine(reloadSpeed));
        }
    }
    private IEnumerator ReloadCoroutine(float waitTimeSeconds)
    {
        isReloading = true;
        CanShoot = false;

        //var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        //GameManager.Instance.ReloadSlider.value = delay.waitTime;

        //yield return delay;
        if (!IsOwner) yield break;
        float timer = waitTimeSeconds;

        while (timer > 0)
        {
            GameManager.Instance.ReloadSlider.value = timer; // Update slider value to reflect remaining wait time
            yield return new WaitForSeconds(0.5f); // Wait for 1 second
            timer -= 0.5f; // Decrease timer
        }


        ammo = maxAmmoCount;
        CanShoot = true;
        isReloading = false;
        GameManager.Instance.ReloadSlider.gameObject.SetActive(false);
    }
    #endregion

    #region firing
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
    #endregion
}
