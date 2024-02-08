using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//
public class InventoryMenu : MenuWindow, IItemSlot
{
    public static InventoryMenu Instance;
    private void Awake()
    {
        Instance = this;
    }

    enum InventoryAction
    {
        None = 0,
        GrabbedAll = 1,
        GrabbedHalf = 2,
    }

    [SerializeField] GameObject window;

    [Header("Inventory menu")]

    public List<ItemSlot> inventoryItemSlots = new List<ItemSlot>();
    

    [SerializeField]
    public ItemSlot hoveredSlot;
    public ItemSlot activeSlot;
    public TMPro.TMP_Text descriptionTMP;
    public Image descriptionPic;

    InventoryAction currentAction;


    int selectedHotbarSlotIndex;

    // Start is called before the first frame update
    void Start()
    {

        foreach (var slot in inventoryItemSlots)
        {
            slot._interface = this;
        }

        MenuManager.Instance.CloseWindow("InventoryMenu");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            if (window.activeSelf)
            {
                MenuManager.Instance.CloseWindow("InventoryMenu");
            }
            else
            {
                MenuManager.Instance.OpenWindow("InventoryMenu", true);
            }
        }

        if (!window.activeSelf)
        {
            HotkeysInputs();

            return;
        }

        if (Input.GetButtonDown("Fire2") && Input.GetKey(KeyCode.LeftShift))
        {
            if (hoveredSlot == null) return;

            DropItem(hoveredSlot);

        }
        else if (Input.GetButtonUp("Fire1")) 
        {
            if(activeSlot == null && hoveredSlot != null) //Grab all item
            {
                if (hoveredSlot.placedItem == null) return;

                activeSlot = hoveredSlot;
                currentAction = InventoryAction.GrabbedAll;
                CursorManager.Instance.SetCursorSprite(activeSlot.placedItem.itemSprite, 0.5f);
            }
            else if(activeSlot != null && hoveredSlot != null) //Place item
            {
                if(currentAction == InventoryAction.GrabbedAll)
                {
                    MoveItemToSlot(activeSlot, hoveredSlot);
                }
                else if (currentAction == InventoryAction.GrabbedHalf)
                {
                    MoveHalfToSlot(activeSlot, hoveredSlot);
                }

                CursorManager.Instance.SetCursorSprite(null, 0.5f);
                activeSlot = null;
                currentAction = InventoryAction.None;
            }
            else
            {
                CursorManager.Instance.SetCursorSprite(null, 0.5f);
                activeSlot = null;
                currentAction = InventoryAction.None;

            }

            
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            if (activeSlot == null && hoveredSlot != null) //Grab half item
            {
                if (hoveredSlot.placedItem == null) return;

                activeSlot = hoveredSlot;
                currentAction = InventoryAction.GrabbedHalf;
                CursorManager.Instance.SetCursorSprite(activeSlot.placedItem.itemSprite, 0.5f);
            }
            else if (activeSlot != null && hoveredSlot != null && currentAction == InventoryAction.GrabbedAll)
            {

            }
            else
            {

            }
        }
    }

    #region Overrides
    public override void Open()
    {
        base.Open();
        UpdateInventory();
        window.SetActive(true);
    }
    public override void Close()
    {
        base.Close();
        activeSlot = null;
        CursorManager.Instance.SetCursorSprite(null, 0.5f);
        window.SetActive(false);
    }

    public override bool GetWindowActive()
    {
        return window.activeSelf;
    }

    #endregion

    #region Public
    public void PutItemToPlayerInventory(Item item)
    {
        //Debug.Log("afgasdgasdg");
        item.gameObject.transform.parent = ItemManager.Instance.transform;

        foreach (var slot in inventoryItemSlots)
        {
            if (slot.placedItem == null)//Find a empty slot
            {
                slot.placedItem = item;
                UpdateInventory();
                return;
            }

            if (item.stackable && slot.placedItem.itemId == item.itemId)//Find a slot with same item and excess space
            {
                ushort _free = 100;
                _free -= slot.placedItem.amount;

                if (item.amount <= _free)
                {
                    slot.placedItem.amount += item.amount;
                    item.amount = 0;
                }
                else
                {
                    slot.placedItem.amount += _free;
                    item.amount -= _free;
                }

                if (item.amount == 0)
                {
                    Destroy(item.gameObject);
                    UpdateInventory();
                    return;
                }

            }

        }

        //If no room, destroy item/amount to prevent duplication exploits
        Console.Instance.ShowMessageInConsole(gameObject, "Excess local items detected!");
        UpdateInventory();
    }

    public ushort CheckFit(int itemId)
    {
        Item item = ItemManager.Instance.items[itemId];

        ushort res = 0;
        foreach (var slot in inventoryItemSlots)
        {
            if (slot.placedItem == null)//Find a empty slot
            {
                return 100;
            }

            if (item.stackable && slot.placedItem.itemId == item.itemId)//Find a slot with same item and excess space
            {
                ushort free = 100;
                free -= slot.placedItem.amount;
                res += free;
            }

            if (res >= 100)
            {
                return 100;
            }

        }

        return res;

    }

    public void UpdateInventory()
    {
        

        foreach (var item in inventoryItemSlots)
        {
            if (item.placedItem != null)
            {
                item.icon.color = Color.white;
                item.icon.sprite = item.placedItem.itemSprite;
                
                if (item.placedItem.stackable)
                {
                    item.amountTmp.text = item.placedItem.amount.ToString();

                }
                else
                {
                    item.amountTmp.text = "";
                }
            }
            else
            {
                item.icon.color = Color.clear;
                item.amountTmp.text = "";
            }
        }

        if(inventoryItemSlots[0].placedItem != null)
        {
            inventoryItemSlots[0].placedItem.Equip();
        }
        else
        {
            ViewModelManager.Instance.SetViewModel(null);
            Player.localPlayer.playerVisuals.ChangeAppearanceServerRpc("");
            return;
        }

        if (inventoryItemSlots[0].placedItem.viewModel != null)
        {
            ViewModelManager.Instance.SetViewModel(inventoryItemSlots[0].placedItem.viewModel);
            Player.localPlayer.playerVisuals.ChangeAppearanceServerRpc(inventoryItemSlots[0].placedItem.itemName);
        }
        else
        {
            ViewModelManager.Instance.SetViewModel(null);
            Player.localPlayer.playerVisuals.ChangeAppearanceServerRpc("");
        }
    }

    #endregion

    #region Inventory manipulation
    public void MoveItemToSlot(ItemSlot startingSlot, ItemSlot destinationSlot)
    {
        if (startingSlot.placedItem == null) return;

        if (startingSlot == destinationSlot) return;

        if (!startingSlot.placedItem.canBePocketed) return;


        if(startingSlot.placedItem != null && destinationSlot.placedItem != null) //switch positions
        {

            if (!destinationSlot.placedItem.canBePocketed) return;

            if (startingSlot.placedItem.itemId == destinationSlot.placedItem.itemId && startingSlot.placedItem.stackable)
            {
                StackItems(startingSlot, destinationSlot);
                return;
            }

            Item switcher = destinationSlot.placedItem;
            destinationSlot.placedItem = startingSlot.placedItem;
            startingSlot.placedItem = switcher;

            
        }
        else
        {

            destinationSlot.placedItem = startingSlot.placedItem;
            startingSlot.placedItem = null;

        }

        UpdateInventory();

    }

    void StackItems(ItemSlot start, ItemSlot destination)
    {
        int startAmount = start.placedItem.amount;
        int destinationAmount = destination.placedItem.amount;

        if(startAmount + destinationAmount <= 100)
        {
            int x = startAmount + destinationAmount;
            destination.placedItem.amount = (ushort)x;
            Destroy(start.placedItem.gameObject);
            start.placedItem = null;
        }
        else
        {
            destination.placedItem.amount = 100;

            int x = startAmount + destinationAmount - 100;
            start.placedItem.amount = (ushort)x;
        }

        UpdateInventory();
    }

    void MoveHalfToSlot(ItemSlot startingSlot, ItemSlot destinationSlot)
    {
        if (startingSlot.placedItem == null) return;

        if (startingSlot == destinationSlot) return;

        if (destinationSlot.placedItem != null) return;

        if (startingSlot.placedItem.amount <= 1) return;

        int amount = startingSlot.placedItem.amount / 2;
        Item item = ItemManager.Instance.CreateLocalItem(startingSlot.placedItem.itemId, (ushort)amount, "");
        item.gameObject.transform.parent = ItemManager.Instance.transform;

        destinationSlot.placedItem = item;
        startingSlot.placedItem.amount -= (ushort)amount;

        UpdateInventory();
    }

    void DropItem(ItemSlot targetSlot)
    {
        if (targetSlot.placedItem == null) return;

        ItemManager.Instance.DropItemServerRpc(targetSlot.placedItem.itemId, targetSlot.placedItem.amount, targetSlot.placedItem.data, Player.localPlayer.transform.position + (Vector3.up* 1.5f), Player.localPlayer.cameraTransform.forward * 2f);
        Destroy(targetSlot.placedItem.gameObject);
        targetSlot.placedItem = null;

        UpdateInventory();
    }

    #endregion

    #region Update
    
    void HotkeysInputs()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            MoveItemToSlot(inventoryItemSlots[1], inventoryItemSlots[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            MoveItemToSlot(inventoryItemSlots[2], inventoryItemSlots[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            MoveItemToSlot(inventoryItemSlots[3], inventoryItemSlots[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            MoveItemToSlot(inventoryItemSlots[4], inventoryItemSlots[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            MoveItemToSlot(inventoryItemSlots[5], inventoryItemSlots[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            MoveItemToSlot(inventoryItemSlots[6], inventoryItemSlots[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            MoveItemToSlot(inventoryItemSlots[7], inventoryItemSlots[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            MoveItemToSlot(inventoryItemSlots[8], inventoryItemSlots[0]);
        }
    }

    #endregion

    #region other
    public void OnHover(ItemSlot itemSlot)
    {
        hoveredSlot = itemSlot;
        if (hoveredSlot.placedItem == null) return;
        descriptionPic.sprite = hoveredSlot.placedItem.itemSprite;
        descriptionPic.color = Color.white;
        descriptionTMP.text = hoveredSlot.placedItem.description;
    }
    #endregion
}
