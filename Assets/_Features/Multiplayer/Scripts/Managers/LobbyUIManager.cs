using TMPro;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField joinCodeInput;
    
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
}