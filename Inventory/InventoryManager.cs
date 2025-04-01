using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Transform inventoryParent;
    public Tooltip tooltip;
    public GameObject inventoryPrefab;
    public List<GameObject> currentInventory = new();

    public List<ItemInfo> lastInventory = new();

    Filter lastFilter = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        GenerateInventory(lastFilter);
    }

    public delegate bool Filter(ItemInfo itemInfo);
    public void GenerateInventory(Filter filter = null)
    {
        if (PlayerItems.localItems == null)
            return;

        if (lastInventory.Count == PlayerItems.localItems.inventory.Count && lastFilter == filter)
        {
            bool difference = false;
            for (int i = 0; i < PlayerItems.localItems.inventory.Count; i++)
            {
                if (PlayerItems.localItems.inventory[i] != lastInventory[i])
                {
                    difference = true;
                    break;
                }
            }
            if (!difference)
                return; //they're the same;
        }
        lastFilter = filter;

        for (int i = currentInventory.Count -1; i >= 0; i--)
        {
            Destroy(currentInventory[i]);
        }
        currentInventory.Clear();

        foreach (ItemInfo ii in PlayerItems.localItems.inventory)
        {
            if (filter != null && !filter(ii))
            {
                continue;
            }
            InventoryButton ib = Instantiate(inventoryPrefab, inventoryParent).GetComponent<InventoryButton>();
            ib.myItem = ii;
            ib.myTooltip = tooltip;
            currentInventory.Add(ib.gameObject);
        }

        lastInventory.Clear();
        lastInventory.AddRange(PlayerItems.localItems.inventory);
    }
}
