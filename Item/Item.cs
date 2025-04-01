using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : SerializedScriptableObject
{
    public string displayName = "NAME!!";
    public string extraInfo = "Description?";
    public Sprite icon;
    public Color iconColor = Color.white;

    [Tooltip("If set true can't be deleted or sent to allies.")]
    public bool canBeTransferred = true;

    public static string GetID(Item item)
    {
        if (item == null)
            return "";
        return item.displayName;
    }

    public virtual string GetDescription(PlayerItems playerItems)
    {
        return $"<b>{ displayName}</b>\n{extraInfo}";
    }

    public virtual void Equip(PlayerItems items, ItemInfo info, int? lastSlot)
    { }

    public virtual void Unequip(PlayerItems items, ItemInfo info)
    { }

    public virtual void ResetGame(PlayerItems items, ItemInfo info)
    { }


    public static void AssignIcon(ItemInfo itemInfo, Image iconImage, bool inInventory)
    {
        var text = iconImage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
            text.text = "";

        AssignIcon(itemInfo?.item, iconImage);
        if (itemInfo == null)
            return;


        if (text != null && itemInfo.item != null && itemInfo.item is Tool t )
        {
            if (t.contract && inInventory)
                text.text = t.maxUses.ToString();
            else
                text.text = itemInfo.count.ToString();
        }
        
    }

    public static void AssignIcon(Item item, Image iconImage)
    {
        if (item == null)
        {
            iconImage.color = new Color(0f, 0f, 0f, 0f);
        }
        else
        {
            iconImage.color = item.iconColor;
            iconImage.sprite = item.icon;
        }
    }

}

public class ItemInfo
{
    public Item item;
    public List<string> modifications = new();
    public int count;

    public ItemInfo Copy()
    {
        ItemInfo copy = new()
        {
            item = item,
            count = count,
        };
        copy.modifications.AddRange(modifications);
        return copy;
    }

    public bool CanCombine(ItemInfo other)
    {
        return other.item == item && other.modifications.MemberwiseCompare(modifications);
    }

    public bool Combine(ItemInfo other)
    {
        if (!CanCombine(other) || other == this)
        {
            return false;
        }
        count += other.count;
        other.count = 0;
        return true;
    }
}
