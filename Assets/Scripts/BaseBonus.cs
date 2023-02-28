using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BaseBonus : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("We started");
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
        Debug.Log("Collided");
        if (IsOwner)
        {
            Destroy(gameObject);
        }
    }
}
