using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class NetworkWriters
{
    public static void WriteList<T>(this NetworkWriter writer, List<T> value)
    {
        writer.Write(value.Count);
        foreach (T t in value)
        {
            writer.Write(t);
        }
    }

    public static List<T> ReadList<T>(this NetworkReader reader)
    {
        List<T> l = new();
        int count = reader.Read<int>();
        for (int i = 0; i < count; i++)
        {
            l.Add(reader.Read<T>());
        }
        return l;
    }

    public static void WriteDictionary<T,U>(this NetworkWriter writer, Dictionary<T,U> value)
    {
        writer.Write(value.Count);
        foreach (var pair in value)
        {
            writer.Write<T>(pair.Key);
            writer.Write<U>(pair.Value);
        }
    }

    public static Dictionary<T,U> ReadDictionary<T, U>(this NetworkReader reader)
    {
        Dictionary<T,U> d = new();
        int count = reader.Read<int>();
        for (int i = 0; i < count; i++)
        {
            d[reader.Read<T>()] = reader.Read<U>();
        }
        return d;
    }

    public static void WriteStatDictionary(this NetworkWriter writer, Dictionary<Stat.StatEnum, int> value)
    {
        writer.WriteDictionary<Stat.StatEnum, int>(value);
    }

    public static Dictionary<Stat.StatEnum, int> ReadStatDictionary(this NetworkReader reader)
    {
        return reader.ReadDictionary<Stat.StatEnum, int>();
    }

    public static void WriteStatEnum(this NetworkWriter writer, Stat.StatEnum value){
        writer.Write((int)value);

    }
    public static Stat.StatEnum ReadStatEnum(this NetworkReader reader)
    {
        return (Stat.StatEnum) reader.ReadInt();
    }

    public static void WriteItem(this NetworkWriter writer, Item value)
    {
        writer.Write<string>(Item.GetID(value));
    }
    public static Item ReadItem(this NetworkReader reader)
    {
        string s = reader.Read<string>();
        return ItemGatherer.GetItem(s);
    }


    public static void WriteItemInfo(this NetworkWriter writer, ItemInfo value)
    {
        if (value == null)
        {
            writer.Write(true);
            return;
        }
        writer.Write(false);
        writer.Write(value.item);
        writer.WriteList<string>(value.modifications);
        writer.Write<int>(value.count);

    }
    public static ItemInfo ReadItemInfo(this NetworkReader reader)
    {
        if (reader.ReadBool())
            return null;

        ItemInfo ii = new();
        ii.item = reader.Read<Item>();
        ii.modifications = reader.ReadList<string>();
        ii.count = reader.Read<int>();
        return ii;
    }
}
