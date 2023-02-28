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
    private float movementSpeed = 0.2f;
    private float rotationSpeed = 1.0f;
    public Vector3 movement = new (0, 0, 0);
    public Vector3 rotate = new (0, 0, 0);


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
        GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
        transform.Find("Sphere").GetComponent<MeshRenderer>().material.color = netPlayerColor.Value;
    }


    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyPlayerColor();
    }

    private Vector3 CalcMovementFromInput(float delta)
    {
        bool isShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float xMove = 0.0f;
        float zMove = Input.GetAxis("Vertical");
        if (isShiftDown)
        {
            xMove = Input.GetAxis("Horizontal");
        }
        Vector3 move = new Vector3(xMove, 0, zMove);
        move *= movementSpeed * delta;
        return move;
    }
    private Vector3 CalcRotationFromInput(float delta)
    {
        bool isShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float yRotate = 0.0f;
        if (isShiftDown)
        {
            yRotate = Input.GetAxis("Horizontal");
        }
        Vector3 setVector = new Vector3(0, yRotate, 0);
        setVector *= rotationSpeed * delta;
        return setVector;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RequestNextColorServerRpc();
        }
        if (Input.GetButtonDown("Fire2"))
        {
            RequestPositionForMovementServerRpc(movement, rotate);
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
