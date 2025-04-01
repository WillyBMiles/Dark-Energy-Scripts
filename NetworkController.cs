using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkController : NetworkBehaviour
{
    public static NetworkController instance;
    NetworkManager manager;

    private void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        instance = GetComponent<NetworkController>();
        
    }

    

    private void OnDisable()
    {
        if (instance == this)
        {
            
            if (isServer)
            {
                NetworkServer.DisconnectAll();
            }
            else
                NetworkClient.Disconnect();

        }
            
    }

    private void Update()
    {

    }
    public void TryRestart(int stationID = -1)
    {
        if (stationID != -1)
        {
            if (isServer)
            {
                RpcSetStationID(stationID);
            }
            else
            {
                CmdSetStationID(stationID);
            }
        }
        ResetManager.instance.InitiateReset();
    }


    [Command(requiresAuthority = false)]
    void CmdSetStationID(int stationID)
    {
        RpcSetStationID(stationID);
    }

    [ClientRpc]
    void RpcSetStationID(int stationID)
    {
        Station.currentStation = stationID;
    }


}
