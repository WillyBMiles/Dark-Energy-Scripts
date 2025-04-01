using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ItemGatherer : SerializedMonoBehaviour
{
    static Dictionary<string, Item> allItems = new();


    [SerializeField]
    Dictionary<string, Item> _allItems = new();

    // Start is called before the first frame update
    void Awake()
    {
        if (allItems.Count == 0)
            allItems = _allItems;
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    void Gather()
    {
#if UNITY_EDITOR
        _allItems.Clear();
        foreach (Item i in Resources.FindObjectsOfTypeAll<Item>())
        {
            _allItems.Add(Item.GetID(i), i);
        }
#endif
    }
    public static Item GetItem(string id)
    {
        if (allItems.ContainsKey(id))
            return allItems[id];
        return null;
    }

}
