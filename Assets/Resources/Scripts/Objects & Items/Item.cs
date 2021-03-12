﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum ItemType { HealthPotion = 0, PrimaryWeapon = 1, SecondaryWeapon = 2, QuestItem = 3, Valuable = 4, Shield = 5 }
[Serializable][CreateAssetMenu(fileName ="New Item", menuName = "Assets/Item")]
public class Item: ScriptableObject
{
    //[HideInInspector]
#if UNITY_EDITOR
    [UniqueID]
#endif
    public string sID;
    public string sItemName;
    [TextArea(3,5)]
    public string sItemDescription;
    public ItemType eType;
    public float fEffectValue;
    [Tooltip("Only Applicable for Weapons")]
    public float fWeaponKnockback;
    public int iPrice;
    public Sprite itemIcon;
    public bool bIsStackable;
    public int iQuantity = 1;
    public int iStackLimit;
    public bool bIsEquipable;
    public bool bIsEquipped { get; private set; }
    structItem structThisItem;

    public GameObject prefabItem;

    public Item(Item _item)
    {
        sID = _item.sID;
        iQuantity = _item.iQuantity;
        sItemName = _item.sItemName;
        sItemDescription = _item.sItemDescription;
        eType = _item.eType;
        fEffectValue = _item.fEffectValue;
        fWeaponKnockback = _item.fWeaponKnockback;
        iPrice = _item.iPrice;
        itemIcon = _item.itemIcon;
        bIsStackable = _item.bIsStackable;
        iStackLimit = _item.iStackLimit;
        prefabItem = _item.prefabItem;
        bIsEquipable = _item.bIsEquipable;
        bIsEquipped = _item.bIsEquipped;

        structThisItem = new structItem();
        structThisItem.sID = sID;
        structThisItem.iQuantity = iQuantity;
        structThisItem.bEquipped = bIsEquipped;
    }
    public void SetItemVariables(structItem _structItem)
    {
        iQuantity = _structItem.iQuantity;
        bIsEquipped = _structItem.bEquipped;
        structThisItem.iQuantity = iQuantity;
        structThisItem.bEquipped = bIsEquipped;
    }
    public void SetItemQuantity(int _iQuantity)
    {
        iQuantity = _iQuantity;
        structThisItem.iQuantity = iQuantity;
    }
    public bool UseItem(PlayerController _player)
    {
        switch (eType)
        {
            case ItemType.HealthPotion:
                 return UseHealthPotion(_player);              
            case ItemType.PrimaryWeapon:
                return EquipPrimaryWeapon(_player);
            case ItemType.Shield:
                return EquipShield(_player);
            default:
                return false;
        }
    }
    public float ItemUseValue()
    {
        return fEffectValue;
    }
    
    public Sprite GetSprite()
    {
        return itemIcon;
    }
    public structItem GetItemStruct()
    {
        structThisItem.bEquipped = bIsEquipped;
        return structThisItem;
    }
    public void SetItem(structItem _structItem)
    {
        structThisItem = _structItem;
    }
    public void SetEquipped(bool _bEquipped)
    {
        bIsEquipped = _bEquipped;
    }
    
    public ItemContainer GetItemPrefab()
    {
        return prefabItem.GetComponent<ItemContainer>();
    }

    /// /Items Use Functions
    public bool UseHealthPotion(PlayerController _player)
    {
        if (_player.fCurrentHitPoints < _player.fMaxHitPoints)
        {
            _player.fCurrentHitPoints += (int)fEffectValue;
            _player.HealthCheck();
            _player.OnReciveDamageUI.Invoke();
            return true;
        }
        return false;
    }
    public bool EquipPrimaryWeapon(PlayerController _player)
    {
        if (!bIsEquipped)
        {
            if (!_player.IsAttacking() && !_player.PrimaryWeaponEquipped())
            {
                bIsEquipped = true;
                UnequipOtherSimilarItem(_player.GetInventory(), eType);
                _player.SetPrimaryWeaponEquipped(this);
                return true;
            }
        }
        else
        {
            if (!_player.IsAttacking() && !_player.PrimaryWeaponEquipped())
            {
                bIsEquipped = false;
                _player.SetPrimaryWeaponEquipped(null);
                return true;
            }
        }
        return false;
    }
    public bool EquipSecondaryWeapon(PlayerController _player)
    {
        if (bIsEquipped)
        {
            bIsEquipped = false;
            _player.SetSecondaryWeaponEquipped(this);
            return true;
        }
        else
        {
            bIsEquipped = true;
            _player.SetSecondaryWeaponEquipped(this);
            return true;
        }
    }
    public bool EquipShield(PlayerController _player)
    {
        if (!bIsEquipped)
        {
            if (!_player.IsUsingShield())
            {
                bIsEquipped = true;
                UnequipOtherSimilarItem(_player.GetInventory(), eType);
                _player.SetShieldEquipped(this);
                return true;
            }
        }
        else
        {
            if (!_player.IsUsingShield())
            {
                bIsEquipped = false;
                _player.SetShieldEquipped(null);
                return true;
            }
        }
        return false;
    }

    public void UnequipOtherSimilarItem(Inventory _inventory, ItemType _eItemType)
    {
        foreach (var _item in _inventory.lstItems)
        {
            if(_item.eType == _eItemType)
            {
                _item.bIsEquipped = false;
            }
        }
    }
}
