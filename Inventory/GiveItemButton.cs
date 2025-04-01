using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GiveItemButton : MonoBehaviour
{
    public Ship ship;
    public ItemInfo item;

    [SerializeField]
    TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = $"Give to {ship.name}";
    }
    public void Give()
    {
        PlayerItems pItems = ship.GetComponent<PlayerItems>();
        if (pItems)
        {
            if (pItems.isServer)
                pItems.RpcGive(item);
            else
                pItems.CmdGive(item);
            PlayerItems.localItems.DestroyItem(item);
        }
    }
}
