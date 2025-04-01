using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTowardsReviving : MonoBehaviour
{
    Ship ship;
    // Start is called before the first frame update
    void Start()
    {
        ship = GetComponentInParent<Ship>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ship != null && ship.shipToRevive != null)
            transform.forward = ship.shipToRevive.transform.position - transform.position;
    }
}
