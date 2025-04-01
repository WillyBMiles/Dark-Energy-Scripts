using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    Ship ship;
    public List<Projectile> projectiles = new();
    public List<Transform> spawnLocations = new();

    bool lastAnimationTrigger0;
    public bool animationTrigger0;
    bool lastAnimationTrigger1;
    public bool animationTrigger1;
    bool lastAnimationTrigger2;
    public bool animationTrigger2;
    bool lastAnimationTrigger3;
    public bool animationTrigger3;
    bool lastAnimationTrigger4;
    public bool animationTrigger4;
    bool lastAnimationTrigger5;
    public bool animationTrigger5;

    PlayerItems pItems;
    // Start is called before the first frame update
    void Start()
    {
        ship = GetComponentInParent<Ship>();
        pItems = GetComponentInParent<PlayerItems>();
    }

    private void Update()
    {
        if (animationTrigger0 && !lastAnimationTrigger0)
        {
            SpawnProjectile(0);
        }
        lastAnimationTrigger0 = animationTrigger0;

        if (animationTrigger1 && !lastAnimationTrigger1)
        {
            SpawnProjectile(1);
        }
        lastAnimationTrigger1 = animationTrigger1;

        if (animationTrigger2 && !lastAnimationTrigger2)
        {
            SpawnProjectile(2);
        }
        lastAnimationTrigger2 = animationTrigger2;

        if (animationTrigger3 && !lastAnimationTrigger3)
        {
            SpawnProjectile(3);
        }
        lastAnimationTrigger3 = animationTrigger3;

        if (animationTrigger4 && !lastAnimationTrigger4)
        {
            SpawnProjectile(4);
        }
        lastAnimationTrigger4 = animationTrigger4;

        if (animationTrigger5 && !lastAnimationTrigger5)
        {
            SpawnProjectile(5);
        }
        lastAnimationTrigger5 = animationTrigger5;


    }

    public void SpawnProjectile(int index)
    {
        Transform spawn = spawnLocations[index];
        Projectile p = Instantiate(projectiles[index], spawn.position, spawn.rotation);
        if (p.useItemDamage && pItems != null)
        {
            p.damage = pItems.GetLastWeaponDamage(p.hitType);
        }
        p.source = ship;
    }
}
