using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    public static Hud Instance;
    public void Awake()
    {
        Instance = this;
    }

    [SerializeField]GameObject death;
    [SerializeField] GameObject blueTeamWon;
    [SerializeField] GameObject redTeamWon;
    [SerializeField] GameObject gameOn;
    [SerializeField] GameObject winner;
    [SerializeField] GameObject thisyou;
    [SerializeField] Image flash;
    [SerializeField] Gradient flashGraient;
    [SerializeField] TMPro.TMP_Text killFeed;

    float killFeedClear;
    float flashDuration;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(killFeedClear > 0f)
        {
            killFeedClear -= Time.deltaTime;
            killFeed.gameObject.SetActive(true);
        }
        else if(killFeedClear < 0f)
        {
            killFeed.text = "";
            killFeedClear = 0f;
        }
        else
        {
            killFeed.gameObject.SetActive(false);
        }

        if(flashDuration > 0.01f)
        {
            flash.color = flashGraient.Evaluate(flashDuration);
            flash.gameObject.SetActive(true);
            thisyou.SetActive(true);
            flashDuration -= Time.deltaTime * 0.5f;
        }
        else
        {
            flash.gameObject.SetActive(false);
            thisyou.gameObject.SetActive(false);
        }
    }

    public void KillFeed(string killer, string killed, string killerColor, string killedColor)
    {
        //
        killFeed.text = $"<color={killerColor}>{killer}</color> killed <color={killedColor}>{killed}</color> \n" + killFeed.text;
        
        killFeedClear = 5f;
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

    public void ShowDeath(bool show)
    {
        death.SetActive(show);
    }

    public async void Flash()
    {
        flashDuration = 1f;

    }
}
