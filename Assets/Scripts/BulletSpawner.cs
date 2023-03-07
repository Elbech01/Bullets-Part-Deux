using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : NetworkBehaviour
{
    public Rigidbody bullet;
    private float bulletSpeed = 50f;
    private float timeToLive = 3f;

    [ServerRpc]
    public void FireServerRpc(Color color, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"I am owned by {rpcParams.Receive.SenderClientId}");
        Rigidbody newBullet = Instantiate(bullet, transform.position, transform.rotation);
        newBullet.GetComponent<NetworkObject>().SpawnWithOwnership(
            rpcParams.Receive.SenderClientId);
        newBullet.velocity = transform.forward * bulletSpeed;
        //newBullet.transform.Find("Sphere").GetComponent<MeshRenderer>().material.color = color;
        Destroy(newBullet.gameObject, timeToLive);
    }

}
