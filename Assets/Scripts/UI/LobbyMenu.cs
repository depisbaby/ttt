using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
//using static UnityEditor.Progress;

public class LobbyMenu : NetworkBehaviour
{
    public static LobbyMenu Instance;
    public void Awake()
    {
        Instance = this;
    }

    public Dictionary<ulong, Player> playersDict = new Dictionary<ulong, Player>();
    public List<Player> players = new List<Player>();
    public List<Player> ready = new List<Player>();

    public List<Player> redTeam = new List<Player>();
    public List<Player> blueTeam = new List<Player>();

    [Header("UI Elements")]
    public TMPro.TMP_Text blueTeamTMP;
    public TMPro.TMP_Text redTeamTMP;
    public TMPro.TMP_Text readyButtonTMP;

    public GameObject window;

    bool readyLocalBool;
    // Start is called before the first frame update
    void Start()
    {
        readyLocalBool = false;
        readyButtonTMP.text = "Not ready";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ReadyPressed()
    {
        if (readyLocalBool)
        {
            PlayerReadyServerRpc(false, NetworkManager.Singleton.LocalClientId);
            readyLocalBool = false;
            readyButtonTMP.text = "Not ready"; 
        }
        else
        {
            PlayerReadyServerRpc(true, NetworkManager.Singleton.LocalClientId);
            readyLocalBool = true;
            readyButtonTMP.text = "Ready";
        }
    }
    public void OnPlayerJoined(Player player)
    {
        if(players.Count == 0 || players.Count % 2 == 0) //put on blue
        {
            blueTeam.Add(player);
            player.SetColorClientRpc(true);
        }
        else //put on red
        {
            redTeam.Add(player);
            player.SetColorClientRpc(false);
        }
        
        players.Add(player);
        playersDict.Add(player.OwnerClientId, player);


        UpdateLobby();
    }

    public void OnPlayerLeft(Player player)
    {
        if (redTeam.Contains(player)) // on red
        {
            redTeam.Remove(player);
        }
        else // on blue
        {
            blueTeam.Remove(player);
        }

        players.Remove(player);
        playersDict.Remove(player.OwnerClientId);

        UpdateLobby();
    }

    public void UpdateLobby()
    {
        string s = "";
        foreach (var item in blueTeam)
        {
            s = s + item.username.Value + "\n";
            item.SetColorClientRpc(true);
        }
        blueTeamTMP.text = s;
        UpdateLobbyUserClientRpc(true, s);
        s = "";

        foreach (var item in redTeam)
        {
            s = s + item.username.Value + "\n";
            item.SetColorClientRpc(false);
        }
        redTeamTMP.text = s;
        UpdateLobbyUserClientRpc(false, s);
    }
    //
    [ServerRpc(RequireOwnership = false)] void PlayerReadyServerRpc(bool _ready, ulong clientID)
    {
        if (_ready)
        {
            ready.Add(playersDict[clientID]);
        }
        else
        {
            ready.Remove(playersDict[clientID]);
        }

        //if (players.Count < 2) return;

        if(ready.Count == players.Count)
        {
            //Start game
            MatchManager.Instance.StartMatch();
            window.SetActive(false);
            HideLobbyClientRpc(true);
        }
    }

    [ClientRpc] void UpdateLobbyUserClientRpc(bool blueTeam, string display)
    {
        if(blueTeam)
        {
            blueTeamTMP.text = display;
        }
        else
        {
            redTeamTMP.text = display;
        }
    }

    [ClientRpc]
    void HideLobbyClientRpc(bool hide)
    {
        window.SetActive(!hide);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
