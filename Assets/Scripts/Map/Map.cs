using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Map : NetworkBehaviour
{
    public static Map Instance;

    private void Awake()
    {
        Instance = this; 
    }

    #region Reseting map

    public delegate void MapReset();
    public event MapReset OnMapReset;

    public void ResetMap()
    {
        // Call the main method, which will invoke the event
        if (OnMapReset != null)
        {
            OnMapReset.Invoke();
        }
    }
    public void SubscribeToReset(MapReset method)
    {
        // Subscribe a method to the event
        OnMapReset += method;
    }

    #endregion


}
