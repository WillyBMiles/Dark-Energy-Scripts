using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Mirror;

public class ShipStats : NetworkBehaviour
{
    public readonly List<StatAdjustment> currentStatAdjustments = new();
    public readonly Dictionary<StatusEffect, GameObject> persistentStatusEffects = new();
    public readonly Dictionary<StatusEffect, (GameObject, float)> simpleStatusEffects = new();

    private readonly List<StatusEffect> tempEffects = new();

    Dictionary<Stat.StatEnum, float> actualDefenses = new() { };


    public float baseCore = 100f;
    public float baseStabilization = 100f;
    public float baseSystems = 100f;

    Ship ship;
    PlayerRPG rpg;
    private void Awake()
    {
        ship = GetComponent<Ship>();
        rpg = GetComponent<PlayerRPG>();
    }

    private void OnEnable()
    {
        ResetManager.instance.Reset += OnReset;
    }
    private void OnDisable()
    {
        ResetManager.instance.Reset -= OnReset;
    }

    private void Update()
    {
        tempEffects.Clear();
        tempEffects.AddRange(simpleStatusEffects.Keys);
        foreach (var se in tempEffects)
        {
            simpleStatusEffects[se] = (simpleStatusEffects[se].Item1, simpleStatusEffects[se].Item2 - Time.deltaTime);
            if (simpleStatusEffects[se].Item2 <= 0)
            {
                RemoveStatusEffect(se);
            }
        }

        actualDefenses.Clear();
        
    }

    public void AddStatusEffect(StatusEffect statusEffect)
    {   
        if (statusEffect.simpleDuration)
        {
            AddSimpleStatusEffect(statusEffect);
        }
        else
        {
            AddPersistentStatusEffect(statusEffect);
        }

    }


    public float GetDefense(Stat.StatEnum stat)
    {
        float baseE = 100f;
        if (actualDefenses.ContainsKey(stat))
        {
            baseE = actualDefenses[stat];
        }
        return ShipStats.AdjustValue(baseE, ship.stats, stat);
    }



    void AddSimpleStatusEffect(StatusEffect statusEffect)
    {
        (GameObject, float) value = (null, statusEffect.duration);
        if (!simpleStatusEffects.ContainsKey(statusEffect))
        {
            currentStatAdjustments.AddRange(statusEffect.statAdjustments); //add stat adjustments
            if (statusEffect.particleEffect != null)
                value = (Instantiate(statusEffect.particleEffect, transform), statusEffect.duration);
        }
        else
        {
            value = (simpleStatusEffects[statusEffect].Item1, statusEffect.duration); //refresh the duration
        }
        simpleStatusEffects[statusEffect] = value;
        
        
    }

    void AddPersistentStatusEffect(StatusEffect statusEffect)
    {
        if (!persistentStatusEffects.ContainsKey(statusEffect))
        {
            currentStatAdjustments.AddRange(statusEffect.statAdjustments);
            persistentStatusEffects[statusEffect] = null;
            if (statusEffect.particleEffect != null)
                persistentStatusEffects[statusEffect] = Instantiate(statusEffect.particleEffect, transform);
        }
    }
    

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        if (simpleStatusEffects.ContainsKey(statusEffect))
        {
            if (simpleStatusEffects[statusEffect].Item1 != null)
            {
                Destroy(simpleStatusEffects[statusEffect].Item1);
            }
            simpleStatusEffects.Remove(statusEffect);
        }
        if (persistentStatusEffects.ContainsKey(statusEffect))
        {
            if (persistentStatusEffects[statusEffect] != null)
            {
                Destroy(persistentStatusEffects[statusEffect]);
            }
            persistentStatusEffects.Remove(statusEffect);
        }

        foreach (StatAdjustment adjustment in statusEffect.statAdjustments)
        {
            currentStatAdjustments.Remove(adjustment);
        }

    }

    public static float AdjustDamageValue(float baseValue, ShipStats source, Stat.StatEnum outgoingStat, ShipStats target, Stat.StatEnum incomingStat)
    {
        float value = baseValue;
        if (source != null)
            value = StatAdjustment.AdjustValue(value, outgoingStat, source.currentStatAdjustments, StatAdjustment.Type.PreAdditive);
        if (target != null)
            value = StatAdjustment.AdjustValue(value, incomingStat, target.currentStatAdjustments, StatAdjustment.Type.PreAdditive);

        if (source != null)
            value = StatAdjustment.AdjustValue(value, outgoingStat, source.currentStatAdjustments, StatAdjustment.Type.Multiplicative);
        if (target != null)
            value = StatAdjustment.AdjustValue(value, incomingStat, target.currentStatAdjustments, StatAdjustment.Type.Multiplicative);

        //MANUAL SUM PERCENTAGE
        float percentSum = 100f;
        if (source != null)
        {
            foreach (StatAdjustment sa in source.currentStatAdjustments)
            {
                if (sa.stat == outgoingStat)
                {
                    percentSum += sa.amount;
                }
            }
        }
        if (target != null)
        {
            foreach (StatAdjustment sa in source.currentStatAdjustments)
            {
                if (sa.stat == outgoingStat)
                {
                    percentSum += sa.amount;
                }
            }
        }
        value = StatAdjustment.AdjustWithPercent(value, percentSum);

        if (source != null)
            value = StatAdjustment.AdjustValue(value, outgoingStat, source.currentStatAdjustments, StatAdjustment.Type.PostAdditive);
        if (target != null)
            value = StatAdjustment.AdjustValue(value, incomingStat, target.currentStatAdjustments, StatAdjustment.Type.PostAdditive);

        return value;
    }



    public static float AdjustValue(float baseValue, ShipStats source, Stat.StatEnum statEnum)
    {
        if (source != null)
            return StatAdjustment.AdjustValue(baseValue, statEnum, source.currentStatAdjustments);
        return baseValue;
    }

    private void OnReset()
    {
        foreach (var statusEffect in simpleStatusEffects.Keys.ToList())
        {
            RemoveStatusEffect(statusEffect);
        }
    }
}
