using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrap : MonoBehaviour
{
    public static Scrap instance;
    public Interactable interactable;

    public GameObject parent;
    public int amount;

    public AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        interactable.Interact += PickUp;
        interactable.Nearby += Nearby;
    }

    // Update is called once per frame
    void Update()
    {

    }

    float time;
    public void PlaceScrap(Vector3 location, int amount)
    {
        if (time == Time.time)
            return;//Don't call twice in one frame you dingus! (BANDAID)

        transform.position = location;
        this.amount = amount;
        parent.SetActive(true);
        time = Time.time;
    }

    public void Nearby()
    {
        Prompt.SetPrompt("<E> Recover.");
    }
    public void PickUp()
    {
        if (!parent.activeInHierarchy)
            return;
        if (PlayerRPG.localRpg != null)
        {
            source.Play();
            PlayerRPG.localRpg.GiveRewardLocal(amount);
            parent.SetActive(false);
            amount = 0;
        }
            
    }
}
