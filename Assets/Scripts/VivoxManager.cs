using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Unity.Netcode;
using System.Threading.Tasks;

public class VivoxManager : MonoBehaviour
{
    [SerializeField] string channelName = "DefaultVoiceRoom";
    [SerializeField] bool useNetcodeClientIdAsUser = true;

    private string userName;

    async void Start()
    {
        await InitializeVivoxAsync();
    }

    async Task InitializeVivoxAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await VivoxService.Instance.InitializeAsync();

            // Decide username: Netcode clientId (Host uses 0) or random fallback
            if (useNetcodeClientIdAsUser && NetworkManager.Singleton != null)
            {
                // If Netcode not started yet, fallback to random and try to update after connect
                userName = "Player_" + (NetworkManager.Singleton.IsListening ? NetworkManager.Singleton.LocalClientId.ToString() : Random.Range(1000, 9999).ToString());
            }
            else
            {
                userName = "User_" + Random.Range(1000, 9999);
            }

            // Build LoginOptions (recommended to use LoginOptions for server-signed tokens in prod)
            var loginOptions = new LoginOptions
            {
                // DisplayName is optional
                DisplayName = userName
            };

            await VivoxService.Instance.LoginAsync(loginOptions);
            Debug.Log($"Vivox: logged in as {userName}");

            // Subscribe to channel joined event so we know when audio is actually connected.
            VivoxService.Instance.ChannelJoined += OnChannelJoined;
            VivoxService.Instance.ChannelLeft += OnChannelLeft;

            // Join a group (non-positional) channel. Use JoinPositionalChannelAsync for 3D audio.
            await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);
            Debug.Log($"Vivox: requested join to channel {channelName} (await only sends request).");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Vivox initialization/join failed: {e}");
        }
    }

    // Fired when the channel media is actually connected and ready.
    void OnChannelJoined(string joinedChannelName)
    {
        if (joinedChannelName == channelName)
        {
            Debug.Log($"Vivox: Channel media READY -> {joinedChannelName}");
            // Aquí puedes activar UI, indicadores de voz, etc.
        }
    }

    void OnChannelLeft(string leftChannelName)
    {
        if (leftChannelName == channelName)
        {
            Debug.Log($"Vivox: Left channel {leftChannelName}");
        }
    }

    private async void OnApplicationQuit()
    {
        try
        {
            // Unsubscribe
            if (VivoxService.Instance != null)
            {
                VivoxService.Instance.ChannelJoined -= OnChannelJoined;
                VivoxService.Instance.ChannelLeft -= OnChannelLeft;

                await VivoxService.Instance.LeaveAllChannelsAsync();
                await VivoxService.Instance.LogoutAsync();
                Debug.Log("Vivox: logged out cleanly.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Vivox quit cleanup error: {e.Message}");
        }
    }
}
