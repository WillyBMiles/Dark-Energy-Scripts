using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class ItemDrop : MonoBehaviour
{
    Interactable interactable;
    public List<Item> items = new();
    [Tooltip("Set to - to reset")]
    public string id = "-";

    public AudioClip clip;


    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.Interact += Pickup;
        interactable.Nearby += Nearby;

    }


    private void OnDestroy()
    {
        ResetManager.instance.Reset -= CleanUp;
    }

    [Button]
    public void SetAllIDs()
    {
        foreach (ItemDrop drop in FindObjectsByType<ItemDrop>(FindObjectsSortMode.InstanceID))
        {
            drop.SetID();
        }
    }

    void SetID()
    {
        if (id == "-")
        {
            id = Random.value.ToString() + Random.value.ToString() + Random.value.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (id == "-" && ResetManager.instance)
        {
            ResetManager.instance.Reset += CleanUp;
            id = ".";
        }

        if (ShipSave.currentSave != null && ShipSave.currentSave.pickups.Contains(id) && id != "." && id != "-")
            Destroy(gameObject);
        if (Ship.playerShip == null)
            return;

    }

    public void Nearby()
    {
        Prompt.SetPrompt("<E> Pickup item.");
    }

    public void Pickup()
    {
        if (Ship.playerShip == null || Ship.playerShip.playerDisabled)
            return;
        PlayerItems playerItems = Ship.playerShip.GetComponent<PlayerItems>();
        playerItems.Pickup(items);
        if (id != "-" && id != ".")
            ShipSave.currentSave.pickups.Add(id);
        CleanUp();
        AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    public void CleanUp()
    {
        if (gameObject != null)
            Destroy(gameObject);
    }
}
