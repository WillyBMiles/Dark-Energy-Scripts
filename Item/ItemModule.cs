using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Module")]
public class ItemModule : Item
{
    public StatusEffect statusEffect = new();

    public override void Equip(PlayerItems items, ItemInfo info, int? lastSlot)
    {
        base.Equip(items, info, lastSlot);
        if (items.ship.stats != null)
            items.ship.stats.AddStatusEffect(statusEffect);
    }

    public override void Unequip(PlayerItems items, ItemInfo info)
    {
        base.Unequip(items, info);
        if (items.ship.stats != null)
            items.ship.stats.RemoveStatusEffect(statusEffect);
    }
}
