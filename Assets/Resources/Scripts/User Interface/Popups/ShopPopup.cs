using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
public class ShopPopup : Popup
{
    const float fItemDeductionAmount = 2f;

    public TextMeshProUGUI playerGoldTxt;
    public TextMeshProUGUI itemPriceTxt;
    public TextMeshProUGUI itemDescriptionTxt;

    public Transform playerInventoryUI;
    public Transform merchantInventoryUI;
    public ButtonElement playerInventoryButton;
    public ButtonElement merchantInventoryButton;
    
    public Sprite lockImage;
    bool bItemMenuOpen;

    private ButtonElement selectedButtonElement;
    private InventorySlot selectedSlot;

    InventorySlot [] playerInventorySlotsLst;
    InventorySlot [] merchantInventorySlotsLst;

    PlayerController player;
    private void Start()
    {
        open();
        playerInventorySlotsLst = playerInventoryUI.GetComponentsInChildren<InventorySlot>();
        merchantInventorySlotsLst = merchantInventoryUI.GetComponentsInChildren<InventorySlot>();
        player = PlayerController.Instance;
        close();
    }
    private void Update()
    {
        if (container.gameObject.activeSelf)
        {
            if (!bItemMenuOpen)
                MenuKeysInput();
        }
    }

    public void open(Inventory _merchantInventory)
    {
        base.open();
        playerInventoryButton.SetSelectedElement(true);
        GameController.inPlayMode = false;
        playerGoldTxt.text = player.GetInventory().GetGoldAmount().ToString();
        InitializePlayerInventoryUI(player.GetInventory());
        InitializeMerchantInventoryUI(_merchantInventory);
    }

    public override void close()
    {
        base.close();
        GameController.inPlayMode = true;
    }

    public void BuyItem(Item _item)
    {
        if (player.GetInventory().GetGoldAmount() >= _item.iPrice)
        {
            if(!player.GetInventory().IsFull())
            {
                Item _itemToAdd = ScriptableObject.CreateInstance<Item>();// new Item(_item);
                _itemToAdd.SetItem(_item);
                player.GetInventory().AddItem(_itemToAdd);
                player.GetInventory().SetGoldAmount(-_itemToAdd.iPrice);
                InitializePlayerInventoryUI(player.GetInventory());
            }
            else
                PopupUIManager.Instance.msgBoxPopup.ShowMessageDirectly("Inventory is Full..");
        }
        else
        {
            PopupUIManager.Instance.msgBoxPopup.ShowMessageDirectly("Not enough Gold..");
        }
        SetItemMenuOpenBool(false);
    }
    public void SellItem(Item _item, int _iSlotNumber)
    {
        player.GetInventory().RemoveItemInSlot(_item, _iSlotNumber);
        int _itemSalePrice = (int)(_item.iPrice / fItemDeductionAmount);
        if (_itemSalePrice <= 0)
            _itemSalePrice = 1;
        player.GetInventory().SetGoldAmount(_itemSalePrice);
        InitializePlayerInventoryUI(player.GetInventory());

        for (int i = 0; i < merchantInventorySlotsLst.Length; i++)
        {
            if (!merchantInventorySlotsLst[i].gameObject.activeSelf)
            {
                merchantInventorySlotsLst[i].gameObject.SetActive(true);
                merchantInventorySlotsLst[i].UpdateSlot(_item, true, true);
                break;
            }
        }
        SetItemMenuOpenBool(false);
    }

    public void InitializePlayerInventoryUI(Inventory _playerInventory)
    {
        playerGoldTxt.text = _playerInventory.GetGoldAmount().ToString();
        for (int i = 0; i < playerInventorySlotsLst.Length; i++)
        {
            playerInventorySlotsLst[i].EmptySlot();
            playerInventorySlotsLst[i].SetSlotNumber(i);
        }

        LockSlots(_playerInventory);

        if (_playerInventory.lstItems.Count <= _playerInventory.iInventorySize)
        {
            for (int i = 0; i < _playerInventory.lstItems.Count; i++)
            {
                playerInventorySlotsLst[i].UpdateSlot(_playerInventory.lstItems[i], true, false);
            }
        }
    }
    public void LockSlots(Inventory _inventory)
    {
        for (int i = _inventory.iInventorySize; i < playerInventorySlotsLst.Length; i++)
        {
            playerInventorySlotsLst[i].icon.gameObject.SetActive(true);
            playerInventorySlotsLst[i].icon.sprite = lockImage;
        }
    }

    public void InitializeMerchantInventoryUI(Inventory _merchantInventory)
    {
        for (int i = 0; i < merchantInventorySlotsLst.Length; i++)
        {
            merchantInventorySlotsLst[i].EmptySlot();
            merchantInventorySlotsLst[i].SetSlotNumber(i);
        }

        if (_merchantInventory.lstItems.Count <= _merchantInventory.iInventorySize)
        {
            for (int i = 0; i < _merchantInventory.lstItems.Count; i++)
            {
                merchantInventorySlotsLst[i].UpdateSlot(_merchantInventory.lstItems[i], true, true);
            }
        }
        DisableSlots();
    }
    public void DisableSlots()
    {
        for (int i = 0; i < merchantInventorySlotsLst.Length; i++)
        {
            if (merchantInventorySlotsLst[i].GetItem() == null)
                merchantInventorySlotsLst[i].gameObject.SetActive(false);
        }
    }

    int iSelectedSlot = 0;
    const int iWidthPlayer = 5;
    const int iHeightPlayer = 4;
    const int iWidthMerchant = 3;
    const int iHeightMerchant = 10;
    bool bSwitchToMerchant = false;
    public override void MenuKeysInput()
    {
        if (Input.GetAxisRaw("Horizontal") < 0f && Input.anyKeyDown) // going left
        {
            if (bSwitchToMerchant)
            {
                if (iSelectedSlot == 0 || iSelectedSlot % iWidthMerchant == 0)
                {
                    bSwitchToMerchant = false;
                    ClickPlayerInventory();
                }
                else if (iSelectedSlot > 0)
                {
                    SetSelected(merchantInventorySlotsLst[iSelectedSlot - 1]);
                }
               
            }
            else
            {
                if (iSelectedSlot == 0 || iSelectedSlot % iWidthPlayer == 0)
                {
                    bSwitchToMerchant = true;
                    ClickMerchantInventory();
                }
                else if (iSelectedSlot > 0)
                {
                    SetSelected(playerInventorySlotsLst[iSelectedSlot - 1]);
                }
            }

        }
        else if (Input.GetAxisRaw("Horizontal") > 0f && Input.anyKeyDown) // going right
        {
            if (bSwitchToMerchant)
            {
                if ((iSelectedSlot + 1) % iWidthMerchant == 0)
                {
                    bSwitchToMerchant = false;
                    ClickPlayerInventory();
                }
                else if (iSelectedSlot >= 0)
                {
                    if(merchantInventorySlotsLst[iSelectedSlot + 1].gameObject.activeSelf)
                    {
                        SetSelected(merchantInventorySlotsLst[iSelectedSlot + 1]);
                    }
                }
            }
            else
            {
                if ((iSelectedSlot + 1) % iWidthPlayer == 0)
                {
                    bSwitchToMerchant = true;
                    ClickMerchantInventory();
                }
                else if (iSelectedSlot >= 0)
                {
                    SetSelected(playerInventorySlotsLst[iSelectedSlot + 1]);
                }
            }
        }
        else if (Input.GetAxisRaw("Vertical") < 0 && Input.anyKeyDown) // going down
        {
            // int _iSlotsInOneRow = (int)panelRectTransform.rect.width / ((int)panelLayout.cellSize.x + (int)panelLayout.spacing.x);
            if (bSwitchToMerchant)
            {
                if (merchantInventorySlotsLst[iSelectedSlot + 1].gameObject.activeSelf)
                {
                     if (iSelectedSlot < iWidthMerchant * (iHeightMerchant - 1))
                        SetSelected(merchantInventorySlotsLst[iSelectedSlot + iWidthMerchant]);
                }
            }
            else
            {
                if (iSelectedSlot < iWidthPlayer * (iHeightPlayer - 1))
                    SetSelected(playerInventorySlotsLst[iSelectedSlot + iWidthPlayer]);
            }
                
        }
        else if (Input.GetAxisRaw("Vertical") > 0 && Input.anyKeyDown) // going up
        {
            if (bSwitchToMerchant)
            {
                if (iSelectedSlot > iWidthMerchant - 1)
                    SetSelected(merchantInventorySlotsLst[iSelectedSlot - iWidthMerchant]);
            }
            else
            {
                if (iSelectedSlot > iWidthPlayer - 1)
                    SetSelected(playerInventorySlotsLst[iSelectedSlot - iWidthPlayer]);
            }
        }
        else if (Input.GetButtonDown("Interact"))
        {
            if (selectedSlot != null)
            {
                selectedSlot.OpenItemMenu();
                selectedSlot = null;
            }
        }
    }
    public void SetSelected(InventorySlot _iSlot, bool _bOnPointerEnter = false)
    {
        if (selectedSlot != null)
            selectedSlot.SetSelectedElement(false);

        selectedSlot = _iSlot;

        if (bSwitchToMerchant)
        {
            for (int i = 0; i < merchantInventorySlotsLst.Length; i++)
            {
                merchantInventorySlotsLst[i].SetSelectedElement(false);
                if (merchantInventorySlotsLst[i] == selectedSlot)
                {
                    iSelectedSlot = i;
                }
            }
        }
        else
        {
            for (int i = 0; i < playerInventorySlotsLst.Length; i++)
            {
                playerInventorySlotsLst[i].SetSelectedElement(false);
                if (playerInventorySlotsLst[i] == selectedSlot)
                {
                    iSelectedSlot = i;
                }
            }
        }

        if (selectedSlot)
        {
            selectedSlot.SetSelectedElement(true);
            
            if (selectedSlot.GetItem())
            {
                if (!_bOnPointerEnter)
                {
                    itemDescriptionTxt.text = selectedSlot.GetItem().sItemDescription;
                    if(bSwitchToMerchant)
                        itemPriceTxt.text = "Item Cost: " + selectedSlot.GetItem().iPrice.ToString();
                    else
                    {
                        int _itemSalePrice = (int)(selectedSlot.GetItem().iPrice / fItemDeductionAmount);
                        if (_itemSalePrice <= 0)
                            _itemSalePrice = 1;

                        itemPriceTxt.text = "Sells For: " + _itemSalePrice.ToString();
                    }
                }
                else
                {
                    if(merchantInventorySlotsLst.Contains(selectedSlot))
                    {
                        itemPriceTxt.text = "Item Cost: " + selectedSlot.GetItem().iPrice.ToString();
                        ActivateMerchantSideButton();
                    }
                    else
                    {
                        int _itemSalePrice = (int)(selectedSlot.GetItem().iPrice / fItemDeductionAmount);
                        if (_itemSalePrice <= 0)
                            _itemSalePrice = 1;

                        itemPriceTxt.text = "Sells For: " + _itemSalePrice.ToString();
                        ActivatePlayerSideButton();
                    }
                }
            }
        }

    }
    public void ClickPlayerInventory()
    {
        merchantInventoryButton.SetSelectedElement(false);
        playerInventoryButton.SetSelectedElement(true);
        SetSelected(playerInventorySlotsLst[0]);
    }
    public void ClickMerchantInventory()
    {
        playerInventoryButton.SetSelectedElement(false);
        merchantInventoryButton.SetSelectedElement(true);
        SetSelected(merchantInventorySlotsLst[0]);
    }
    public void ActivatePlayerSideButton()
    {
        merchantInventoryButton.SetSelectedElement(false);
        playerInventoryButton.SetSelectedElement(true);
    }
    public void ActivateMerchantSideButton()
    {
        playerInventoryButton.SetSelectedElement(false);
        merchantInventoryButton.SetSelectedElement(true);
    }
    public void SetItemMenuOpenBool(bool _setBool)
    {
        bItemMenuOpen = _setBool;
    }
}
