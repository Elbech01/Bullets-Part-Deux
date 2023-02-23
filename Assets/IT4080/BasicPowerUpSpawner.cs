using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class BasicPowerUpSpawner : NetworkBehaviour
{
    public bool spawnOnLoad = true;
    public float refreshTime = 2f;
    public Rigidbody bonusPrefab = null;

    public float timeUntilSpawn = 0.0f;
    public Rigidbody currentBonus = null;

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
        if (IsServer && bonusPrefab != null )
            spawnBonus();
    }

    private void spawnBonus()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = 2;
        Rigidbody bonusSpawn = Instantiate(
            bonusPrefab,
            spawnPosition,
            Quaternion.identity);
        bonusSpawn.GetComponent<NetworkObject>().Spawn();
    }

    private void ServerUpdate()
    {
        if (timeUntilSpawn > 0f)
        {
            timeUntilSpawn = Time.deltaTime;
            if(timeUntilSpawn <= 0)
            {
                spawnBonus();
            }
            else if (currentBonus == null)
            {
                timeUntilSpawn = refreshTime;
            }
        }
    }

}
