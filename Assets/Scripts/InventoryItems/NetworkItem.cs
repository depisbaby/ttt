using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;

public class NetworkItem : NetworkBehaviour
{
    public NetworkVariable<int> itemId = new NetworkVariable<int>();
    public NetworkVariable<ushort> amount = new NetworkVariable<ushort>();
    public NetworkVariable<FixedString128Bytes> data = new NetworkVariable<FixedString128Bytes>();

    [Header("PII")]
    public GameObject worldModelPrefab;
    public NetworkObject no;

    GameObject displayedWorldModel;

    [ClientRpc]public void UpdateItemClientRpc(int itemId)
    {
        UpdateItem(itemId);
    }

    public void UpdateItem(int itemId)
    {
        displayedWorldModel = Instantiate(ItemManager.Instance.items[itemId].worldModelPrefab);
        displayedWorldModel.transform.parent = transform;
        displayedWorldModel.transform.localPosition = Vector3.zero;
        displayedWorldModel.GetComponent<NetworkItemInteract>().networkItem = this;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
    }

    
    
}
