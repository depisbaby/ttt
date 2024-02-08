using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public bool actionBlockingWindowOpen;

    [SerializeField] Canvas canvas;
    public List<MenuWindow> menus = new List<MenuWindow>();

    public LobbyMenu lobbyMenu;

    /// <summary>
    /// Call this to open a menu of given name. The name is same as the name of GameObject, the MenuWindow is on.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="closeOthers"></param>
    public void OpenWindow(string name, bool closeOthers)
    {
        if (!closeOthers)
        {
            foreach (var item in menus)
            {
                if (item.gameObject.name == name)
                {
                    item.Open();
                }
            }

            CheckForActionBlocking();
            CheckForWindowsOpen();
            return;
        }

        foreach(var item in menus)
        {
            if (item.gameObject.name == name)
            {
                item.Open();
            }
            else
            {
                item.Close();
            }
        }

        CheckForActionBlocking();
        CheckForWindowsOpen();
    }

    public void CloseWindow(string name)
    {
        foreach (var item in menus)
        {
            if (item.gameObject.name == name)
            {
                item.Close();
            }
        }

        CheckForActionBlocking();
        CheckForWindowsOpen();
    }

    /// <summary>
    /// Call this to close all windows
    /// </summary>
    /// <param name="name"></param>
    /// <param name="closeOthers"></param>
    public void CloseAll()
    {
        foreach (var item in menus)
        {
            item.Close();

        }

        CheckForActionBlocking();
        CheckForWindowsOpen();
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            MenuWindow w = gameObject.transform.GetChild(i).gameObject.GetComponent<MenuWindow>();
            if (w == null) continue;
            menus.Add(gameObject.transform.GetChild(i).gameObject.GetComponent<MenuWindow>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CheckForActionBlocking()
    {
        actionBlockingWindowOpen = false;
        foreach (var item in menus)
        {
            if (item.actionBlocking == true && item.GetWindowActive() == true) actionBlockingWindowOpen = true;
        }
    }

    void CheckForWindowsOpen()
    {
        if (lobbyMenu.window.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            return;
        }

        bool open = false;
        foreach (var item in menus)
        {
            if (item.GetWindowActive() == true)
            {
                open = true;
            }
        }

        if (open)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

}
