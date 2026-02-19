using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Player = Unity.Services.Lobbies.Models.Player;

public class LobbyUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField joinCodeInput;

    [Space]
    [Header("Screens")]
    public GameObject CTA;
    public GameObject joinScreen;
    public GameObject lobbyScreen;

    [Space]
    [SerializeField] private TextMeshProUGUI codeTxt;
    [SerializeField] private Transform playersContent;
    [SerializeField] private GameObject playerEntry;

    private Dictionary<string, GameObject> playerEntries = new();
    MultiplayerLobbyManager lobbyManager => MultiplayerLobbyManager.Instance;
    public void HostButtonCallback()
    {
        lobbyManager.CreateLobby();
    }
    public void JoinButtonCallback()
    {
        if (string.IsNullOrEmpty(joinCodeInput.text)) return;
        lobbyManager.JoinLobbyByCode(joinCodeInput.text);
    }

    public void CopyLobbyCode()
    {
        if (string.IsNullOrEmpty(codeTxt.text)) return;
        GUIUtility.systemCopyBuffer = codeTxt.text;
        MultiplayerLobbyManager.DebugLog($"Code: {codeTxt.text} copied to clipboard!");
    }

    void OnEnable()
    {
        MultiplayerLobbyManager.OnLobbyCreated += PrepareLobby;
        MultiplayerLobbyManager.OnLobbyJoined += PrepareLobby;
        MultiplayerLobbyManager.OnLobbyStateChanged += RefreshLobbyUI;
    }

    void OnDisable()
    {
        MultiplayerLobbyManager.OnLobbyCreated -= PrepareLobby;
        MultiplayerLobbyManager.OnLobbyJoined -= PrepareLobby;
        MultiplayerLobbyManager.OnLobbyStateChanged -= RefreshLobbyUI;
    }


    #region EVENT_HANDLERS

    private void PrepareLobby()
    {
        CTA.SetActive(false);
        joinScreen.SetActive(false);
        lobbyScreen.SetActive(true);
        var lobby = lobbyManager.currentLobby;
        codeTxt.text = lobby.LobbyCode;
        RefreshLobbyUI(lobby);
    }


    private void RefreshLobbyUI(Lobby lobby)
    {
        if (lobby.Players.Count == playerEntries.Count) return;
        Debug.Log($"Refreshing lobby: players = {lobby.Players.Count}");
        foreach (var entry in playerEntries)
            Destroy(entry.Value);
        playerEntries.Clear();

        foreach (var player in lobby.Players)
        {
            string playerName = player.Data["Name"].Value;
            Debug.Log($"Player = {playerName}");
            bool isHost = player.Id == lobby.HostId;
            bool isLocalHost = AuthenticationService.Instance.PlayerId == lobby.HostId;

            SpawnPlayerEntry(playerName, player.Id, isHost, isLocalHost && !isHost);
        }
    }

    #endregion

    private void SpawnPlayerEntry(string pName, string pID, bool host, bool spawningFromHost = false)
    {
        PlayerEntryRefs entry = Instantiate(playerEntry, playersContent, false).GetComponent<PlayerEntryRefs>();
        entry.playerName.text = pName;
        entry.hostTag.SetActive(host);
        entry.kickButton.SetActive(spawningFromHost);
        entry.playerId = pID;
        playerEntries.Add(pID, entry.gameObject);
    }

    private void RemovePlayerEntry(string pName)
    {
        if (!playerEntries.ContainsKey(pName)) return;
        Destroy(playerEntries[pName]);
        playerEntries.Remove(pName);
    }
}