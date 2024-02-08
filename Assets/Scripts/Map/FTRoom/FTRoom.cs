using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

public class FTRoom : NetworkBehaviour, IButtons
{
    public static FTRoom Instance;

    private void Awake()
    {
        Instance = this;
    }


    [SerializeField] GameObject Go_innerBridge;
    [SerializeField] GameObject Go_outerBridge;

    [SerializeField] Transform Tr_innerOpen;
    [SerializeField] Transform Tr_outerOpen;

    bool open;

    bool rightPressed;
    bool leftPressed;

    string currentCode;

    // Start is called before the first frame update
    void Start()
    {
        currentCode = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (!open) return;

        Go_innerBridge.transform.position = Vector3.MoveTowards(Go_innerBridge.transform.position, Tr_innerOpen.position, Time.deltaTime * 2f);
        Go_outerBridge.transform.position = Vector3.MoveTowards(Go_outerBridge.transform.position, Tr_outerOpen.position, Time.deltaTime * 1f);
    }
    public void OpenBridge()
    {
        if (open) return;
        open = true;

    }

    #region Keyboard

    [ServerRpc(RequireOwnership = false)] public void KeyboardNumberPressedServerRpc(int number)
    {
        if (currentCode.Length == 4) return;

        currentCode += number.ToString();

        UpdateCodeClientRpc(currentCode);

    }

    [ServerRpc(RequireOwnership = false)]
    public void KeyboardClearPressedServerRpc()
    {
        if (currentCode.Length == 0) return;

        currentCode = "";

        UpdateCodeClientRpc(currentCode);

    }

    [ServerRpc(RequireOwnership = false)] public void KeyboardSubmitPressedServerRpc()
    {
        if (currentCode.Length != 4) return;

        MatchManager.Instance.SubmitFTCode(currentCode);
    }

    [ClientRpc] public void UpdateCodeClientRpc(FixedString128Bytes code)
    {
        FTRoomMenu.Instance.UpdateCode(code.ToString());
    }


    #endregion

    #region Bridge buttons
    [ServerRpc(RequireOwnership =false)] public void RightButtonPressedServerRpc()
    {
        RightPressed();
    }

    [ServerRpc(RequireOwnership = false)] public void LeftButtonPressedServerRpc()
    {
        LeftPressed();
    }

    async void RightPressed()
    {
        if (leftPressed)
        {
            OpenBridge();
        }

        rightPressed = true;

        await Task.Delay(5000);

        leftPressed = false;
    }

    async void LeftPressed()
    {
        if (rightPressed)
        {
            OpenBridge();
        }

        leftPressed = true;

        await Task.Delay(5000);

        leftPressed = false;
    }


    public void ButtonPressed(string message)
    {
        if(message == "keyboard")
        {
            MenuManager.Instance.OpenWindow("FTRoomMenu", true);
            return;
        }

        if (message == "right")
        {
            RightButtonPressedServerRpc();
        }
        else
        {
            LeftButtonPressedServerRpc();
        }
    }
    #endregion
}
