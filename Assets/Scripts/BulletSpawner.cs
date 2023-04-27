using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : NetworkBehaviour
{
    public Rigidbody bullet;
    private float timeToLive = 1.5f;

    [ServerRpc]
    public void FireServerRpc(int speed, int scale, ServerRpcParams rpcParams = default)
    {
        Rigidbody newBullet = Instantiate(bullet, transform.position, transform.rotation);
        newBullet.GetComponent<NetworkObject>().SpawnWithOwnership(
            rpcParams.Receive.SenderClientId);
        newBullet.transform.localScale = new Vector3(scale, scale, scale);
        newBullet.velocity = transform.forward * speed;
        Destroy(newBullet.gameObject, timeToLive);
    }

}
