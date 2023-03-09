using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    private static Color[] availColors = new Color[] {
            Color.black, Color.blue, Color.cyan,
            Color.gray, Color.green, Color.yellow };
    private int hostColorIndex = 0;
    private Camera _camera;
    public NetworkVariable<Color> netPlayerColor = new NetworkVariable<Color>();
    public NetworkVariable<int> netPlayerScore = new NetworkVariable<int>(100);
    private float movementSpeed = 30.0f;
    private float rotationSpeed = -150.0f;
    public BulletSpawner bulletSpawner;

    public override void OnNetworkSpawn()
    {
        netPlayerColor.OnValueChanged += OnPlayerColorChanged;
        netPlayerScore.OnValueChanged += OnPlayerScoreChanged;
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = IsOwner;

        bulletSpawner = transform.Find("Sphere (2)").transform.Find("BulletSpawner").GetComponent<BulletSpawner>();

        netPlayerColor.Value = availColors[hostColorIndex];
        ApplyPlayerColor();
        UpdateScoreDiaplay();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsHost)
        {
            if (collision.gameObject.CompareTag("Bullet"))
            {
                HostHandleBulletCollision(collision.gameObject);
            }
        }
        
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
    public void OnPlayerScoreChanged(int previous, int current)
    {
        UpdateScoreDiaplay();
    }

    private void UpdateScoreDiaplay()
    {
        if (IsOwner)
        {
            Debug.Log($"My score = {netPlayerScore.Value}");
        }
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
        if (!isShiftDown)
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
        if (IsOwner)
        {
            UpdateOwner();
        }        
    }

    void UpdateOwner()
    {
        Vector3 moveBy = CalcMovementFromInput(Time.deltaTime);
        Vector3 rotateBy = CalcRotationFromInput(-Time.deltaTime);
        RequestPositionForMovementServerRpc(moveBy, rotateBy);
            if (Input.GetButtonDown("Fire1"))
            {
                RequestNextColorServerRpc();
            }
            if (Input.GetButtonDown("Fire2"))
            {
                bulletSpawner.FireServerRpc(netPlayerColor.Value);
            }

    }
    private void HostHandleBulletCollision(GameObject bullet)
    {
        Bullet bulletScript = (Bullet)bullet.GetComponent("Bullet");
        netPlayerScore.Value -= 1;
        RequestNextColorServerRpc();
        ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
        Player otherPlayer =
            NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();
        otherPlayer.netPlayerScore.Value += 1;
        RequesScoreUpdateServerRpc();
        Destroy(bullet);
    }
    [ServerRpc(RequireOwnership = false)]
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

    [ServerRpc(RequireOwnership = false)]
    void RequesScoreUpdateServerRpc(ServerRpcParams serverRpcParams = default)
    {
        UpdateScoreDiaplay();
    }

    [ServerRpc]
    public void RequestPositionForMovementServerRpc(Vector3 posChange, Vector3 rotChange, ServerRpcParams serverRpcParams = default)
    {
        transform.Translate(posChange);
        transform.Rotate(rotChange);
    }
}
