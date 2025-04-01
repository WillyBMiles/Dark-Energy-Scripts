using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatAdjustment
{
    public enum Type
    {
        PreAdditive,
        SumPercentage, //Sum percentage: Add up all of the percents and then apply that as a multiplier
        Multiplicative,
        PostAdditive
    }

    public Stat.StatEnum stat;
    public Type adjustmentType;
    public float amount;

    public static float AdjustValue(float baseValue, Stat.StatEnum stat, IEnumerable<StatAdjustment> statAdjustments)
    {

        foreach (StatAdjustment sa in statAdjustments)
        {
            if (sa.stat == stat && sa.adjustmentType== Type.PreAdditive)
            {
                baseValue += sa.amount;
            }
        }

        float percentage = 100f;
        foreach (StatAdjustment sa in statAdjustments)
        {
            if (sa.stat == stat && sa.adjustmentType == Type.SumPercentage)
            {
                percentage += sa.amount;
            }
        }
        baseValue = AdjustWithPercent(baseValue, percentage);

        foreach (StatAdjustment sa in statAdjustments)
        {
            if (sa.stat == stat && sa.adjustmentType == Type.Multiplicative)
            {
                baseValue *= sa.amount;
            }
        }


        foreach (StatAdjustment sa in statAdjustments)
        {
            if (sa.stat == stat && sa.adjustmentType == Type.PostAdditive)
            {
                baseValue += sa.amount;
            }
        }

        return baseValue;
    }

    public static float AdjustValue(float baseValue, Stat.StatEnum stat, IEnumerable<StatAdjustment> statAdjustments, Type type )
    {
        float percentage = 100f;
        foreach (StatAdjustment sa in statAdjustments)
        {
            
            if (sa.stat == stat && sa.adjustmentType == type)
            {
                switch (type) {
                    
                    case Type.Multiplicative:
                        baseValue *= sa.amount;
                        break;
                    case Type.PreAdditive:
                    case Type.PostAdditive:
                        baseValue += sa.amount;
                        break;
                    case Type.SumPercentage:
                        percentage += sa.amount;
                        break;
                }
            }
            baseValue = AdjustWithPercent(baseValue, percentage);
        }

        return baseValue;
    }

    public static float AdjustWithPercent(float baseValue, float percentSum)
    {
        if (percentSum >= 100f)
            return baseValue * percentSum / 100f;
        float totalSum = -(percentSum - 100f);
        return baseValue * (100 / (100 + totalSum));
    }
}