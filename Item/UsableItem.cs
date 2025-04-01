using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public enum HitType
{
    light, heavy, sustain
}

[CreateAssetMenu(menuName = "Weapon")]
public abstract class UsableItem : Item
{

    [Tooltip("Damage shown in menus. Leave as -1 to match light damage")]
    public float repDamage = -1;
    [Tooltip("Damage done with light attack. Leave as -1 to match rep damage")]
    public float lightDamage = -1;
    [Tooltip("Additional damage multiplier for heavy. Leave as 1 to match light damage")]
    public float heavyMult = 1.5f;
    [Tooltip("Damage done per tick on sustain damage. Leave as -1 to match light damage")]
    public float sustainDamage = -1;

    [Tooltip("Conversion of damage to Power.")]
    public float blockEfficiency = -1f;
    [Header("Main scalings should be a number from 0-100, usually summing to 100. \n Most common scalings are Plasma, Energy, and Quantum ")]
    public Dictionary<Stat.StatEnum, float> scalings = new();

    [Tooltip("If null use basic scalings")]
    public Dictionary<Stat.StatEnum, float> blockScalings = null;

    [Tooltip("If null use basic scalings")]
    public Dictionary<Stat.StatEnum, float> heavyScalings = null;
    [Tooltip("If null use basic scalings")]
    public Dictionary<Stat.StatEnum, float> sustainScalings = null;

    [Header("use Sparingly")]
    public Dictionary<Stat.StatEnum, int> minStats = new();

    public AnimationClip lightClip;
    public AnimationClip heavyClip;
    [Space(20)]
    public AnimationClip sustainIn;
    public AnimationClip sustainOut;
    public AnimationClip sustainSustain;

    public int lightPower = 1;
    public int heavyPower = 3;
    public int sustainPower = 0;

    [Space(20)]
    public AudioClip lightUseSound;
    public AudioClip heavyUseSound;
    public AudioClip sustainUseSound;
    public AudioClip sustainLoopSound;

    public virtual void Use(PlayerItems items, ItemInfo info, HitType type)
    {
        items.ship.CostPower((type switch 
        {
            HitType.heavy => heavyPower,
            HitType.light => lightPower,
            HitType.sustain => sustainPower,
            _ => 0,
        }));
    }

    public virtual void SustainInStart(PlayerItems items, ItemInfo info)
    { }

    public virtual void SustainOutEnd(PlayerItems items, ItemInfo info)
    { }

    public virtual void FinishUse(PlayerItems items, ItemInfo info)
    { }

    public float GetDamage(PlayerRPG rpg, HitType type)
    {
        float dmg = GetBaseDamage(rpg, type);
        dmg += GetAdditionalDamage(rpg, type);

        if (type == HitType.heavy)
        {
            dmg *= heavyMult;
        }
        return dmg;
    }
    public float GetBaseDamage(PlayerRPG rpg, HitType type)
    {
        float baseDmg = type switch
        {
            HitType.heavy => lightDamage,
            HitType.light => lightDamage,
            HitType.sustain => sustainDamage,
            _ => lightDamage,
        };
        if (baseDmg == -1)
        {
            baseDmg = lightDamage;
        }
        if (baseDmg == -1)
        {
            baseDmg = repDamage;
        }
        return baseDmg;
    }


    public float GetAdditionalDamage(PlayerRPG rpg, HitType hitType = HitType.light)
    {
        var scales = scalings;
        if (hitType == HitType.heavy && heavyScalings != null)
            scales = scalings;
        if (hitType == HitType.sustain && sustainScalings != null)
            scales = sustainScalings;


        float addDamage = 0;
        foreach (var pair in scales)
        {
            int lvl = rpg.GetStat(pair.Key);
            addDamage += PlayerRPG.GetBonusBasedOnCutoffs(lvl, weaponCutoffs) * (pair.Value / 100f);
        }
        if (!HasMinStats(rpg))
        {
            addDamage -= GetBaseDamage(rpg, hitType) * .75f;
        }

        return addDamage;
    }

    /*
     Each level is a soft cap. 
    The float is how much additional damage you can deal, with 100% scaling,
    when you reach that level, compared to the previous level.

    With multiple scalings it will scale less hard.

    So 
    (10, .2f)
    (25, 1.3f)
    means at level 10 you'll deal .2 extra damage
    and level 25 you'll deal 1.5 extra damage (.2+1.3)
    At level 20 you'll do somewhere between .2 and 1.5 extra damage
    (all of this assumes 100% scaling)
     
    Damage shown in game is 100x bigger
     */
    static List<(int, float)> weaponCutoffs = new List<(int, float)>()
    {
        ( 10, 0f ),
        ( 25, 2.5f ),
        ( 40, 2f),
        ( 60, 1f ),
        ( 100, 1f ),
    };

    static List<(int, float)> shieldCutoffs = new List<(int, float)>()
    {
        ( 10, 0f ),
        ( 25, 1f ),
        ( 40, .8f),
        ( 60, .6f ),
        ( 100, .6f ),
    };



    public override string GetDescription(PlayerItems playerItems)
    {

        PlayerRPG playerRpg = playerItems.GetComponent<PlayerRPG>();

        float b = repDamage;
        if (b == -1)
        {
            b = lightDamage;
        }
        float a = GetAdditionalDamage(playerRpg);
        string s = $"<b>{displayName}</b>\n{extraInfo}";

        if (b > 0)
        {
            s += $"\nDamage: {(int)(b * 100f)} + {(int)(a * 100f)} = {(int)((a + b) * 100f)}";
        }

        float BE = blockEfficiency;
        float BA = GetAdditionalBlockEfficiency(playerRpg);

        if (blockEfficiency != -1)
        {
            s += $"\nShield Efficiency: {(int)(BE * 100f)} + {(int)(BA * 100f)} = {(int)((BA + BE) * 100f)}";
        }

        if (scalings.Count > 0)
        {
            s += "\n<i>Scaling</i>\n";
        }
        foreach (var pair in scalings)
        {
            s += $"{Stat.allStats[pair.Key].id}: {(int)pair.Value} |";
        }
        if (scalings.Count > 0)
        {
            s = s[0..^1];
        }

        if (blockScalings != null && blockScalings.Count > 0)
        {
            s += "\n<i>Block: </i>";
            foreach (var pair in blockScalings)
            {
                s += $"{Stat.allStats[pair.Key].id}: {(int)pair.Value} |";
            }
            s = s[0..^1];
        }


        if (heavyScalings != null && heavyScalings.Count > 0)
        {
            s += "\n<i>Heavy: </i>";
            foreach (var pair in heavyScalings)
            {
                s += $"{Stat.allStats[pair.Key].id}: {(int)pair.Value} |";
            }
            s = s[0..^1];
        }

        if (sustainScalings != null && sustainScalings.Count > 0)
        {
            s += "\n<i>Sustain: </i>";
            foreach (var pair in sustainScalings)
            {
                s += $"{Stat.allStats[pair.Key].id}: {(int)pair.Value} |";
            }
            s = s[0..^1];
        }

        return s;
    }

    public float GetBlockEfficiency(PlayerRPG playerRPG)
    {
        return blockEfficiency + GetAdditionalBlockEfficiency(playerRPG);
    }
    public float GetAdditionalBlockEfficiency(PlayerRPG playerRPG)
    {
        float addDamage = 0;
        var scales = blockScalings == null ? scalings : blockScalings;

        foreach (var pair in scales)
        {
            int lvl = playerRPG.GetStat(pair.Key);
            addDamage += PlayerRPG.GetBonusBasedOnCutoffs(lvl, shieldCutoffs) * (pair.Value / 100f);
        }
        return addDamage;
    }

    public bool HasMinStats(PlayerRPG playerRPG)
    {
        foreach (var pair in minStats)
        {
            if (playerRPG.stats[pair.Key] < pair.Value)
                return false;
        }
        return true;
    }
}


