using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{

    public static Station nearbyStation { get; private set; }
    public static int  currentStation { get; set; } = -1;
    public Transform spawnLocation;

    [ReadOnly]
    public int id = -1;

    public bool starting = false;
    public bool resetStationId = false;


    // Start is called before the first frame update
    void Start()
    {
        if (starting && currentStation == -1) {
            currentStation = id;
        }
    }

    public static Vector3 GetSpawnPosition()
    {
        foreach (Station s in FindObjectsByType<Station>(FindObjectsSortMode.InstanceID))
        {
            if (s.id == currentStation)
            {
                return s.spawnLocation.transform.position;
            }
        }
        Debug.LogWarning("No set spawn location!");
        return new Vector3(0, 0, 0);
    }

    public static void Rest()
    {
        if (nearbyStation == null)
            return;
        if (Ship.playerShip != null)
        {
            NetworkController.instance.TryRestart(nearbyStation.id);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

    public float stationDistance;
    private void Update()
    {
        if (Ship.playerShip == null)
        {
            nearbyStation = null;
            return;
        }
        if (Vector3.Distance(transform.position, Ship.playerShip.transform.position) < stationDistance)
        {
            nearbyStation = this;
        }
        else if (nearbyStation == this)
            nearbyStation = null;
    }

    [Button]
    public void SetIds()
    {
#if UNITY_EDITOR
        List<int> usedIDs = new();

        foreach (Station s in FindObjectsByType<Station>(FindObjectsSortMode.InstanceID))
        {
            if (s.resetStationId)
            {
                s.id = -1;
                s.resetStationId = false;
            }
            if (s.starting)
            {
                s.id = 1;
                usedIDs.Add(s.id);
                UnityEditor.EditorUtility.SetDirty(s.gameObject);
                UnityEditor.EditorUtility.SetDirty(s);
            }
            if (s.id != -1)
            {
                usedIDs.Add(s.id);
            }
        }

        int startID = 0;
        foreach (Station s in FindObjectsByType<Station>(FindObjectsSortMode.InstanceID))
        {
            if (s.starting)
            {
                continue;
            }
            if (s.id == -1 || usedIDs.Contains(s.id))
            {
                do
                {
                    s.id = ++startID;
                }
                while (usedIDs.Contains(s.id));
                    
                UnityEditor.EditorUtility.SetDirty(s.gameObject);
                UnityEditor.EditorUtility.SetDirty(s);
                usedIDs.Add(s.id);
            }
           

        }
#endif
    }
}
