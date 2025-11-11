using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ChatScript : NetworkBehaviour
    {
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private TMP_Text chatScrollView;
        [SerializeField] private ScrollRect scrollRect;

        public void Send()
        {
            // Do not send anything if blank
            if (string.IsNullOrWhiteSpace(chatInput.text))
            {
                return;
            }

            if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
            {
                Debug.LogError("NetworkManager is not running. Cannot send chat message.");
                return;
            }

            SendChatMessageRpc(chatInput.text);
            CleanInputAndRefocus();
        }

        private void CleanInputAndRefocus()
        {
            chatInput.text = "";
            chatInput.ActivateInputField();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SendChatMessageRpc(string message, RpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            BroadcastChatMessageRpc($"<b>Player {senderId}</b>: {message}");
        }

        [Rpc(SendTo.Everyone)]
        private void BroadcastChatMessageRpc(string message, RpcParams rpcParams = default)
        {
            chatScrollView.text += message + "\n";
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}