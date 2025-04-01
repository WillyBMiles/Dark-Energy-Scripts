using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions 
{
    public static bool ContainsIndex<T>(this List<T> list, int index)
    {
        return index < list.Count && index >= 0;
    }

    public static bool MemberwiseCompare<T>(this List<T> list, List<T> compareTo)
    {
        if (list.Count != compareTo.Count)
            return false;

        for (int i = 0; i <list.Count; i++)
        {
            if (list[i].Equals(compareTo[i]))
                return false;
        }
        return true;
    }
}
