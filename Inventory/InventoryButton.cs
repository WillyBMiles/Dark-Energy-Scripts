using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    static InventoryButton currentOpen;

    public static InventoryButton hoverButton;

    public Image targetImage;

    public Color defaultColor;
    public Color downColor;
    public Color clickColor;
    public Color hoverColor;

    public ItemInfo myItem;
    [SerializeField]
    TextMeshProUGUI text;
    [SerializeField]
    TextMeshProUGUI slotText;
    InventoryManager manager;
    public Tooltip myTooltip;

    public Button EquipLeftButton;
    public Button EquipRightButton;
    public Button EquipToolButton;
    public Button EquipModuleButton;
    public Button UnequipButton;
    public Button DestroyButton;
    public Button SplitButton;
    public Button CombineButton;
    public Image icon;
    public GameObject GiveButtonPrefab;
    public GameObject Canvas;
    List<GameObject> giveItemButtons = new();

    [Tooltip("Set by the player when acting. To prevent animation overload.")]
    public static bool InventoryLockOut { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponentInParent<InventoryManager>();
        OnStart();
    }

    virtual protected void OnStart()
    {

    }
    //Time since last opened
    float openSinceTime = -1f;
    int overrideTime = 0;
    private void Update()
    {
        text.text = myItem.item.displayName;
        if (myItem.item is Tool tool)
        {
            if (tool.contract)
            {
                text.text += $" ({tool.maxUses})";
            }
            else
            {
                text.text +=$" ({myItem.count})";
            }
        }

        if (hoverButton == this)
        {
            if (overrideTime <= 0)
                targetImage.color = hoverColor;
            myTooltip.SetText(myItem.item.GetDescription(PlayerItems.localItems));
        }
        else
        {
            if (overrideTime <= 0)
                targetImage.color = defaultColor;
        }
        overrideTime--;

        if (Canvas != null)
        {
            SplitButton.gameObject.SetActive(false);
            CombineButton.gameObject.SetActive(false);
            if (myItem.item is Weapon)
            {
                EquipLeftButton.gameObject.SetActive(true);
                EquipRightButton.gameObject.SetActive(true);
                EquipToolButton.gameObject.SetActive(false);
                EquipModuleButton.gameObject.SetActive(false);
            }
            if (myItem.item is Tool myTool)
            {
                EquipLeftButton.gameObject.SetActive(false);
                EquipRightButton.gameObject.SetActive(false);
                EquipToolButton.gameObject.SetActive(true);
                EquipModuleButton.gameObject.SetActive(false);
                if (!myTool.contract)
                {
                    if (myItem.count > 1)
                    {
                        SplitButton.gameObject.SetActive(true);
                    }
                    if (PlayerItems.localItems.NumberOfCopies(myItem) > 1)
                    {
                        CombineButton.gameObject.SetActive(true);
                    }
                }
            }
            if (myItem.item is ItemModule)
            {
                EquipLeftButton.gameObject.SetActive(false);
                EquipRightButton.gameObject.SetActive(false);
                EquipToolButton.gameObject.SetActive(false);
                
                if (PlayerItems.localItems.IsModuleEquipped(myItem))
                {
                    UnequipButton.gameObject.SetActive(true);
                    EquipModuleButton.gameObject.SetActive(false);
                }
                else
                {
                    UnequipButton.gameObject.SetActive(false);
                    EquipModuleButton.gameObject.SetActive(true);
                }
                    
            }

            if (!myItem.item.canBeTransferred)
            {
                DestroyButton.gameObject.SetActive(false);
            }
        }
        //End of Canvas stuff


        int? slot = PlayerItems.localItems.IsEquipped(myItem);
        if (!slot.HasValue && myItem.item is Weapon)
        {
            slotText.text = "";
            if (Canvas != null)
                UnequipButton.gameObject.SetActive(false);
        }
        else if (myItem.item is Weapon)
        {
            slotText.text = $"/Install Slot: { (slot == 1 ? "Left" : "Right")}";
            
            if (Canvas != null)
                UnequipButton.gameObject.SetActive(true);
        }
        else if(myItem.item is Tool)
        {
            int? toolSlot = PlayerItems.localItems.GetToolSlot(myItem);
            if (toolSlot.HasValue)
            {
                slotText.text = $"/Install Slot: {toolSlot.Value + 1}";

                if (Canvas != null)
                {
                    EquipToolButton.gameObject.SetActive(false);
                    UnequipButton.gameObject.SetActive(true);
                }
                    
            }
            else
            {
                slotText.text = "";
                if (Canvas != null)
                    UnequipButton.gameObject.SetActive(false);
            }

        }
        else if (myItem.item is ItemModule)
        {
            bool equipped = PlayerItems.localItems.IsModuleEquipped(myItem);
            if (equipped)
            {
                slotText.text = $"/Installed";
            }
            else
                slotText.text = "";
        }

        slotText.text = ((myItem.item is Weapon) ? "Weapon" : "")
            + ((myItem.item is ItemModule) ? "Module" : "")
            + ((myItem.item is Tool t) ? (t.contract ? "Contract" : "Tool") : "") + slotText.text ;


        Item.AssignIcon(myItem, icon, true);

        
    }

    private void LateUpdate()
    {
        if (!Canvas)
            return;
        if (openSinceTime != Time.time && Canvas.activeInHierarchy && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
            HideCanvas();
    }


    private void OnDisable()
    {
        HideCanvas();
        if (hoverButton == this)
            hoverButton = null;
    }

    public void ShowCanvas()
    {

        if (!Canvas.activeInHierarchy)
        {
            if (currentOpen != null)
                return;
            openSinceTime = Time.time;

            currentOpen = this;
            Canvas.SetActive(true);
            foreach (GameObject go in giveItemButtons)
            {
                Destroy(go);
            }
            giveItemButtons.Clear();

            if (myItem.item.canBeTransferred)
            {
                foreach (Ship s in Ship.PlayerShips)
                {
                    if (s == Ship.playerShip)
                        continue;

                    GameObject go = Instantiate(GiveButtonPrefab, Canvas.transform);
                    GiveItemButton gib = go.GetComponent<GiveItemButton>();
                    gib.ship = s;
                    gib.item = myItem;
                    giveItemButtons.Add(go);
                }
            }

        }
        else
            HideCanvas();
    }
    public void HideCanvas()
    {
        if (Canvas == null)
            return;

        Canvas.SetActive(false);
        if (currentOpen == this) {
            currentOpen = null;
        }
    }

    public virtual void OnLeftClick()
    {
        if (myItem.item is Weapon)
            EquipLeft();
        else if (myItem.item is Tool)
            EquipTool();
        else if (myItem.item is ItemModule)
            EquipModule();
    }
    public virtual void OnRightClick()
    {
        if (myItem.item is Weapon)
            EquipRight();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverButton = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverButton == this)
            hoverButton = null;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
            overrideTime = 2;
            targetImage.color = clickColor;
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
            overrideTime = 2;
            targetImage.color = clickColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetImage.color != clickColor)
            targetImage.color = downColor;
        overrideTime = 2;
    }

    public void EquipLeft()
    {
        if (InventoryLockOut)
            return;
        PlayerItems.localItems.EquipItem(myItem, 1);

        HideCanvas();
    }
    public void EquipRight()
    {
        if (InventoryLockOut)
            return;
        PlayerItems.localItems.EquipItem(myItem, 2);

        HideCanvas();
    }

    public void EquipTool()
    {
        if (InventoryLockOut)
            return;
        PlayerItems.localItems.EquipTool(myItem);
        HideCanvas();
    }

    public void EquipModule()
    {
        if (InventoryLockOut)
            return;
        PlayerItems.localItems.EquipModule(myItem);
        HideCanvas();
    }

    public void Split()
    {
        if (InventoryLockOut)
            return;
        PlayerItems.localItems.Split(myItem);
    }

    public void Combine()
    {
        if (InventoryLockOut)
            return;
        PlayerItems.localItems.Combine(myItem);
    }

    public void Unequip()
    {
        if (InventoryLockOut)
            return;
        if (myItem.item is Tool t && t.contract)
        {
            ConfirmPopup.instance.Show($"Are you sure you want to unequip {myItem.item.displayName}? The contract will be cleared until you make repairs at a station.",ConfirmUnequip, DenyUnequip, 10f);
        }else
        {
            PlayerItems.localItems.UnequipItem(myItem);
        }
        if (myItem.item is ItemModule)
        {
            PlayerItems.localItems.UnequipModule(myItem);
        }
        

        HideCanvas();
    }

    public void ConfirmUnequip()
    {
        PlayerItems.localItems.UnequipItem(myItem);
    }

    public void DenyUnequip()
    {
        //pass
    }

    public void Destroy()
    {
        ConfirmPopup.instance.Show($"Are you sure you want to destroy {myItem.item.displayName}?", DestroyYes, DestroyNo, 3f);

        HideCanvas();
    }

    public void DestroyYes()
    {
        if (InventoryLockOut)
            return;

        PlayerItems.localItems.DestroyItem(myItem);
    }

    public void DestroyNo()
    {
        HideCanvas();
    }
}
