using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShotChangingPowerUp : NetworkBehaviour
{
    public Rigidbody shotChangingPU;

    public bool spawnOnLoad = true;
    public float refreshTime = 10f;
    public float timeUntilSpawn = 0.0f;

    public Rigidbody currentBonus = null;

    public Transform spawnPointTransform;
    public void Start()
    {
        spawnPointTransform = transform.Find("Sphere");
    }

    public void Update()
    {

        if (IsServer)
        {
            ServerUpdate();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer && shotChangingPU != null)
            spawnBonus();
    }
    private void HostOnNetworkSpawn()
    {
        if (currentBonus != null && spawnOnLoad)
        {
            spawnBonus();
        }
    }

    private void spawnBonus()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = -66;
        spawnPosition.x -= 4;
        spawnPosition.z += 7; 
        Rigidbody bonusSpawn = Instantiate(
            shotChangingPU,
            spawnPosition,
            Quaternion.identity);
        bonusSpawn.GetComponent<NetworkObject>().Spawn();
        currentBonus = bonusSpawn;
    }

    private void ServerUpdate()
    {
        if (timeUntilSpawn > 0f)
        {
            timeUntilSpawn -= Time.deltaTime;
            if (timeUntilSpawn <= 0f)
            {
                spawnBonus();
            }
        }
        else if (currentBonus == null)
        {
            timeUntilSpawn = refreshTime;
            Debug.Log($"Time until spawn = {timeUntilSpawn}");
        }

    }
}
