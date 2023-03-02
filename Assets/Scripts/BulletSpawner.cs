using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : NetworkBehaviour
{
    public Rigidbody bullet;
    private float bulletSpeed = 40f;
    private float timeToLive = 3F;

    [ServerRpc]
    public void FireServerRpc(Color color, ServerRpcParams rpcParams = default)
    {
        Rigidbody newBullet = Instantiate(bullet, transform.position, transform.rotation);
        newBullet.GetComponent<NetworkObject>().SpawnWithOwnership(
            rpcParams.Receive.SenderClientId);
        newBullet.velocity = transform.forward * bulletSpeed;
        Destroy(newBullet.gameObject, timeToLive);
    }
}
