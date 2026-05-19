using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

public class MultiplayerLobbyManager : MonoBehaviour
{
    public Lobby currentLobby;
    [Tooltip("To keep lobby alive")]
    [SerializeField] private float heartBeatTimerLimit = 20f;
    private float heartBeatTimer;
    [SerializeField] private float lobbyPollTimerLimit = 1.1f;
    [SerializeField] private string gameplaySceneName = "PlayerController";
    private float lobbyPollTimer;
    private bool isPolling = false;

    // EVENTS
    public static Action OnLobbyCreated;
    public static Action OnLobbyJoined;
    public static Action<Lobby> OnLobbyStateChanged;
    public static Action OnLobbyLeft;

    public static MultiplayerLobbyManager Instance;

    #region MONOBEHAVIOURS
    private async void Awake()
    {
        Instance = this;
        await InitializeAsync();
    }

    private async void Update()
    {
        HandleHeartbeat();
        if (!isPolling)
        {
            isPolling = true;
            await HandleLobbyPolling();
            isPolling = false;
        }
    }
    #endregion

    #region INITIALIZATION
    public async Task InitializeAsync()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        DebugLog($"Signed in as: {AuthenticationService.Instance.PlayerId}");
    }
    #endregion

    #region POLLS
    private void HandleHeartbeat()
    {
        if (currentLobby != null && AuthenticationService.Instance.PlayerId == currentLobby.HostId)
        {
            heartBeatTimer += Time.deltaTime;

            if (heartBeatTimer >= heartBeatTimerLimit)
            {
                heartBeatTimer = 0f;
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
    }
    private async Task HandleLobbyPolling()
    {
        if (currentLobby == null) return;
        lobbyPollTimer += Time.deltaTime;

        if (lobbyPollTimer >= lobbyPollTimerLimit)
        {
            lobbyPollTimer = 0f;

            try
            {
                Lobby newLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                currentLobby = newLobby;

                bool stillInLobby = false;

                foreach (var player in currentLobby.Players)
                {
                    if (player.Id == AuthenticationService.Instance.PlayerId)
                    {
                        stillInLobby = true;
                        break;
                    }
                }

                if (!stillInLobby)
                {
                    DebugLog("You were removed from the lobby.");
                    LeaveLobbyLocally();
                    return;
                }

                OnLobbyStateChanged?.Invoke(currentLobby);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    DebugLog("Lobby was deleted.");
                    currentLobby = null;
                    // OnLobbyLeft?.Invoke();
                }
                else
                {
                    DebugLog("Polling error: " + e.Message);
                }
            }
        }
    }

    private void LeaveLobbyLocally()
    {
        currentLobby = null;

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        OnLobbyLeft?.Invoke();
    }

    #endregion

    #region API

    public async Task CreateLobby()
    {
        var options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Player = new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    {"Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "Host")}
                })
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync("Test-Lobby", 4, options);

        DebugLog($"Lobby created. Code: {currentLobby.LobbyCode}");

        await CreateRelayAndStartHost();
    }


    public async Task JoinLobbyByCode(string lobbyCode)
    {
        var options = new JoinLobbyByCodeOptions
        {
            Player = new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    {
                        "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "Client")
                    }
                }
            )
        };
        currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
        string relayCode = currentLobby.Data["RelayCode"].Value;
        await JoinRelayAndStartClient(relayCode);
    }

    public async Task LeaveLobby()
    {
        if (currentLobby == null) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            DebugLog("Error leaving lobby: " + e.Message);
        }
        finally
        {
            LeaveLobbyLocally();
        }
    }

    public async Task KickPlayer(string playerId)
    {
        if (AuthenticationService.Instance.PlayerId != currentLobby.HostId) return;
        await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }


    /// ---------- RELAY ------------
    public async Task CreateRelayAndStartHost()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var data = new Dictionary<string, DataObject>
        {
            {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode)}
        };

        await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
        {
            Data = data
        });

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        NetworkManager.Singleton.StartHost();

        OnLobbyCreated?.Invoke();
        DebugLog("Host started with Relay");
    }

    public async Task JoinRelayAndStartClient(string relayCode)
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

        NetworkManager.Singleton.StartClient();

        OnLobbyJoined?.Invoke();
        DebugLog("Client connected via Relay");
    }

    #endregion

    #region UTILS
    public static void DebugLog(string msg)
    {
        Debug.Log($"[Multiplayer] {msg}");
    }
    #endregion
}
