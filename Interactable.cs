using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    static Interactable currentInteractable;

    public delegate void InteractDelegate();
    public InteractDelegate Interact;
    public InteractDelegate Nearby;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Ship.playerShip == null || Ship.playerShip.playerDisabled)
            return;

        if ( currentInteractable == this && Vector2.Distance(transform.position, Ship.playerShip.transform.position) < 1f)
        {
            DoNearby();
            if (Input.GetButtonDown("Interact"))
                DoInteract();
        }


        if (Vector2.Distance(transform.position, Ship.playerShip.transform.position) < 1f)
        {
            if (currentInteractable == null || !currentInteractable.gameObject.activeInHierarchy)
                currentInteractable = this;
        }
        else
        {
            if (currentInteractable == this)
                currentInteractable = null;
        }
    }
    private void OnDisable()
    {
        if (currentInteractable == this)
            currentInteractable = null;
    }

    void DoInteract()
    {
        Interact?.Invoke();
    }

    void DoNearby()
    {
        Nearby?.Invoke();
    }
}
