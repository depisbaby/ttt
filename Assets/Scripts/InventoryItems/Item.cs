using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [Header("Item")]
    //Network
    [HideInInspector]public int itemId;
    [HideInInspector] public ushort amount;
    public FixedString128Bytes data;

    //Local
    public string itemName;
    public ItemType itemType;
    public GameObject worldModelPrefab;
    public ViewModel viewModel;
    public Sprite itemSprite;
    public bool stackable;
    public bool canBePocketed;
    public string description;


    public enum ItemType
    {
        Generic = 0,
        Gun = 1,
        

    }

    public virtual void UseLeftClick()
    {

    }

    public virtual void UseRightClick()
    {

    }

    public virtual void Equip()
    {

    }

    public virtual void CustomMessage(string message)
    {

    }

}
