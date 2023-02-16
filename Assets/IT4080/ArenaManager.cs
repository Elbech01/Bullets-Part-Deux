using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    public Player playerPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SpawnAllPlayers();
        }
    }

    private void SpawnAllPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {

        }
    }

    private Player SpawnPlayerForClient(ulong clientId)
    {
        Vector3 spawnPosition = new Vector3(0, 1, clientId * 5);
        Player playerSpawn = Instantiate(
            playerPrefab,
            spawnPosition,
            Quaternion.identity);
        playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        return playerSpawn;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
