using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance;
    public void Awake()
    {
        Instance = this;
    }

    [SerializeField] List<Transform> blueSpawns = new List<Transform>();
    [SerializeField] List<Transform> redSpawns = new List<Transform>();

    [SerializeField] List<Player> blueAlive = new List<Player>();
    [SerializeField] List<Player> redAlive = new List<Player>();

    bool matchActive;

    public async void StartMatch()
    {
        blueAlive.Clear();
        redAlive.Clear();

        for (int i = 0; i < LobbyMenu.Instance.blueTeam.Count; i++)
        {
            LobbyMenu.Instance.blueTeam[i].health.Value = 100;
            LobbyMenu.Instance.blueTeam[i].ReviveClientRpc();
            LobbyMenu.Instance.blueTeam[i].TeleportAndFreezeClientRpc(blueSpawns[i].position);
            blueAlive.Add(LobbyMenu.Instance.blueTeam[i]);

        }

        for (int i = 0; i < LobbyMenu.Instance.redTeam.Count; i++)
        {
            LobbyMenu.Instance.redTeam[i].health.Value = 100;
            LobbyMenu.Instance.redTeam[i].ReviveClientRpc();
            LobbyMenu.Instance.redTeam[i].TeleportAndFreezeClientRpc(redSpawns[i].position);
            redAlive.Add(LobbyMenu.Instance.redTeam[i]);
        }

        await Task.Delay(1000);
        ShowMessageOnScreenClientRpc("gameOn");

        foreach (Player player in LobbyMenu.Instance.players)
        {
            player.UnfreezeClientRpc();
        }
        matchActive = true;
    }

    public async void MatchEnd()
    {
        matchActive = false;
        await Task.Delay(5000);
        StartMatch();
    }

    public void PlayerKilled(Player whoDied, Player whoKilled)
    {
        if (!matchActive) return;

        string killerColor;
        string killedColor;

        if(LobbyMenu.Instance.blueTeam.Contains(whoKilled)) 
        {
            killerColor = "blue";
        }
        else
        {
            killerColor = "red";
        }

        if(blueAlive.Contains(whoDied))
        {
            killedColor = "blue";
            blueAlive.Remove(whoDied);
        }
        else
        {
            killedColor="red";
            redAlive.Remove(whoDied);
        }

        KillFeedClientRpc(whoKilled.username.Value, whoDied.username.Value, killerColor, killedColor);

        if(blueAlive.Count == 0)
        {
            ShowMessageOnScreenClientRpc("redWon");
            MatchEnd();
        }
        else if(redAlive.Count == 0)
        {
            ShowMessageOnScreenClientRpc("blueWon");
            MatchEnd();
        }

    }

    [ClientRpc] void ShowMessageOnScreenClientRpc(FixedString128Bytes message)
    {
        string parsed = message.ToString();
        switch (parsed) {
            case "redWon":
                Hud.Instance.ShowRedTeamWon();
                break;

            case "blueWon":
                Hud.Instance.ShowBlueTeamWon();
                break;

            case "gameOn":
                Hud.Instance.ShowGameOn();
                break;

            default: break;
        }
    }

    [ClientRpc] void KillFeedClientRpc(FixedString128Bytes killer, FixedString128Bytes killed, FixedString128Bytes killerColor, FixedString128Bytes killedColor)
    {
        Hud.Instance.KillFeed(killer.ToString(), killed.ToString(), killerColor.ToString(), killedColor.ToString());
    }

}
