using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatServer : NetworkBehaviour
{
    


    [ClientRpc]
    public void SendChatMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log(message);
    }

    [ServerRpc]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"Host got message: {message}");
        string newMessage = $"Player #{serverRpcParams.Receive.SenderClientId}: {message}";
        SendChatMessageClientRpc(newMessage);
    }
    [ServerRpc]
    public void RequestSendMessageServerRpc(string msg)
    {
        SendChatMessageServerRpc(msg);
    }
    [ServerRpc]
    public void SendSystemMessageServerRpc(string msg, ulong clientId)
    {
        It4080.Chat chat = new It4080.Chat();
        chat.SystemMessage(msg);
    }
}
