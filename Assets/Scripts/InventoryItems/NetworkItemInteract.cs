using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkItemInteract : MonoBehaviour, IInteractable
{
    [HideInInspector]public NetworkItem networkItem;
    public string GetInteractMessage()
    {
        return "Pick up item";
    }

    public void Interact()
    {
        NetworkItem networkItem = transform.parent.gameObject.GetComponent<NetworkItem>();

        if (!ItemManager.Instance.items[networkItem.itemId.Value].canBePocketed)
        {
            if (InventoryMenu.Instance.inventoryItemSlots[0].placedItem != null)
            {
                for (int i = 1; i < InventoryMenu.Instance.inventoryItemSlots.Count; i++)
                {
                    
                    if (InventoryMenu.Instance.inventoryItemSlots[i].placedItem == null)
                    {
                        InventoryMenu.Instance.MoveItemToSlot(InventoryMenu.Instance.inventoryItemSlots[0], InventoryMenu.Instance.inventoryItemSlots[i]);
                    }
                }

                if(InventoryMenu.Instance.inventoryItemSlots[0].placedItem != null)
                {
                    return;
                }
            }
        }

        ulong networkId = networkItem.no.NetworkObjectId;

        ItemManager.Instance.AttemptPickingUpServerRpc(networkId, NetworkManager.Singleton.LocalClientId, InventoryMenu.Instance.CheckFit(networkItem.itemId.Value));
    }
    //
}
