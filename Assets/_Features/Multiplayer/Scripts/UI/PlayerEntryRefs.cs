using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntryRefs : MonoBehaviour
{
    public Image profilePic;
    public TextMeshProUGUI playerName;
    public GameObject hostTag;
    public GameObject kickButton;

    public string playerId { private get; set; }


    public void KickPlayer()
    {
        MultiplayerLobbyManager.Instance.KickPlayer(playerId);
    }
}