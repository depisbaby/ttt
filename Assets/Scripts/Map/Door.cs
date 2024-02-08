using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour, IInteractable 
{
    NetworkVariable<bool> open = new NetworkVariable<bool>();

    [SerializeField] GameObject door1;
    [SerializeField] GameObject door2;
    [SerializeField] Transform door1OpenPosition;
    [SerializeField] Transform door2OpenPosition;
    Vector3 door1TargetPosition;
    Vector3 door2TargetPosition;


    void OnMapReset()
    {
        Debug.Log("bruh");
        open.Value = false;
    }

    public void Start()
    {
        open.Value = false;
        Map.Instance.SubscribeToReset(OnMapReset);


    }

    public void Update()
    {
        if(open.Value == true)
        {
            door1.transform.position = Vector3.MoveTowards(door1.transform.position, door1OpenPosition.position, Time.deltaTime * 3f);
            door2.transform.position = Vector3.MoveTowards(door2.transform.position, door2OpenPosition.position, Time.deltaTime * 3f);
        }
        else
        {
            door1.transform.position = Vector3.MoveTowards(door1.transform.position, transform.position, Time.deltaTime * 3f);
            door2.transform.position = Vector3.MoveTowards(door2.transform.position, transform.position, Time.deltaTime * 3f);
        }
        
    }

    [ServerRpc(RequireOwnership = false)] public void PressButtonServerRpc()
    {
        open.Value = !open.Value;

    }


    public void Interact()
    {
        PressButtonServerRpc();
    }

    public string GetInteractMessage()
    {
        return "Operate the door";
    }
}
