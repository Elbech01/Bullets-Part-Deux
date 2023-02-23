using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    private static Color[] availColors = new Color[] {
            Color.black, Color.blue, Color.cyan,
            Color.gray, Color.green, Color.yellow };
    private int hostColorIndex = 0;
    private Camera _camera;
    public NetworkVariable<Color> netPlayerColor = new NetworkVariable<Color>();
    public override void OnNetworkSpawn()
    {
        netPlayerColor.OnValueChanged += OnPlayerColorChanged;
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = IsOwner;

        netPlayerColor.Value = availColors[hostColorIndex];
        ApplyPlayerColor();
    }


    public void ApplyPlayerColor()
    {
       // GetComponent().material.color = netPlayerColor.Value;
    }


    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyPlayerColor();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RequestNextColorServerRpc();
        }
    }
    [ServerRpc]
    void RequestNextColorServerRpc(ServerRpcParams serverRpcParams = default)
    {
        hostColorIndex += 1;
        if (hostColorIndex > availColors.Length - 1)
        {
            hostColorIndex = 0;
        }

        Debug.Log($"Host color index = {hostColorIndex} for {serverRpcParams.Receive.SenderClientId}");
        netPlayerColor.Value = availColors[hostColorIndex];
    }

    [ServerRpc]
    public void RequestPositionForMovementServerRpc(Vector3 posChange, Vector3 rotChange, ServerRpcParams serverRpcParams = default)
    {
        transform.Translate(posChange);
        transform.Rotate(rotChange);
    }
}
