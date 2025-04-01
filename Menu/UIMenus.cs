using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenus : MonoBehaviour
{
    public GameObject StationMenu;
    public GameObject InventoryMenu;
    public GameObject EquipmentMenu;

    public static bool InventoryOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        InventoryMenu.SetActive(false);
        EquipmentMenu.SetActive(false);
        InventoryOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (InventoryMenu.activeInHierarchy)
            {
                InventoryMenu.SetActive(false);
                EquipmentMenu.SetActive(true);
            }
            else if (EquipmentMenu.activeInHierarchy)
            {
                EquipmentMenu.SetActive(false);
            }
            else
            {
                InventoryMenu.SetActive(true);
            }
            
            InventoryOpen = InventoryMenu.activeInHierarchy || EquipmentMenu.activeInHierarchy;
        }
        StationMenu.SetActive(Station.nearbyStation != null);
    }
}
