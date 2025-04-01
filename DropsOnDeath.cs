using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropsOnDeath : MonoBehaviour
{
    static float badluckChance; //prevents too much badluck. Potentially exploitable. That's ok with me tbh.

    Ship ship;
    public GameObject prefab;
    public float chance = .1f;

    

    // Start is called before the first frame update
    void Start()
    {
        ship = GetComponent<Ship>();
        ship.DeathEvent += OnKill;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnKill()
    {
        float actualChance = ShipStats.AdjustDamageValue(chance, ship.stats, Stat.StatEnum.DropChancePercent, Ship.playerShip.stats, Stat.StatEnum.DropChancePercentEarned);

        if (actualChance > Random.value || badluckChance > 1f)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
            badluckChance = 0f;
        } else
        {
            badluckChance += chance;
        }
    }
}
