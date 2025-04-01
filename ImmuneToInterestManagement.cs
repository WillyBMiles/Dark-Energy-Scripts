using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmuneToInterestManagement : MonoBehaviour
{
    [SerializeField]
    [Header("This just makes the object follow the player.")]
    bool _;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Ship.playerShip != null)
        {
            transform.position = Ship.playerShip.transform.position;
        }
    }
}
