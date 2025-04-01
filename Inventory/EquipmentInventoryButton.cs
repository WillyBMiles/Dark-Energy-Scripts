using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventoryButton : InventoryButton
{
    EquipmentManager equipmentManager;
    protected override void OnStart()
    {
        equipmentManager = GetComponentInParent<EquipmentManager>();
    }

    public override void OnLeftClick()
    {
        equipmentManager.Equip(myItem);
    }
    public override void OnRightClick()
    {
        //Pass
    }
}
