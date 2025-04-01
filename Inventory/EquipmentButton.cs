using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    static EquipmentButton hover;

    public Image icon;
    ItemInfo itemInfo;
    Tooltip tooltip;
    EquipmentManager manager;
    public FilterType filterType;

    public enum FilterType { 
        Tools,
        Modules,
        Weapons
    }

    [SerializeField]
    public int slotNumber;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponentInParent<EquipmentManager>();
        tooltip = transform.parent.parent.GetComponentInChildren<Tooltip>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (hover == this && itemInfo != null && itemInfo.item != null && PlayerItems.localItems != null)
        {
            tooltip.SetText(itemInfo.item.GetDescription(PlayerItems.localItems));
        }
    }

    InventoryManager.Filter GetFilter(FilterType filterType)
    {
        return filterType switch
        {
            FilterType.Modules => (ItemInfo item) => item.item is ItemModule,
            FilterType.Tools => (ItemInfo item) => item.item is Tool,
            FilterType.Weapons => (ItemInfo item) => item.item is Weapon,
            _ => null

        };
    }

    public void Press()
    {
        if (itemInfo != null && itemInfo.item is Tool t && t.contract)
        {
            ConfirmPopup.instance.Show($"Are you sure you want to change contract {t.displayName}? This will clear the contract and you'll have to refresh it at a station.", PressConfirm, PressDecline, 10f);
        }
        else
        {
            manager.PressButton(this, GetFilter(filterType));
        }
       
    }

    public void PressConfirm()
    {
        manager.PressButton(this, GetFilter(filterType));
    }

    public void PressDecline()
    {
        //pass
    }

    public void SetButton(ItemInfo itemInfo)
    {
        Item.AssignIcon(itemInfo, icon, false);
        this.itemInfo = itemInfo;
    }

    public void Equip(ItemInfo itemInfo)
    {
        if (PlayerItems.localItems == null)
            return;

        switch (filterType)
        {
            case FilterType.Modules:
                PlayerItems.localItems.EquipItem(itemInfo, slotNumber);
                break;
            case FilterType.Tools:
                PlayerItems.localItems.EquipItem(itemInfo, slotNumber);
                break;
            case FilterType.Weapons:
                PlayerItems.localItems.EquipItem(itemInfo, slotNumber);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hover = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hover == this)
        {
            hover = null;
        }
    }
}
