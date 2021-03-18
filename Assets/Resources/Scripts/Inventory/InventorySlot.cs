using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public Image icon;
    public Image imgSelected;
    public TextMeshProUGUI quanintyText;
    public TextMeshProUGUI equippedText;
    public RectTransform tMenuPosition;
    private Item item;
    private bool bSelected;
   // private InventoryPopup inventoryPopup;
    int iSlotNumber;

    bool bShopSlot;
    bool bMerchantSlot;
    public void UpdateSlot(Item _item)
    {
        item = ScriptableObject.CreateInstance<Item>();// new Item(_item);
        item.SetItem(_item);

        icon.gameObject.SetActive(true);
        icon.sprite = item.GetSprite();

        equippedText.text = "";
        quanintyText.text = "";

        if (item.bIsEquipable) 
        {
            if (item.bIsEquipped)
                equippedText.text = "E";
        }
        if(item.eType == ItemType.QuestItem)
        {
            equippedText.text = "Q";
        }

        if (item.iQuantity > 1)
        {
            quanintyText.text = item.iQuantity.ToString();
        }
    }
    public void UpdateSlot(Item _item, bool _bShopSlot, bool _bIsMerchants)
    {
        bShopSlot = _bShopSlot;
        bMerchantSlot = _bIsMerchants;

        item = ScriptableObject.CreateInstance<Item>();// new Item(_item);
        item.SetItem(_item);

        icon.gameObject.SetActive(true);
        icon.sprite = item.GetSprite();

        equippedText.text = "";
        quanintyText.text = "";

        if (item.bIsEquipable)
        {
            if (item.bIsEquipped)
                equippedText.text = "E";
        }
        if (item.eType == ItemType.QuestItem)
        {
            equippedText.text = "Q";
        }

        if (item.iQuantity > 1)
        {
            quanintyText.text = item.iQuantity.ToString();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OpenItemMenu();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PopupUIManager.Instance.inventoryPopup.container.gameObject.activeSelf)
            PopupUIManager.Instance.inventoryPopup.SetSelected(this);
        if(PopupUIManager.Instance.shopPopup.container.gameObject.activeSelf)
            PopupUIManager.Instance.shopPopup.SetSelected(this, true);
    }
    public void OpenItemMenu()
    {
        if (item != null)
        {
            if (item.eType != ItemType.QuestItem)
            {
                if (bShopSlot) 
                {
                    PopupUIManager.Instance.shopPopup.SetItemMenuOpenBool(true);
                    List<structSubMenu> _lstSubMenu = new List<structSubMenu>();
                    structSubMenu _subMenu = new structSubMenu();
                    if (bMerchantSlot)
                    {
                        _subMenu.sName = "Buy";
                        _subMenu.action = delegate () { ClickBuy(); };
                    }
                    else
                    {
                        _subMenu.sName = "Sell";
                        _subMenu.action = delegate () { ClickSell(); };
                    }
                    _lstSubMenu.Add(_subMenu);

                    _subMenu = new structSubMenu();
                    _subMenu.sName = "Cancel";
                    _subMenu.action = delegate () { ClickCancel(); };
                    _lstSubMenu.Add(_subMenu);

                    PopupUIManager.Instance.subMenuPopup.openMenu(_lstSubMenu, tMenuPosition.position);
                }
                else
                {
                    PopupUIManager.Instance.inventoryPopup.SetItemMenuOpenBool(true);
                    List<structSubMenu> _lstSubMenu = new List<structSubMenu>();
                    structSubMenu _subMenu = new structSubMenu();
                    _subMenu.sName = "Use";
                    if (item.bIsEquipable)
                    {
                        _subMenu.sName = "Equip";

                        if (item.bIsEquipped)
                        {
                            _subMenu.sName = "Unequip";
                        }
                    }
                    _subMenu.action = delegate () { ClickUse(); };
                    _lstSubMenu.Add(_subMenu);

                    _subMenu = new structSubMenu();
                    _subMenu.sName = "Discard";
                    _subMenu.action = delegate () { ClickDiscard(); };
                    _lstSubMenu.Add(_subMenu);

                    if (item.bIsStackable)
                    {
                        _subMenu = new structSubMenu();
                        _subMenu.sName = "Discard All";
                        _subMenu.action = delegate () { ClickDiscardAll(); };
                        _lstSubMenu.Add(_subMenu);
                    }

                    _subMenu = new structSubMenu();
                    _subMenu.sName = "Cancel";
                    _subMenu.action = delegate () { ClickCancel(); };
                    _lstSubMenu.Add(_subMenu);

                    PopupUIManager.Instance.subMenuPopup.openMenu(_lstSubMenu, tMenuPosition.position);
                }
                
            }
            else
            {
                PopupUIManager.Instance.msgBoxPopup.ShowMessageDirectly("Can't use a Quest Item..");
            }
        }
          
    }
    public void ClickUse()
    {
        if (item.UseItem(PlayerController.Instance))
        {
            Debug.Log(item.sItemName + " used");

            if (item.bIsStackable)
                item.SetItemQuantity(item.iQuantity - 1);
           
            UpdatePlayerIventory(item); 
        }
        else
        {
            if(item.bIsEquipable)
                PopupUIManager.Instance.msgBoxPopup.ShowMessageDirectly("Can't use this action right now..... ");
            else
                PopupUIManager.Instance.msgBoxPopup.ShowMessageDirectly("Can't use this item right now..... ");

        }
        PopupUIManager.Instance.inventoryPopup.SetItemMenuOpenBool(false);
    }
    public void ClickDiscard()
    {
        Debug.Log(item.sItemName + " Discarded");
        bool _bDropItem = true;

        if (item.bIsStackable)
        {
            item.SetItemQuantity(item.iQuantity - 1);
        }
        else
        {
            if (item.bIsEquipable)
            {
                if (item.bIsEquipped)
                {
                    _bDropItem = false;
                    PopupUIManager.Instance.msgBoxPopup.ShowMessageDirectly("Cannot Discard Equipped Item.", 1);
                }
                else
                    item.SetItemQuantity(0);
            }
            else
                item.SetItemQuantity(0);
        }
        if (_bDropItem)
        {
            Vector3 _itemDropPosition = PlayerController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 1.5f, Random.Range(-1f, 1f));
            ItemContainer _newDroppedItem = Instantiate(item.GetItemPrefab(), _itemDropPosition, Quaternion.identity);
            _newDroppedItem.SetItem(item);
        }

        UpdatePlayerIventory(item);

        PopupUIManager.Instance.inventoryPopup.SetItemMenuOpenBool(false);
    }
    public void ClickDiscardAll()
    {
        Debug.Log(item.sItemName + " Discarded");
        for (int i = 0; i < item.iQuantity; i++)
        {
            Vector3 _itemDropPosition = PlayerController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 1.5f, Random.Range(-1f, 1f));
            ItemContainer _newDroppedItem = Instantiate(item.GetItemPrefab(), _itemDropPosition, Quaternion.identity);

            _newDroppedItem.SetItem(item);
        }

        item.SetItemQuantity(0); // that will make it equal to 0

        UpdatePlayerIventory(item);
        PopupUIManager.Instance.inventoryPopup.SetItemMenuOpenBool(false);
    }
    public void ClickCancel()
    {
        PopupUIManager.Instance.inventoryPopup.SetItemMenuOpenBool(false);
        PopupUIManager.Instance.shopPopup.SetItemMenuOpenBool(false);
    }
    public void UpdatePlayerIventory(Item _item) // It Removes or equip/unequip the item and updates player inventory
    {
        if (item.bIsEquipable)
        {
            Inventory _updatedUIInventory = PlayerController.Instance.GetInventory();

            if(_item.iQuantity > 0)
                _updatedUIInventory.UpdateItemInSlot(item, iSlotNumber);
            else
            {
                _updatedUIInventory.RemoveItemInSlot(item, iSlotNumber);
                EmptySlot();
            }

            PlayerController.Instance.UpdateInventory(_updatedUIInventory);
        }
        else
        {
            if (item.bIsStackable)
            {
                Inventory _updatedUIInventory = PlayerController.Instance.GetInventory();
                if(_item.iQuantity > 0)
                    _updatedUIInventory.UpdateItemInSlot(item, iSlotNumber);
                else
                {
                    _updatedUIInventory.RemoveItemInSlot(item, iSlotNumber);
                    EmptySlot();
                }
                PlayerController.Instance.UpdateInventory(_updatedUIInventory);
            }
            else
            {
                Inventory _updatedUIInventory = PlayerController.Instance.GetInventory();
                _updatedUIInventory.RemoveItemInSlot(item, iSlotNumber);
                EmptySlot();
                PlayerController.Instance.UpdateInventory(_updatedUIInventory);
            }
        }
    }
    public void EmptySlot()
    {
        item = null;
        icon.sprite = null;
        icon.gameObject.SetActive(false);
        quanintyText.text = "";
        equippedText.text = "";
    }
    public void SetSelectedElement(bool _bSelected)
    {
        bSelected = _bSelected;
        imgSelected.gameObject.SetActive(bSelected);
        SetItemDetails();
    }
    void SetItemDetails()
    {
        if (PopupUIManager.Instance.inventoryPopup != null)
        {
            if (item != null)
            {
                PopupUIManager.Instance.inventoryPopup.txtDetailItemName.text = item.sItemName;
                PopupUIManager.Instance.inventoryPopup.txtDetailItemDescription.text = item.sItemDescription;
            }
            else
            {
                PopupUIManager.Instance.inventoryPopup.txtDetailItemName.text = "No Item Selected";
                PopupUIManager.Instance.inventoryPopup.txtDetailItemDescription.text = "";
            }
        }
    }
    public void SetSlotNumber(int _number)
    {
        iSlotNumber = _number;
    }
    public Item GetItem()
    {
        if (item)
            return item;
        return null;
    }
    public void ClickBuy()
    {
        PopupUIManager.Instance.shopPopup.BuyItem(item);
    }
    public void ClickSell()
    {
        PopupUIManager.Instance.shopPopup.SellItem(item, iSlotNumber);
    }
}
