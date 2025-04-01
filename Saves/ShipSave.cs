using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public static class ShipSaveSerializer
{
    public static void WriteShipSave(this NetworkWriter writer, ShipSave value)
    {
        if (value == null)
        {
            writer.Write<bool>(true);
            return;
        }
            
        writer.Write<bool>(false);

        writer.Write(value.fileName);
        writer.Write(value.name);
        writer.Write(value.color);

        writer.Write(value.health);
        writer.Write(value.currentLocation);


        writer.Write(value.level);
        writer.Write(value.currency);
        writer.WriteDictionary<Stat.StatEnum, int>(value.stats);


        writer.WriteList<ItemInfo>(value.inventory);
        if (value.Weapon1 == null || !value.inventory.Contains(value.Weapon1))
            writer.WriteInt(-1);
        else
            writer.WriteInt(value.inventory.IndexOf(value.Weapon1));

        if (value.Weapon2 == null || !value.inventory.Contains(value.Weapon2))
            writer.WriteInt(-1);
        else
            writer.WriteInt(value.inventory.IndexOf(value.Weapon2));

        List<int> outputList = new();
        ShipSave.ConvertItemList(value.tools, value.inventory, outputList);
        writer.WriteList<int>(outputList);
        ShipSave.ConvertItemList(value.modules, value.inventory, outputList);
        writer.WriteList<int>(outputList);

        writer.WriteList<string>(value.pickups);
        writer.WriteList<string>(value.bossesBeaten);
        writer.WriteList<string>(value.worldEvents);
        

        writer.WriteBool(value.droppedScrap);
        writer.WriteVector3(value.scrapLocation);
        writer.WriteInt(value.scrapAmount);

        writer.Write(value.station);
    }

    public static ShipSave ReadShipSave(this NetworkReader reader)
    {
        if (reader.Read<bool>())
            return null;

        ShipSave save = new();
        save.fileName = reader.Read<string>();
        save.name = reader.Read<string>();
        save.color = reader.Read<Color>();

        save.health = reader.Read<float>();
        save.currentLocation = reader.Read<Vector2>();

        save.level = reader.Read<int>();
        save.currency = reader.Read<int>();
        save.stats = reader.ReadDictionary<Stat.StatEnum, int>();


        save.inventory = reader.ReadList<ItemInfo>();

        int w1 = reader.Read<int>();
        if (w1 != -1)
        {
            save.Weapon1 = save.inventory[w1];
        }
        int w2 = reader.Read<int>();
        if (w2 != -1)
        {
            save.Weapon2 = save.inventory[w2];
        }

        ShipSave.ConvertItemList(reader.ReadList<int>(), save.inventory, save.tools);
        ShipSave.ConvertItemList(reader.ReadList<int>(), save.inventory, save.modules);

        save.pickups = reader.ReadList<string>();
        save.bossesBeaten = reader.ReadList<string>();
        save.worldEvents = reader.ReadList<string>();

        save.droppedScrap = reader.ReadBool();
        save.scrapLocation = reader.ReadVector3();
        save.scrapAmount = reader.ReadInt();

        save.station = reader.ReadInt();
        return save;
    }
}

public class ShipSave 
{
    public static ShipSave currentSave;

    public string fileName = "FileName";

    public string name = "Name";
    public Color color = Color.white;

    public float health;
    public Vector2 currentLocation;

    //RPG STUFF
    public int level = 1;
    public int currency = 0;
    public Dictionary<Stat.StatEnum, int> stats = new();

    //ITEM_STUFF
    public ItemInfo Weapon1;
    public ItemInfo Weapon2;
    public List<ItemInfo> inventory = new();
    public List<ItemInfo> tools = new();
    public List<ItemInfo> modules = new();
    public List<int> uses = new();

    //WORLD STUFF -> TODO
    public List<string> pickups = new(); //All pickups are LOCAL ONLY (just like Dark Souls lol)
    public List<string> bossesBeaten = new(); //includes any non-respawning enemies
    public List<string> worldEvents = new(); //Like doors

    public bool droppedScrap = false;
    public Vector3 scrapLocation = new Vector3();
    public int scrapAmount = 0;

    public int station;

    public static void ConvertItemList(List<int> itemIds, List<ItemInfo> inventory, List<ItemInfo> output)
    {
        output.Clear();
        foreach (int id in itemIds)
        {
            if (inventory.ContainsIndex(id))
            {
                output.Add(inventory[id]);
            }
            if (id < 0)
            {
                output.Add(null);
            }
        }
    }

    public static void ConvertItemList(List<ItemInfo> items, List<ItemInfo> inventory, List<int> output)
    {
        output.Clear();
        foreach (ItemInfo item in items)
        {
            if (inventory.Contains(item))
            {
                output.Add(inventory.IndexOf(item));
            }
            else
            {
                output.Add(-1);
            }
        }
    }

    public ShipSave()
    {
        
    }

    //loaded for all players
    public void Load(Ship ship, bool isPlayer)
    {
        PlayerMovement player = ship.GetComponent<PlayerMovement>();
        player.Recolor(color);

        ship.Prestart();
        ship.name = name;
        ship.health = health;

        if (isPlayer) //failsafe for spawning with 0 hp
        {
            Ship.playerShip = ship;
            if (ship.health <= 0f)
            {
                ship.transform.position = currentLocation;
                ship.DealDamage(1f, Vector3.zero, Vector3.zero, 0f, ship, Damage.Type.Physical, HitType.light, Damage.DamageTag.None);
            }
        }
            
        //ship.transform.position = currentLocation; //Only if host?


        PlayerRPG prpg = ship.GetComponent<PlayerRPG>();
        prpg.stats = stats;
        prpg.currency = currency;
        prpg.level = level;
        if (isPlayer)
            prpg.UpdateStats();

        PlayerItems pitems = ship.GetComponent<PlayerItems>();
        pitems.tools.Clear();
        pitems.tools.AddRange(tools);
        if (pitems.tools.Count > 0)
            pitems.EquipItem(pitems.tools[0], 0);

        pitems.inventory.Clear();
        pitems.inventory.AddRange(inventory);
        pitems.weapon1 = Weapon1;
        pitems.EquipItem(Weapon1, 1);
        pitems.weapon2 = Weapon2;
        pitems.EquipItem(Weapon2, 2);

        pitems.GetModuleCapacity();
        int i = 0;
        foreach (ItemInfo m in modules)
        {
            pitems.EquipModule(m, i);
            i++;
        }

        pitems.HasInitialized = true;

        if (ship.isServer && Ship.playerShip == ship)
            Station.currentStation = station;

        if (ship.isServer && ship.health > 0f)
        {
            ship.transform.position = Station.GetSpawnPosition();
            ship.GetComponent<NetworkRigidbodyReliable2D>().RpcTeleport(Station.GetSpawnPosition());
            //ship.networkTransform.RpcTeleport(Station.GetSpawnPosition());
        }
            

        if (isPlayer && droppedScrap)
        {
            Scrap.instance.PlaceScrap(scrapLocation, scrapAmount);
        }
    }

    public void Save(Ship ship)
    {
        
        PlayerRPG prpg = ship.GetComponent<PlayerRPG>();
        level = prpg.level;
        stats = prpg.stats;
        currency = prpg.currency;

        health = ship.health;
        currentLocation = ship.transform.position;

        PlayerItems pitems = ship.GetComponent<PlayerItems>();
        tools.Clear();
        tools.AddRange(pitems.tools);

        modules.Clear();
        modules.AddRange(pitems.modules);

        inventory.Clear();
        inventory.AddRange(pitems.inventory);
        Weapon1 = pitems.weapon1;
        Weapon2 = pitems.weapon2;

        droppedScrap = Scrap.instance.parent.activeInHierarchy;
        scrapLocation = Scrap.instance.transform.position;
        scrapAmount = Scrap.instance.amount;

        station = Station.currentStation;
    }
}
