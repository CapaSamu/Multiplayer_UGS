using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatScript : NetworkBehaviour
{
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private TMP_Text chatScrollView;
    [SerializeField] private ScrollRect scrollRect;

    private static ChatScript instance;

    private void Awake()
    {
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && !IsSpawned)
        {
            GetComponent<NetworkObject>().Spawn();
            Debug.Log("[Chat] ChatManager spawned by host.");
        }

        Debug.Log($"[Chat] OnNetworkSpawn - IsServer:{IsServer}, IsClient:{IsClient}, IsSpawned:{IsSpawned}");
    }

    public void Send()
    {
        if (string.IsNullOrWhiteSpace(chatInput.text))
        {
            return;
        }

        if (!IsClient)
        {
            Debug.LogWarning("[Chat] Not a client - cannot send message.");
            return;
        }

        Debug.Log($"[Chat] Sending message: {chatInput.text}");
        SendChatMessageRpc(chatInput.text);
        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SendChatMessageRpc(string message, RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        string formatted = $"<b>Player {senderId}</b>: {message}";

        Debug.Log($"[Chat] Server received from {senderId}: {message}");

        BroadcastChatMessageRpc(formatted);
    }

    [Rpc(SendTo.Everyone)]
    private void BroadcastChatMessageRpc(string message, RpcParams rpcParams = default)
    {
        AppendMessage(message);
    }

    private void AppendMessage(string message)
    {
        chatScrollView.text += message + "\n";

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        Debug.Log($"[Chat] Message appended: {message}");
    }
}
