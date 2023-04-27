using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using TMPro;

public class Player : NetworkBehaviour
{
    private static Color[] availColors = new Color[] {
            Color.black, Color.blue, Color.cyan,
            Color.gray, Color.green, Color.yellow };
    private int hostColorIndex = 0;
    private Camera _camera;
    private TextMeshPro _textMeshPro;
    private TextMeshPro _textMeshPro2;
    public NetworkVariable<Color> netPlayerColor = new NetworkVariable<Color>();
    public NetworkVariable<int> netPlayerScore3 = new NetworkVariable<int>(50);
    public NetworkVariable<int> netPlayerDamage = new NetworkVariable<int>(10);
    public NetworkVariable<int> netPlayerShotSize = new NetworkVariable<int>(1);
    public NetworkVariable<int> netPlayerShotSpeed = new NetworkVariable<int>(50);
    public NetworkVariable<int> netPlayerKills = new NetworkVariable<int>(0);
    private float movementSpeed = 25.0f;
    private float rotationSpeed = -150.0f;
    public BulletSpawner bulletSpawner;
    public int totalPlayers = 0;

    public override void OnNetworkSpawn()
    {
        netPlayerColor.OnValueChanged += OnPlayerColorChanged;
        netPlayerScore3.OnValueChanged += OnPlayerScoreChanged;
        netPlayerKills.OnValueChanged += OnPlayerKillsChanged;
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = IsOwner;
        _textMeshPro = transform.Find("Score").GetComponent<TextMeshPro>();
        _textMeshPro.enabled = IsOwner;
        _textMeshPro2 = transform.Find("Kills").GetComponent<TextMeshPro>();
        _textMeshPro2.enabled = IsOwner;
        totalPlayers = totalPlayers = GameObject.FindGameObjectsWithTag("Player").Length;
        Debug.Log("Players: " + totalPlayers);
        bulletSpawner = transform.Find("Sphere (2)").transform.Find("BulletSpawner").GetComponent<BulletSpawner>();

        netPlayerColor.Value = availColors[hostColorIndex];
        ApplyPlayerColor();
        UpdateHealthDisplay();
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
        UpdateHealthDisplay();
    }
    public void OnPlayerKillsChanged(int previous, int current)
    {
        UpdateKillsDisplay();
    }
    private void UpdateHealthDisplay()
    {
        if (IsOwner)
        {
            Debug.Log($"My health = {netPlayerScore3.Value}");
            transform.Find("Score").GetComponent<TextMeshPro>().text = "Health: " + netPlayerScore3.Value.ToString();
        }
    }

    private void UpdateKillsDisplay()
    {
        if (IsOwner)
        {
            Debug.Log($"My kills = {netPlayerKills.Value}");
            transform.Find("Kills").GetComponent<TextMeshPro>().text = "Kills: " + netPlayerKills.Value.ToString();
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

            if (totalPlayers == 0)
            {
                NetworkManager.SceneManager.LoadScene("GameOver", UnityEngine.SceneManagement.LoadSceneMode.Single);
                totalPlayers = -1;
                
            }
        }

    }

    void UpdateOwner()
    {
        Vector3 moveBy = CalcMovementFromInput(Time.deltaTime);
        Vector3 rotateBy = CalcRotationFromInput(-Time.deltaTime);
        RequestPositionForMovementServerRpc(moveBy, rotateBy);
        if (Input.GetButtonDown("Fire2"))
        {
            RequestNextColorServerRpc();
        }
        if (Input.GetButtonDown("Fire1"))
        {
            bulletSpawner.FireServerRpc(netPlayerShotSpeed.Value, netPlayerShotSize.Value);
            Debug.Log("I am owned by: " + NetworkManager.Singleton.LocalClientId);
        }

    }

    private void HostHandleBulletCollision(GameObject bullet)
    {
        Bullet bulletScript = (Bullet)bullet.GetComponent("Bullet");
        
        ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
        Player otherPlayer =
            NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();
        Destroy(bullet);
        netPlayerScore3.Value -= otherPlayer.netPlayerDamage.Value;

        if (netPlayerScore3.Value <= 0)
        {
            Debug.Log("I got a kill");
            otherPlayer.netPlayerKills.Value++;
            otherPlayer.totalPlayers--;
            Debug.Log("Players: " + totalPlayers);
            Destroy(gameObject);
        }
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

    [ServerRpc]
    public void RequestPositionForMovementServerRpc(Vector3 posChange, Vector3 rotChange, ServerRpcParams serverRpcParams = default)
    {
        transform.Translate(posChange);
        transform.Rotate(rotChange);
    }
}
