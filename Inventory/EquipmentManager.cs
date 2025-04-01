using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    public EquipmentButton leftButton;
    public EquipmentButton rightButton;

    public GameObject toolButtonPrefab;
    public Transform toolParent;
    public List<EquipmentButton> toolButtons;
    public List<EquipmentButton> moduleButtons;

    EquipmentButton currentButton;
    public InventoryManager inventoryManager;

    [Space(20)]
    public GameObject moduleButtonPrefab;
    public Transform moduleParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    int lastCapacity = 0;
    int lastModuleCapacity;
    // Update is called once per frame
    void Update()
    {
        if (PlayerItems.localItems == null)
            return;

        if (lastCapacity != PlayerItems.localItems.MaxToolCapacity() || lastModuleCapacity != PlayerItems.localItems.GetModuleCapacity())
        {
            UpdateEquipment();
            lastCapacity = PlayerItems.localItems.tools.Count;
            lastModuleCapacity = PlayerItems.localItems.modules.Count;
        }
    }

    private void OnEnable()
    {
        UpdateEquipment();
        inventoryManager.gameObject.SetActive(false);
        currentButton = null;
    }

    public void UpdateEquipment()
    {
        if (PlayerItems.localItems == null)
            return;

        leftButton.SetButton(PlayerItems.localItems.weapon1);
        rightButton.SetButton(PlayerItems.localItems.weapon2);

        foreach (EquipmentButton eb in toolButtons)
        {
            Destroy(eb.gameObject);
        }
        toolButtons.Clear();
        for (int j = 0; j < PlayerItems.localItems.MaxToolCapacity(); j++)
        {
            EquipmentButton button = Instantiate(toolButtonPrefab, toolParent).GetComponent<EquipmentButton>();
            button.slotNumber = j;
            toolButtons.Add(button);
        }


        foreach (EquipmentButton eb in moduleButtons)
        {
            Destroy(eb.gameObject);
        }
        moduleButtons.Clear();
        for (int j = 0; j < PlayerItems.localItems.GetModuleCapacity(); j++)
        {
            EquipmentButton button = Instantiate(moduleButtonPrefab, moduleParent).GetComponent<EquipmentButton>();
            button.slotNumber = j;
            moduleButtons.Add(button);
        }


        int i = 0;
        for (; i < PlayerItems.localItems.tools.Count; i++)
        {
            toolButtons[i].SetButton(PlayerItems.localItems.tools[i]);
        }
        for (; i < toolButtons.Count; i++) {
            toolButtons[i].SetButton(null);
        }

        i = 0;
        for (; i < PlayerItems.localItems.modules.Count; i++)
        {
            moduleButtons[i].SetButton(PlayerItems.localItems.modules[i]);
        }
        for (; i < moduleButtons.Count; i++)
        {
            moduleButtons[i].SetButton(null);
        }
    }


    public void PressButton(EquipmentButton button, InventoryManager.Filter filter)
    {
        inventoryManager.gameObject.SetActive(true);
        inventoryManager.GenerateInventory(filter);
        currentButton = button;
    }

    public void Equip(ItemInfo itemInfo)
    {
        if (currentButton == null)
            return;
        currentButton.Equip(itemInfo);
        currentButton = null;
        inventoryManager.gameObject.SetActive(false);
        UpdateEquipment();
    }

    public void ChooseItem(ItemInfo item)
    {
        if (currentButton != null)
        {
            currentButton.SetButton(item);
        }
    }
}
