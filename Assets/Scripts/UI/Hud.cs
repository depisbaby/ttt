using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Hud : NetworkBehaviour
{
    public static Hud Instance;
    public void Awake()
    {
        Instance = this;
    }
    [SerializeField] GameObject crossHair;
    [SerializeField] GameObject death;
    [SerializeField] GameObject blueTeamWon;
    [SerializeField] GameObject redTeamWon;
    [SerializeField] GameObject gameOn;
    [SerializeField] GameObject raiseWeaponIcon;
    [SerializeField] GameObject winner;
    [SerializeField] Image flash;
    [SerializeField] Gradient flashGraient;
    [SerializeField] TMPro.TMP_Text teamTMP;

    float flashDuration;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(flashDuration > 0.01f)
        {
            flash.color = flashGraient.Evaluate(flashDuration);
            flash.gameObject.SetActive(true);
            
            flashDuration -= Time.deltaTime * 0.5f;
        }
        else
        {
            flash.gameObject.SetActive(false);
            
        }
    }

    public void ShowRaiseWeapon()
    {
        raiseWeaponIcon.SetActive(true);

        crossHair.SetActive(false);
    }

    public void ShowNoIcon()
    {
        raiseWeaponIcon.SetActive(false);

        crossHair.SetActive(true);
    }

    public void ShowGameOn()
    {
        if (gameOn.activeSelf)
        {
            gameOn.SetActive(false);
            return;
        }
        gameOn.SetActive(true);

        Invoke("ShowGameOn", 2f);
    }

    public void ShowBlueTeamWon()
    {
        if(blueTeamWon.activeSelf)
        {
            blueTeamWon.SetActive(false);
            winner.SetActive(false);
            return;
        }
        blueTeamWon.SetActive(true);
        winner.SetActive(true);

        Invoke("ShowBlueTeamWon", 5f);
    }

    public void ShowRedTeamWon()
    {
        if(redTeamWon.activeSelf)
        {
            redTeamWon.SetActive(false);
            winner.SetActive(false);
            return;
        }
        redTeamWon.SetActive(true);
        winner.SetActive(true);

        Invoke("ShowRedTeamWon", 5f);
    }

    [ClientRpc]public void SetTeamClientRpc(ulong clientId, bool blue, FixedString128Bytes ftCode)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return; 

        if (blue)
        {
            teamTMP.text = "You are <color=blue>a human</color>.";

            if (ftCode != "")
            {
                teamTMP.text += "\nYou also know that the correct FT-Code is: " + ftCode;
            }
        }
        else
        {
            teamTMP.text = "You are <color=red>a mimic</color>.";
        }
    }

    public void ShowDeath(bool show)
    {
        death.SetActive(show);
    }

    public void Flash()
    {
        flashDuration = 1f;

    }
}
