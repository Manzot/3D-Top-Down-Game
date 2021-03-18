using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory 
{
    public int iInventorySize = 20;
    public List<Item> lstItems;

    int iGoldAmount = 0;
    structInventory structInventory;

    public Inventory(int _iInventoryStartSize)
    {
        structInventory = new structInventory();
        structInventory.iInventorySize = _iInventoryStartSize;
        lstItems = new List<Item>();
        iInventorySize = structInventory.iInventorySize;
    }

    public void SetGoldAmount(int _iAmountToAddOrRemove)
    {
        iGoldAmount += _iAmountToAddOrRemove;
    }
    public int GetGoldAmount()
    {
        return iGoldAmount;
    }
    public void AddItem(Item _item)
    {
        if(lstItems.Count < iInventorySize)
        {
            lstItems.Add(_item);
        }
        else
        {
            Debug.Log("Your Inventory is full.");
        }
    }
    public bool HasItem(string _sId)
    {
        for (int i = 0; i < lstItems.Count; i++)
        {
            if (lstItems[i].sID == _sId)
            {
                return true;
            }
        }
        return false;
    }
    public bool HasQuestItem(Item _item)
    {
        for (int i = 0; i < lstItems.Count; i++)
        {
            if (lstItems[i].sID == _item.sID)
            {
                if (lstItems[i].eType == _item.eType)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void RemoveQuestItem(Item _item)
    {
        for (int i = 0; i < lstItems.Count; i++)
        {
            if (lstItems[i].sID == _item.sID)
            {
                if(lstItems[i].eType == _item.eType)
                {
                    lstItems.Remove(lstItems[i]);
                    break;
                }
            }
        }
    }
    public void RemoveItemInSlot(Item _item, int _iNumber)
    {
        
         lstItems.Remove(lstItems[_iNumber]);
    }
    public void UpdateItemAmount(Item _item)
    {
        for (int i = 0; i < lstItems.Count; i++)
        {
            if (lstItems[i].sID == _item.sID)
            {
                lstItems[i].SetItemQuantity(lstItems[i].iQuantity - 1);
            }
        }
    }
    public void UpdateItem(Item _item)
    {
        for (int i = 0; i < lstItems.Count; i++)
        {
            if (lstItems[i].sID == _item.sID)
            {
                lstItems[i] = ScriptableObject.CreateInstance<Item>();// new Item(_item);
                lstItems[i].SetItem(_item);
                break;
            }
        }
    }
    public void UpdateItemInSlot(Item _item, int _iNumber)
    {
        lstItems[_iNumber] = ScriptableObject.CreateInstance<Item>();// new Item(_item);
        lstItems[_iNumber].SetItem(_item);
    }
    public structInventory SaveInventoryStats()
    {
        structInventory.itemsLst = new List<structItem>();
        structInventory.iGoldAmount = iGoldAmount;
        for (int i = 0; i < lstItems.Count; i++)
        {
            structInventory.itemsLst.Add(lstItems[i].GetItemStruct());
        }
        return structInventory;
    }
    public void LoadInventoryStats(structInventory _structInventory)
    {
        structInventory = _structInventory;
        iGoldAmount = structInventory.iGoldAmount;
        iInventorySize = structInventory.iInventorySize;

        lstItems = new List<Item>();

        List<structItem> _savedItemsLst = structInventory.itemsLst;

        for (int i = 0; i < _savedItemsLst.Count; i++)
        {
            Item _newItem = ScriptableObject.CreateInstance<Item>();// new Item(_item);
            _newItem.SetItem(ItemDatabaseManager.Instance.GetItemByID(_savedItemsLst[i].sID));// new Item(ItemDatabaseManager.Instance.GetItemByID(_savedItemsLst[i].sID));
            if(_newItem != null)
            {
                _newItem.SetItemVariables(_savedItemsLst[i]);
                lstItems.Add(_newItem);
            }
        }
    }
    public bool IsFull()
    {
        return lstItems.Count >= iInventorySize;
    }
}
