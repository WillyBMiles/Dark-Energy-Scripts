using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    static SaveManager singleton;
    static string continueSave;

    public const string PLAYER_PREFS_CONTINUE = "CONTINUE_SAVE";
    // Start is called before the first frame update
    void Start()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;

        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey(PLAYER_PREFS_CONTINUE))
        {
            continueSave = PlayerPrefs.GetString(PLAYER_PREFS_CONTINUE);
            ShipSave ss = LoadShip(continueSave);
            if (ss != null)
                LoadShipToGame(ss);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static string DATA_PATH { get { return Application.persistentDataPath + "/Saves/"; } }
    public const string SHIP_FILE_EXTENSION = ".ship";

    public static void SaveCurrentShip()
    {
        Save(ShipSave.currentSave, Ship.playerShip);
    }

    public static void Save(ShipSave save, Ship ship)
    {
        save.Save(ship);
        Save(save);
    }

    public static void Save(ShipSave ship)
    {

        NetworkWriter writer = new();
        writer.Write(ship);
        Directory.CreateDirectory(DATA_PATH);
        FileStream stream = new(ship.fileName, FileMode.Create);
        stream.Write(writer.ToArray(),0,writer.ToArray().Length);
        stream.Close();
    }

    public static ShipSave LoadShip(string path)
    {
        try
        {
            FileStream stream = new($"{path}", FileMode.Open);
            NetworkReader reader = new(ReadFully(stream));
            stream.Close();
            try
            {
                ShipSave save = reader.Read<ShipSave>();

                return save;
            }
            catch
            {
                Debug.LogWarning($"{path} is not a proper shipSave.");
            }
        }
        catch
        {
            Debug.LogWarning($"{path} may not exist.");
        }
        
        return null;
    }

    public static void LoadShipToGame(ShipSave save)
    {
        if (save == null)
            return;
        ShipSave.currentSave = save;
        PlayerPrefs.SetString(PLAYER_PREFS_CONTINUE, save.fileName);
    }

    public static byte[] ReadFully(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }

    public static void LoadAllShips(List<ShipSave> shipsToLoad) 
    {
        shipsToLoad.Clear();

        DirectoryInfo d = new(DATA_PATH); 

        FileInfo[] Files = d.GetFiles($"*{SHIP_FILE_EXTENSION}");

        foreach (FileInfo file in Files)
        {
            ShipSave si = LoadShip(file.FullName);
            if (si != null)
            {
                si.fileName = file.FullName; //just in case user moved or renamed file
                shipsToLoad.Add(si);
            }
            
        }
    }

    public static void Save()
    {
        if (ShipSave.currentSave != null)
            Save(ShipSave.currentSave);
    }
}
