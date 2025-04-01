using Mirror;
using Mirror.SimpleWeb;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour
{
    public static List<NetworkPlayer> players = new();

    public GameObject shipPrefab;
    public GameObject myShip;

    [SyncVar]
    GameObject syncedShip;
    [SyncVar]
    public ShipSave save = null;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        players.Add(this);

        if (syncedShip != null && save != null)
        {
            myShip = syncedShip;
            save.Load(myShip.GetComponent<Ship>(), false); //for late joiners?
        }
    }

    private void OnDestroy()
    {
        players.Remove(this);
    }

    float retryTimer = 0f; //prevents too many cmd messages, don't want to send a command every frame e.g.
    const float RETRY_TIME = .1f;
    private void Update()
    {
        if (isLocalPlayer && myShip != null)
        {
            if (Ship.playerShip == null || Ship.playerShip.gameObject != myShip)
                Ship.playerShip = myShip.GetComponent<Ship>();
        }
        if (isLocalPlayer && myShip == null && NetworkClient.ready)
        {
            if (retryTimer <= 0f)
            {
                CmdRequestShip(ShipSave.currentSave);
                retryTimer = RETRY_TIME;
            }
            retryTimer -= Time.deltaTime;
        }
        if (isServer && myShip != null)
            syncedShip = myShip;
        
        if (myShip != null)
        {
            transform.position = myShip.transform.position;
        }
    }

    [Server]
    void SpawnShip(ShipSave save)
    {
        if (myShip != null)
            return;
        Vector3 pos = Station.GetSpawnPosition();
        myShip = Instantiate(shipPrefab, pos, Quaternion.identity);
        NetworkServer.Spawn(myShip, gameObject);
        this.save = save;
        AssignShip(myShip, save);
        RpcAssignShip(myShip, save);

    }

    public void AssignShip(GameObject ship, ShipSave save)
    {
        Ship theShip = ship.GetComponent<Ship>();
        myShip = ship;
        save.Load(theShip, isLocalPlayer);
        if (isLocalPlayer)
            CameraFollow.SetCamera(ship.transform.position);
    }

    [ClientRpc]
    void RpcAssignShip(GameObject ship, ShipSave save)
    {
        
        if (!isServer)
        {
            AssignShip(ship, save);
        }
    }

    [Command]
    void CmdRequestShip(ShipSave save)
    {
        SpawnShip(save);
    }


}
