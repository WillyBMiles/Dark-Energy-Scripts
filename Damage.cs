using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class Damage 
{
    public enum Type
    {
        Physical,

        Energy,
        Plasma,
        Quantum,
        DarkEnergy
    }
    
    [System.Flags]
    public enum DamageTag
    {
        None = 0,
        Missile = 1,
    }

    public static bool FitsTag(DamageTag testTag, DamageTag testAgainstTag, bool mustContainAll = true)
    {
        if (mustContainAll)
            return (testTag & testAgainstTag) == testAgainstTag;
        return (testTag | testAgainstTag) != 0;
    }
}
