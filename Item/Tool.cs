using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Tool", menuName = "Tool")]
public class Tool : UsableItem
{
    public bool contract;

    [Header("Only used if it's a contract.")]
    public int maxUses;

    public override void Equip(PlayerItems items, ItemInfo info, int? lastSlot)
    {
        base.Equip(items, info, lastSlot);
        if (contract && items.HasInitialized && lastSlot == null)
        {
            info.count = 0;
        }
            
    }

    public override void ResetGame(PlayerItems items, ItemInfo info)
    {
        base.ResetGame(items, info);
        if (contract)
        {
            info.count = GetMaxCount(items);
        }
            
    }

    public override void FinishUse(PlayerItems items, ItemInfo info)
    {
        base.FinishUse(items, info);
        if (contract && GetMaxCount(items) < info.count)
        {
            info.count = GetMaxCount(items);
        }

        info.count--;
        if (PlayerItems.localItems != null && info.count <= 0 && !contract)
        {
            PlayerItems.localItems.DestroyItem(info);
        }
    }

    int GetMaxCount(PlayerItems items)
    {
        return (int)ShipStats.AdjustValue(maxUses, items.ship.stats, Stat.StatEnum.ContractCapacity);
    }
}
