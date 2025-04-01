using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerItems : NetworkBehaviour
{
    public static PlayerItems localItems;

    public List<ItemInfo> inventory = new();
    public Ship ship { get; private set; }

    public bool HasInitialized { get; set; } = false;

    public ItemInfo weapon1;
    Weapon weaponLeft;
    public ItemInfo weapon2;
    Weapon weaponRight;

    public ItemInfo nonauthTool;

    public AnimationClip emptyClip;

    public List<ItemInfo> tools = new();
    public int currentTool = 0;
    public List<ItemInfo> modules = new();

    AnimatorOverrideController animatorOverrideController;
    Animator animator;

    PlayerRPG playerRPG;

    const string L = "Light";
    const string H = "Heavy";
    const string SI = "Sustain_In";
    const string S = "Sustain";
    const string SO = "Sustain_Out";

    public ItemInfo lastItemUsed;
    public HitType lastHitType;

    [SerializeField]
    AudioSource useItemSource;
    [SerializeField]
    AudioSource sustainItemSource;

    void PreStart()
    {
        if (animator != null)
            return;
        animator = GetComponent<Animator>();
        animatorOverrideController = new(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;
        ship = GetComponent<Ship>();
        playerRPG = GetComponent<PlayerRPG>();
    }


    // Start is called before the first frame update
    void Start()
    {
        PreStart();
        if (ship != Ship.playerShip)
            CmdRequestUpdate();
        else
        {
            AuthUpdateInventory();
        }

        MaxToolCapacity();
    }

    void Update()
    {
        if (ship == Ship.playerShip)
            localItems = this;
        bool interrupted = animator.GetCurrentAnimatorStateInfo(0).IsName("Interrupt");

        if (ship.InControl)
        {
            //Controls
            if (ship == Ship.playerShip)
            {
                InventoryButton.InventoryLockOut = !ship.isIdling;


                if (UIMenus.InventoryOpen)
                {
                    animator.SetBool("Sustain1", false);
                    animator.SetBool("Sustain2", false);
                    animator.SetBool("ToolSustain", false);
                }
                else
                {
                    CheckControls();
                }
                CheckAllTrigger();
            }




            AnimatorStateInfo si = animator.GetCurrentAnimatorStateInfo(0);

            if (si.IsName("Light1") || si.IsName("Light2") || si.IsName("ToolLight"))
                lastHitType = HitType.light;
            if (si.IsName("Heavy1") || si.IsName("Heavy2") || si.IsName("ToolHeavy"))
                lastHitType = HitType.heavy;
            if (si.IsName("Sustain_In1") || si.IsName("Sustain1") || si.IsName("Sustain_Out1") ||
                si.IsName("Sustain_In2") || si.IsName("Sustain2") || si.IsName("Sustain_Out2") ||
                si.IsName("ToolSustain_In") || si.IsName("ToolSustain") || si.IsName("ToolSustain_Out"))
                lastHitType = HitType.sustain;

            GetCurrentTool(out ItemInfo toolInfo);
            if (si.IsName("Light1") || si.IsName("Heavy1") || si.IsName("Sustain_In1") || si.IsName("Sustain1") || si.IsName("Sustain_Out1"))
                CheckUsableItemStart(weapon1);
            if (si.IsName("Light2") || si.IsName("Heavy2") || si.IsName("Sustain_In2") || si.IsName("Sustain2") || si.IsName("Sustain_Out2"))
                CheckUsableItemStart(weapon2);
            if (si.IsName("ToolLight") || si.IsName("ToolHeavy") || si.IsName("ToolSustain_In") || si.IsName("ToolSustain") || si.IsName("ToolSustain_Out"))
                CheckUsableItemStart(toolInfo);

            if (ship.isIdling)
            {
                CheckUsableItemEnd();
            }

        }
        else
        {

            if (ship == Ship.playerShip)
                InventoryButton.InventoryLockOut = true;
            if (interrupted)
                CheckUsableItemEnd();
            animator.SetBool("Sustain1", false);
            animator.SetBool("Sustain2", false);
        }

    }
    
    public void CheckControls()
    {
            if (CanAttack(1, HitType.light) && Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
            {
                animator.SetTrigger(GetTrigger(1, HitType.light));
            }
            if (CanAttack(1, HitType.heavy) && Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
            {
                animator.SetTrigger(GetTrigger(1, HitType.heavy));
            }
            animator.SetBool(GetTrigger(1, HitType.sustain), CanAttack(1, HitType.sustain) && Input.GetMouseButton(0));


            if (CanAttack(2, HitType.light) && Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftShift))
            {
                animator.SetTrigger(GetTrigger(2, HitType.light));
            }
            if (CanAttack(2, HitType.heavy) && Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftShift))
            {
                animator.SetTrigger(GetTrigger(2, HitType.heavy));
            }
            animator.SetBool(GetTrigger(2, HitType.sustain), CanAttack(2, HitType.sustain) && Input.GetMouseButton(1));


            if (CanAttack(0, HitType.light) && Input.GetKeyDown(KeyCode.F) && !Input.GetKey(KeyCode.LeftShift))
            {
                animator.SetTrigger(GetTrigger(0, HitType.light));
            }
            if (CanAttack(0, HitType.heavy) && Input.GetKeyDown(KeyCode.F) && Input.GetKey(KeyCode.LeftShift))
            {
                animator.SetTrigger(GetTrigger(0, HitType.heavy));
            }
            animator.SetBool(GetTrigger(0, HitType.sustain), CanAttack(0, HitType.sustain) && Input.GetKey(KeyCode.F));

            if (ship.isIdling)
            {
                if (WhichKeyPressed(out int key))
                {
                    SwapTool(key - 1);
                }
            }
            
    }

    public bool CanAttack(int slot, HitType hitType)
    {
        if (HoveringOverMenu.IsHovering)
            return false;
        if (ship.power <= 0f)
            return false;

        UsableItem w;
        if (slot == 0)
        {
            w = GetCurrentTool(out var info);
            if (info == null)
                return false;
            if (info.count <= 0)
                return false;
            if (info.count == 1 && lastItemUsed != null && 
                lastItemUsed.item == w && !ship.isIdling) //we are actively using the last charge, kinda hacky?
                return false;
        } 
        else if (slot == 1)
            w = weaponLeft;
        else if (slot == 2)
            w = weaponRight;
        else
            return false;
        if (w == null)
            return false;

        return hitType switch
        {
            HitType.heavy => w.heavyClip != null && w.heavyClip != emptyClip,
            HitType.light => w.lightClip != null && w.lightClip != emptyClip,
            HitType.sustain => w.sustainIn != null && w.sustainSustain != null && w.sustainOut != null &&
                                w.sustainIn != emptyClip && w.sustainSustain != emptyClip && w.sustainOut != emptyClip,
            _ => false

        };
    }

    #region Managing Usable Items
    void CheckUsableItemEnd()
    {
        if (lastItemUsed != null)
        {
            UsableItem ui = lastItemUsed.item as UsableItem;
            if (lastHitType == HitType.sustain)
            {
                SustainOutEnd();
            }
            ui.FinishUse(this, lastItemUsed);
            lastItemUsed = null;
        }
    }
    void CheckUsableItemStart(ItemInfo itemInfo)
    {
        if (lastItemUsed == null)
        {
            lastItemUsed = itemInfo;
            if (lastHitType == HitType.sustain)
            {
                SustainInStart();
            }
        }
        if (lastHitType != HitType.sustain)
        {
            StopSustainSound(); //failsafe
        }
    }

    //Stops triggers when you can't attack
    public void CheckAllTrigger()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!CanAttack(i, HitType.light))
            {
                UnsetTrigger(i, HitType.light);
            }
            if (!CanAttack(i, HitType.heavy))
            {
                UnsetTrigger(i, HitType.heavy);
            }
            if (!CanAttack(i, HitType.sustain))
            {
                UnsetTrigger(i, HitType.sustain);
            }
        }
    }


    #endregion

    #region Equipping/Swapping/Pickup/Destroy
    void EquipWeapon(ItemInfo weaponInfo, int slot)
    {
        string pre = slot == 0 ? "Tool" : "";
        string post = slot == 0 ? "" : slot.ToString();

        if (weaponInfo == null)
        {
            if (slot == 1)
            {
                if (weapon1 != null)
                {
                    weapon1.item.Unequip(this, weapon1);
                }
                weapon1 = null;
                weaponLeft = null;
            }
            if (slot == 2)
            {
                if (weapon2 != null)
                {
                    weapon2.item.Unequip(this, weapon2);
                }

                weapon2 = null;
                weaponRight = null;
            }
            animatorOverrideController[pre + L + post] = emptyClip;
            animatorOverrideController[pre + H + post] = emptyClip;
            animatorOverrideController[pre + SI + post] = emptyClip;
            animatorOverrideController[pre + S + post] = emptyClip;
            animatorOverrideController[pre + SO + post] = emptyClip;
        }
        else
        {
            EquipWeapon(null, slot);

            int? currentSlot = IsEquipped(weaponInfo);
            if (currentSlot.HasValue)
            {
                EquipWeapon(null, currentSlot.Value);
            }

            UsableItem weapon = weaponInfo.item as UsableItem;


            animatorOverrideController[pre + L + post] = weapon.lightClip;
            animatorOverrideController[pre + H + post] = weapon.heavyClip;
            animatorOverrideController[pre + SI + post] = weapon.sustainIn;
            animatorOverrideController[pre + S + post] = weapon.sustainSustain;
            animatorOverrideController[pre + SO + post] = weapon.sustainOut;

            if (weapon is Weapon w)
            {
                if (slot == 1)
                {
                    weapon1 = weaponInfo;
                    weaponLeft = w;
                }
                if (slot == 2)
                {
                    weapon2 = weaponInfo;
                    weaponRight = w;
                }
            }


            if (slot == 0)
            {
                nonauthTool = weaponInfo;
            }


        }


        //FINALLY
        if (ship == Ship.playerShip)
            AuthUpdateInventory();

    }

    public void EquipItem(ItemInfo item, int slot)
    {
        PreStart(); // In case we need to equip items before anything else

        int? lastSlot = null;
        if (item != null && item.item != null) //This is for item.item.Equip() at bottom of function
        {
            lastSlot = item.item is Tool ?  GetToolSlot(item) : IsEquipped(item);
        }



        if (item == null || item.item == null || item.item is Weapon _)
        {
            EquipWeapon(item, slot);
        }
        else if (item.item is Tool _)
        {
            if (MaxToolCapacity() > slot && slot >= 0)
            {
                if (item != null)
                {
                    while (tools.Contains(item))
                    {
                        tools[tools.IndexOf(item)] = null;
                    }
                }



                if (tools[slot] != null)
                {
                    UnequipItem(tools[slot]);
                }

                tools[slot] = item;
                if (currentTool == slot)
                {
                    EquipWeapon(item, 0);
                }
            }
        }
        else if (item.item is ItemModule)
        {
            EquipModule(item, slot);
            return;
        }


        if (item != null && item.item != null)
        {
            item.item.Equip(this, item, lastSlot);
        }
        //Equip something else??
    }

    public void EquipTool(ItemInfo info)
    {
        MaxToolCapacity();
        if (GetToolSlot(info).HasValue)
            return;
        int? slot = GetFirstAvailableSlot();
        if (slot.HasValue)
        {
            EquipItem(info, slot.Value);
        }
    }

    public void UnequipItem(ItemInfo item)
    {
        int? currentSlot = IsEquipped(item);
        if (currentSlot.HasValue)
        {
            EquipWeapon(null, currentSlot.Value);
        }
        if (item != null)
        {
            while (tools.Contains(item))
            {
                item.item.Unequip(this, item);
                tools[tools.IndexOf(item)] = null;
            }
        }
        UnequipModule(item);

        

    }

    public void DestroyItem(ItemInfo item)
    {
        UnequipItem(item);
        inventory.Remove(item);
        ship.AnimationReset();
    }

    public void Pickup(IEnumerable<Item> itemToPickup)
    {
        foreach (Item i in itemToPickup)
        {
            ItemInfo item = ContainsItem(i);
            if (item != null && i is Tool t && !t.contract)
            {
                item.count++;
            }
            else
            {
                inventory.Add(new ItemInfo() { item = i, count = 1 });
            }

            if (localItems == this)
            {
                InfoPopupManager.MakePopup("Picked up: " + i.displayName);
            }
        }

        AuthUpdateInventory();
    }

    void SwapTool(int destination)
    {
        currentTool = destination;
        currentTool %= MaxToolCapacity();
        EquipWeapon(tools[currentTool], 0);
    }
    #endregion

    #region Last Weapon
    public float GetLastWeaponDamage(HitType hitType)
    {
        if (lastItemUsed == null)
            return 0f;
        UsableItem item = lastItemUsed.item as UsableItem;
        return item.GetDamage(playerRPG, hitType);
    }

    public float GetLastShieldEfficiency()
    {
        if (lastItemUsed == null)
            return 0f;
        UsableItem item = lastItemUsed.item as UsableItem;
        return ShipStats.AdjustValue( item.GetBlockEfficiency(playerRPG), ship.stats, Stat.StatEnum.ShieldEfficiency);
    }

    #endregion

    #region Item Callbacks

    public void ResetGame()
    {
        foreach (ItemInfo ii in inventory)
        {
            if (ii != null && ii.item !=null)
                ii.item.ResetGame(this, ii);
        }
    }

    public void UseItem()
    {
        if (lastItemUsed == null)
        {
            return;
        }
        UsableItem i = lastItemUsed.item as UsableItem;
        i.Use(this, lastItemUsed, lastHitType);
        useItemSource.clip = lastHitType switch
        {
            HitType.light => i.lightUseSound,
            HitType.heavy => i.heavyUseSound,
            HitType.sustain => i.sustainUseSound,
            _ => null
        };
        useItemSource.Play();
    }

    public void SustainInStart()
    {
        if (lastItemUsed == null)
            return;
        UsableItem i = lastItemUsed.item as UsableItem;
        i.SustainInStart(this, lastItemUsed);
        sustainItemSource.clip = i.sustainLoopSound;
        sustainItemSource.Play();
    }

    public void SustainOutEnd()
    {
        if (lastItemUsed == null)
            return;
        UsableItem i = lastItemUsed.item as UsableItem;
        i.SustainOutEnd(this, lastItemUsed);
        StopSustainSound();
    }

    void StopSustainSound()
    {
        sustainItemSource.Stop();

    }
    #endregion

    #region Info Utilities

    int? GetFirstAvailableSlot()
    {
        int id = 0;
        foreach (ItemInfo t in tools)
        {

            if (t == null)
            {
                return id;
            }
            id++;
        }
        return null;
    }

    public Tool GetCurrentTool()
    {
        return GetCurrentTool(out ItemInfo _);
    }
    public Tool GetCurrentTool(out ItemInfo itemInfo)
    {
        if (!ship.IsAuthoritative)
        {
            if (nonauthTool == null)
            {
                itemInfo = null;
                return null;
            }
            itemInfo = nonauthTool;
            return nonauthTool.item as Tool;
        }

        if (currentTool >= 0 && currentTool < tools.Count)
        {
            itemInfo = tools[currentTool];
            return itemInfo?.item as Tool;
        }
        itemInfo = null;
        return null;
    }
    public int? IsEquipped(ItemInfo itemInfo)
    {
        if (itemInfo == null)
            return null;

        if (itemInfo == weapon1)
            return 1;
        if (itemInfo == weapon2)
            return 2;
        if (tools != null && tools.Count > currentTool && tools[currentTool] == itemInfo)
        {
            return 0;
        }
        return null;
    }

    public int? GetToolSlot(ItemInfo itemInfo)
    {
        if (tools.Contains(itemInfo))
            return tools.IndexOf(itemInfo);
        return null;
    }

    public int NumberOfCopies(ItemInfo item)
    {
        int num = 0;
        foreach (ItemInfo i in inventory)
        {
            if (i.item == item.item && i.modifications.MemberwiseCompare( item.modifications))
                num++;
        }
        return num;
    }


    public void Split(ItemInfo itemInfo)
    {
        if (itemInfo.count <= 1)
        {
            return;
        }

        int nextCount = itemInfo.count / 2;
        itemInfo.count -= nextCount;
        ItemInfo newInfo = itemInfo.Copy();
        newInfo.count = nextCount;
        inventory.Add(newInfo);
    }

    public void Combine(ItemInfo itemInfo)
    {
        if (NumberOfCopies(itemInfo) <= 1 || itemInfo.item is not Tool t || t.contract)
            return;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (itemInfo.Combine(inventory[i]))
            {
                DestroyItem(inventory[i]);
            }
        }
    }

    public int MaxToolCapacity()
    {
        int capacity = playerRPG.GetStat(Stat.StatEnum.Capacity);
        
        int sum = 0;
        foreach (int breakPoint in CapacityBreakPoints)
        {
            if (capacity >= breakPoint)
                sum += 1;
            else
                break;
        }

        sum = (int)ShipStats.AdjustValue(sum, ship.stats, Stat.StatEnum.ToolCapacity);
        while (sum > tools.Count)
        {
            tools.Add(null);
        }
        while (tools.Count > sum)
        {
            UnequipItem(tools[tools.Count - 1]);
            tools.RemoveAt(tools.Count - 1);
        }

        return sum;

    }

    public string GetTrigger(int slot, HitType hittype)
    {
        string pre = slot == 0 ? "Tool" : "";
        string post = slot == 0 ? "" : slot.ToString();

        return hittype switch
        {
            HitType.light => pre + L + post,
            HitType.heavy => pre + H + post,
            HitType.sustain => pre + S + post,
            _ => throw new System.Exception("Trigger not managed: " + hittype.ToString())
        };
    }

    public void UnsetTrigger(int slot, HitType hittype)
    {
        string trigger = GetTrigger(slot, hittype);
        switch (hittype) {
            case HitType.light:
            case HitType.heavy:
                animator.ResetTrigger(trigger);
                break;
            case HitType.sustain:
                animator.SetBool(trigger, false);
                break;
        }
    }




    public ItemInfo ContainsItem(Item item)
    {
        foreach (ItemInfo i in inventory)
        {
            if (i.item == item)
            {
                return i;
            }
        }
        return null;
    }

    bool WhichKeyPressed(out int number)
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            number = 1;
            return true;
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            number = 2;
            return true;
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            number = 3;
            return true;
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            number = 4;
            return true;
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            number = 5;
            return true;
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            number = 6;
            return true;
        }
        if (Input.GetKey(KeyCode.Alpha7))
        {
            number = 7;
            return true;
        }
        if (Input.GetKey(KeyCode.Alpha8))
        {
            number = 8;
            return true;
        }
        if (Input.GetKey(KeyCode.Alpha9))
        {
            number = 9;
            return true;
        }
        if (Input.GetKey(KeyCode.Alpha0))
        {
            number = 10;
            return true;
        }
        number = 0;
        return false;
    }

    #endregion

    #region Module Utilities
    public int? GetNextModuleSlot()
    {
        GetModuleCapacity();

        for (int i = 0; i < modules.Count; i++) {
            if (modules[i] == null)
                return i;
        }
        return null;
    }

    const int MODULE_CAPACITY = 3; //TEMPORARY: Can improve?
    public int GetModuleCapacity()
    {
        int capacity = MODULE_CAPACITY;
        capacity = (int)ShipStats.AdjustValue(capacity, ship.stats, Stat.StatEnum.ModuleCapacity);

        while (modules.Count < capacity)
        {
            modules.Add(null);
        }
        while (modules.Count > capacity)
        {
            UnequipModule(modules.Count - 1);
            modules.RemoveAt(modules.Count - 1);
        }
        return modules.Count;
    }

    public bool IsModuleEquipped(ItemInfo module)
    {
        return modules.Contains(module);
    }

    #endregion

    #region Modules

    public void EquipModule(ItemInfo itemInfo, int? slot = null)
    {
        if (itemInfo == null || itemInfo.item == null || itemInfo.item is not ItemModule)
            return;
        if (modules.Contains(itemInfo) && !slot.HasValue)
        {
            return; //already equipped
        }
        if (!slot.HasValue)
            slot = GetNextModuleSlot();
        if (slot == null)
            return;
        if (slot.Value < 0 || slot.Value >= modules.Count)
            return;
        
        UnequipModule(itemInfo);
        UnequipModule(slot.Value);
        modules[slot.Value] = itemInfo;
        ItemModule im = itemInfo.item as ItemModule;
        im.Equip(this, itemInfo, null);
    }

    public void UnequipModule(int slot)
    {
        if (slot > modules.Count || slot < 0 || modules[slot] == null)
            return;
        if (modules[slot].item != null && modules[slot].item is ItemModule im)
            im.Unequip(this, modules[slot]);
        modules[slot] = null;
    }

    public void UnequipModule(ItemInfo info)
    {
        if (modules.Contains(info))
            UnequipModule(modules.IndexOf(info));
    }

    #endregion

    #region InventoryUpdates
    public void AuthUpdateEquips()
    {
        int w1 = inventory.Contains(weapon1) ? inventory.IndexOf(weapon1) : -1;
        int w2 = inventory.Contains(weapon2) ? inventory.IndexOf(weapon2) : -1;
        GetCurrentTool(out ItemInfo tool);
        int t = inventory.Contains(tool) ? inventory.IndexOf(tool) : -1;

        CmdUpdateEquips(w1, w2, t);
    }

    public void AuthUpdateInventory()
    {
        int w1 = inventory.Contains(weapon1) ? inventory.IndexOf(weapon1) : -1;
        int w2 = inventory.Contains(weapon2) ? inventory.IndexOf(weapon2) : -1;
        GetCurrentTool(out ItemInfo tool);
        int t = inventory.Contains(tool) ? inventory.IndexOf(tool) : -1;
        CmdUpdateInventory(inventory, w1, w2, t);
    }

    public void NonauthUpdateEquips(int w1, int w2, int tool)
    {
        EquipWeapon((w1 >= 0 && inventory.Count > w1) ? inventory[w1] : null, 1);
        EquipWeapon((w2 >= 0 && inventory.Count > w2) ? inventory[w2] : null, 2);

        EquipWeapon((tool >= 0 && inventory.Count > tool) ? inventory[tool] : null, 0);
    }
    #endregion

    #region Commands and RPCs
    [Command(requiresAuthority = false)]
    public void CmdRequestUpdate()
    {
        RpcRequestUpdate();
    }

    [ClientRpc]
    public void RpcRequestUpdate()
    {
        if (ship == Ship.playerShip)
        {
            AuthUpdateInventory();
        }
    }

    [Command]
    public void CmdUpdateInventory(List<ItemInfo> inventory, int weapon1, int weapon2, int tool)
    {
        RpcUpdateInventory(inventory, weapon1, weapon2, tool);
    }

    [ClientRpc]
    public void RpcUpdateInventory(List<ItemInfo> inventory, int weapon1, int weapon2, int tool)
    {
        if (ship == Ship.playerShip)
            return;

        this.inventory = inventory;
        NonauthUpdateEquips(weapon1, weapon2, tool);
    }

    [Command]
    public void CmdUpdateEquips(int weapon1, int weapon2, int tool)
    {
        RpcUpdateEquips(weapon1, weapon2, tool);
    }

    [ClientRpc]
    public void RpcUpdateEquips(int weapon1, int weapon2, int tool)
    {
        if (ship == Ship.playerShip)
            return;
        NonauthUpdateEquips(weapon1, weapon2, tool);
    }

    [Command(requiresAuthority = false)]
    public void CmdGive(ItemInfo item)
    {
        RpcGive(item);
    }

    [ClientRpc]
    public void RpcGive(ItemInfo item)
    {
        inventory.Add(item);
    }

    public List<int> CapacityBreakPoints = new()
    {
        10, 13, 16, 20, 24, 29, 34, 40, 47, 56, 63, 70, 80, 90, 100

    };
    #endregion
   
}
