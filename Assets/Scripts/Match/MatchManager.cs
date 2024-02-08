using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance;
    public void Awake()
    {
        Instance = this;
    }
    public string ftCode;

    [SerializeField] int mimicPerAmountOfPlayers;
    [SerializeField] List<Transform> spawnPoints = new List<Transform>();

    [SerializeField] List<Player> blueAlive = new List<Player>();
    [SerializeField] List<Player> redAlive = new List<Player>();

    [SerializeField] Map map;

    bool matchActive;

    public async void StartMatch()
    {
        map.ResetMap();

        ftCode = GetFTCode();

        blueAlive.Clear();
        redAlive.Clear();

        List<Player> randomizedPlayers = ShuffledList(LobbyMenu.Instance.players);

        for (int i = 0; i < LobbyMenu.Instance.players.Count; i++)
        {
            //TODO Randomize spawns
            LobbyMenu.Instance.players[i].health.Value = 100;
            LobbyMenu.Instance.players[i].ReviveClientRpc();
            LobbyMenu.Instance.players[i].TeleportAndFreezeClientRpc(spawnPoints[i].position);
        }

        randomizedPlayers.Clear();
        randomizedPlayers = ShuffledList(LobbyMenu.Instance.players);

        int numberOfMimics = randomizedPlayers.Count / mimicPerAmountOfPlayers + 1;
        Console.Instance.ShowMessageInConsole("MatchManager", "Number of mimics in match: " + numberOfMimics);

        for (int i = 0; i < randomizedPlayers.Count; i++)
        {
            if (i < numberOfMimics)
            {
                Hud.Instance.SetTeamClientRpc(randomizedPlayers[i].OwnerClientId, false, "");
                redAlive.Add(randomizedPlayers[i]);
                continue;
            }

            if (i == randomizedPlayers.Count - 1)
            {
                Hud.Instance.SetTeamClientRpc(randomizedPlayers[i].OwnerClientId, true, ftCode);
            }
            else
            {
                Hud.Instance.SetTeamClientRpc(randomizedPlayers[i].OwnerClientId, true, "");
            }

            blueAlive.Add(randomizedPlayers[i]);
        }

        await Task.Delay(1000);
        ShowMessageOnScreenClientRpc("gameOn");

        foreach (Player player in LobbyMenu.Instance.players)
        {
            player.FreezeClientRpc(false);
        }

        matchActive = true;
    }

    public async void MatchEnd()
    {
        matchActive = false;
        await Task.Delay(5000);
        StartMatch();
    }

    #region Match events
    public void PlayerKilled(Player whoDied, Player whoKilled)
    {
        if (!matchActive) return;

        if (blueAlive.Contains(whoDied))
        {
            blueAlive.Remove(whoDied);
        }
        else
        {
            redAlive.Remove(whoDied);
        }

        if (blueAlive.Count == 0)
        {
            ShowMessageOnScreenClientRpc("redWon");
            MatchEnd();
        }
        else if (redAlive.Count == 0)
        {
            ShowMessageOnScreenClientRpc("blueWon");
            MatchEnd();
        }

    }
    #endregion

    #region HUD messages
    [ClientRpc]
    void ShowMessageOnScreenClientRpc(FixedString128Bytes message)
    {

        MenuManager.Instance.CloseAll();

        string parsed = message.ToString();
        switch (parsed)
        {
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
    #endregion

    #region FTRoom
    string GetFTCode()
    {
        string[] numbers = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        string res = "";
        for (int i = 0; i < 4; i++)
        {
            res += numbers[UnityEngine.Random.Range(0, numbers.Length)];
        }
        return res;
    }

    public void SubmitFTCode(string code)
    {
        if (!matchActive) return;

        if (code != ftCode) //Red team wins
        {
            ShowMessageOnScreenClientRpc("redWon");
            MatchEnd();
        }
        else //blue team wins
        {
            ShowMessageOnScreenClientRpc("blueWon");
            MatchEnd();
        }
    }

    #endregion

    #region Utility
    public List<Player> ShuffledList(List<Player> original)
    {
        List<Player> list = new List<Player>();
        List<Player> res = new List<Player>();

        foreach (Player player in original)
        {
            list.Add(player);
        }

        while (list.Count > 0)
        {
            int rng = Random.Range(0, list.Count);
            res.Add(list[rng]);
            list.RemoveAt(rng);
        }

        return res;
    }
    #endregion
}
