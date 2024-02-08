using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager.UI;
using UnityEngine;

public class FTRoomMenu : MenuWindow
{
    public static FTRoomMenu Instance;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] GameObject window;

    [SerializeField] TMPro.TMP_Text pinTMP;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(window.activeSelf == true)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                MenuManager.Instance.CloseWindow("FTRoomMenu");
            }
        }
    }

    public void NumberPressed(int number)
    {
        FTRoom.Instance.KeyboardNumberPressedServerRpc(number);
    }

    public void SubmitPressed()
    {
        FTRoom.Instance.KeyboardSubmitPressedServerRpc();
    }
    public void ClearPressed()
    {
        FTRoom.Instance.KeyboardClearPressedServerRpc();
    }

    public void UpdateCode(string code)
    {
        pinTMP.text = code;
    }

    #region Overrides
    public override void Open()
    {
        base.Open();
        
        window.SetActive(true);
    }
    public override void Close()
    {
        base.Close();
        
        window.SetActive(false);
    }

    public override bool GetWindowActive()
    {
        return window.activeSelf;
    }
    #endregion
}
