using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShotChangingBonus : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Me = {this}");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("we net spawned");

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        ulong owner = collision.collider.GetComponent<NetworkObject>().OwnerClientId;
        Player player =
            NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();
        player.netPlayerShotSize.Value = 2;
        player.netPlayerShotSpeed.Value = 25;
        Debug.Log("Collided");
        if (IsOwner)
        {
            Destroy(gameObject);
        }
    }
}
