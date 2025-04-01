using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRevive : MonoBehaviour
{
    Ship ship;
    [SerializeField] bool triggerIn;
    // Start is called before the first frame update
    void Start()
    {
        ship = GetComponentInParent<Ship>();
    }

    // Update is called once per frame
    void Update()
    {
        if (triggerIn && Ship.playerShip != ship)
        {
            Prompt.SetPrompt("<R> Revive.");
            if (Input.GetKey(KeyCode.R))
            {
                Ship.playerShip.TriggerRevive(ship);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Ship.playerShip == null || Ship.playerShip.gameObject == null || Ship.playerShip == ship)
            return;
        if (collision.gameObject == Ship.playerShip.gameObject)
        {
            triggerIn = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Ship.playerShip == null || Ship.playerShip.gameObject == null)
            return;
        if (collision.gameObject == Ship.playerShip.gameObject)
        {
            triggerIn = false;
        }
    }

    void OnDisable()
    {
        triggerIn = false;
    }
}
