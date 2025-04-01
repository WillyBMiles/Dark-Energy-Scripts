using Mirror;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/**/


public class PlayerRPG : SerializedMonoBehaviour
{
    public int level = 1;

    public Dictionary<Stat.StatEnum, int> stats = new();

    static List<PlayerRPG> playerRPGs = new();
    public int currency;

    public float startingHealth;
    public float startingPower;
    public float startingTargetting = 100f;
    public static PlayerRPG localRpg;




    Ship ship;
    // Start is called before the first frame update
    void Start()
    {
        PreStart();
    }

    void PreStart()
    {
        if (stats.Count == 0)
        {
            foreach (Stat.StatEnum s in Stat.allStats.Keys)
            {
                if (s != Stat.StatEnum.DarkEnergy)
                    stats[s] = Stat.allStats[s].startingValue;
            }
        }
        if (ship == null)
        {
            ship = GetComponent<Ship>();
            playerRPGs.Add(this);
        }
    }

    private void OnDestroy()
    {
        playerRPGs.Remove(this);
    }

    public void ImproveStat(Stat.StatEnum stat)
    {
        stats[stat]++;
        level++;
    }
    public void Cost(int currencyCost)
    {
        currency -= currencyCost;
    }

    // Update is called once per frame
    void Update()
    {
        if (ship == Ship.playerShip)
            localRpg = this;
        UpdateStats();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
            GiveReward(1000);
#endif
    }

    public static void GiveReward(int amount)
    {
        foreach (PlayerRPG prpg in playerRPGs)
        {
            prpg.GiveRewardLocal(amount);
        }
        SaveManager.SaveCurrentShip();
    }
    public void GiveRewardLocal(int amount)
    {
        currency += amount;
    }

    const int STARTING_COST = 100;
    const float POWER = 1.1f;
    public static int GetCost(int level)
    {
        return (int) (STARTING_COST * Mathf.Pow(POWER, level - 1));
    }

    public int AdjustStat(Stat.StatEnum stat)
    {
        return (int) ShipStats.AdjustValue(stats[stat], ship.stats, stat);
    }

    public int GetStat(Stat.StatEnum stat)
    {
        if (!stats.ContainsKey(stat))
            return 0;
        return AdjustStat(stat);
    }

    public int GetBaseStat(Stat.StatEnum stat)
    {
        if (!stats.ContainsKey(stat))
            return 0;
        return stats[stat];
    }

    List<(int, float)> hullCutoffs = new()
    {
        ( 10, 0f ),
        ( 25, 10f ),
        ( 40, 8f),
        ( 60, 6f),
        ( 80, 4f),
        (100, 2f),
    };
    List<(int, float)> powerCutoffs = new()
    {
        ( 10, 0f ),
        ( 25, 4f ),
        ( 40, 3f),
        ( 60, 2f),
        ( 80, 1f),
        (100, 1f),
    };

    List<(int, float)> targettingCutoffs = new()
    {
        ( 10, 0f ),
        ( 25, 50f ),
        ( 40, 40f ),
        ( 60, 30f ),
        ( 80, 20f ),
        (100, 10f ),
    };

    public void UpdateStats()
    {
        PreStart();
        ship.maxHealth = startingHealth + GetBonusBasedOnCutoffs(AdjustStat(Stat.StatEnum.Hull), hullCutoffs);
        ship.maxPower = startingPower + GetBonusBasedOnCutoffs(AdjustStat(Stat.StatEnum.Power), powerCutoffs);
    }




    public static float GetBonusBasedOnCutoffs(int level, List<(int, float)> cutoffs)
    {
        float sum = 0f;
        int lastLvl = 0;
        foreach ((int lvl, float add) in cutoffs)
        {
            if (lvl < level)
            {
                sum += add;
            }
            else
            {
                return sum + Mathf.Lerp(0f, add, ((float)(level - lastLvl)) / (lvl - lastLvl));
            }
            lastLvl = lvl;
        }
        return sum;
    }

}
