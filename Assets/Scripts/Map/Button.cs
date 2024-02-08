using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    [SerializeField] string buttonMessage;
    [SerializeField] GameObject[] connectedDevices;
    List<IButtons> devices = new List<IButtons>();

    public void Start()
    {
        foreach (var _connectedDevices in connectedDevices)
        {
            IButtons i = _connectedDevices.GetComponent<IButtons>();
            if (i != null)
            {
                devices.Add(i);
            }
        }
    }

    public void Interact()
    {
        foreach(var _devices in devices)
        {
            _devices.ButtonPressed(buttonMessage);
        }
    }

    public string GetInteractMessage()
    {
        return "Press the button";
    }
}
