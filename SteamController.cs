using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using UnityEngine.SceneManagement;

public class SteamController : MonoBehaviour
{
    public static SteamController singleton;
    NetworkManager manager;
    bool myLobby = true;

    public string mainMenuScene;

    public Callback<GameLobbyJoinRequested_t> joinRequested;
    public Callback<LobbyCreated_t> lobbyCreated;
    public Callback<LobbyEnter_t> lobbyEntered;

    void Awake()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;

        DontDestroyOnLoad(gameObject);

        manager = FindObjectOfType<NetworkManager>();

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
    }

    private void Start()
    {
        manager.StartHost();

        //Steamworks.SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        //Steamworks.SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void OnDestroy()
    {
        //if (singleton == this)
        //SteamClient.Shutdown();
        //Steamworks.SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        //Steamworks.SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        if (singleton != this)
            return;
        
        lobbyCreated.Dispose();
        lobbyEntered.Dispose();
        joinRequested.Dispose();
    }

    public CSteamID? lobbyID { get; private set; } = null;
    private void OnLobbyCreated(LobbyCreated_t lobbyCreated)
    {
        lobbyID = new CSteamID(lobbyCreated.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyID.Value, "Connection", SteamUser.GetSteamID().ToString());
        myLobby = true;
    }
    /*
     Steamworks.Data.Lobby? lobby = await Steamworks.SteamMatchmaking.CreateLobbyAsync();
        if (lobby.HasValue)
        {
            currentLobby = lobby;
            currentLobby.Value.SetData("JOIN_KEY", SteamClient.SteamId.ToString());
            currentLobby.Value.SetFriendsOnly();
            myLobby = true;
        }
     */


    private void OnLobbyEntered(LobbyEnter_t lobbyEntered)
    {
        CSteamID thisLobbyID = new CSteamID(lobbyEntered.m_ulSteamIDLobby);

        if (lobbyID.HasValue && lobbyID.Value.m_SteamID == thisLobbyID.m_SteamID) //THIS IS MY LOBBY!!
        {
            return;
        }
        lobbyID = thisLobbyID;

        if (lobbyID != null)
        {
            manager.StopHost();
            manager.networkAddress = SteamMatchmaking.GetLobbyData(lobbyID.Value, "Connection");
            

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            StartCoroutine(StartClientDelayed());
            
        }

    }

    IEnumerator StartClientDelayed()
    {
        yield return 5;
        manager.StartClient();
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t joinRequest)
    {
        //lobbyID = joinRequest.m_steamIDLobby;
        JoinLobby(joinRequest.m_steamIDLobby);
        myLobby = false;
    }

    public static void JoinLobby(CSteamID steamID)
    {
        SteamMatchmaking.JoinLobby(steamID);
    }

    /*
    private void OnGameLobbyJoinRequested(Steamworks.Data.Lobby lobby, SteamId id)
    {
        if (currentLobby.HasValue)
        {
            currentLobby.Value.Leave();
        }
        Steamworks.SteamMatchmaking.JoinLobbyAsync(lobby.Id);
        myLobby = false;

    }
    */
    /*
    void OnLobbyEntered(Steamworks.Data.Lobby lobby)
    {
        if (currentLobby.HasValue && currentLobby.Value.Id == lobby.Id) //THIS IS MY LOBBY!!
        {
            return;
        }
        
        currentLobby = lobby;
        string joinKey = lobby.GetData("JOIN_KEY");
        //Debug.LogError(joinKey);
        manager.StopHost();
        
        manager.networkAddress = joinKey;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StartCoroutine(StartClientDelayed());
        
    }

    IEnumerator StartClientDelayed()
    {
        
        yield return 5;
        manager.StartClient();
    }
    */



    private void OnGUI()
    {
        //if (SteamClient.IsValid)
        //    GUILayout.Box(SteamClient.SteamId.ToString());
    }

    
    public void OpenLobby()
    {
        if (!lobbyID.HasValue)
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 50);
        /*
        Steamworks.Data.Lobby? lobby = await Steamworks.SteamMatchmaking.CreateLobbyAsync();
        if (lobby.HasValue)
        {
            currentLobby = lobby;
            currentLobby.Value.SetData("JOIN_KEY", SteamClient.SteamId.ToString());
            currentLobby.Value.SetFriendsOnly();
            myLobby = true;
        }
        */
    }
    

    public void CloseLobby()
    {
        if (lobbyID.HasValue)
        {
            SteamMatchmaking.LeaveLobby(lobbyID.Value);
        }
        /*
        if (currentLobby.HasValue)
        {
            currentLobby.Value.Leave();
        }
        */
    }

    public void LeaveLobby()
    {
        if (lobbyID.HasValue)
        {
            SteamMatchmaking.LeaveLobby(lobbyID.Value);
            //currentLobby.Value.Leave();
            SceneManager.LoadScene(mainMenuScene);

            Destroy(gameObject);
            Destroy(manager.gameObject);
        }
        
    }

    public bool IsInOtherLobby()
    {
        return lobbyID.HasValue && !myLobby;
    }
    public bool IsInMyLobby()
    {
        return lobbyID.HasValue && myLobby;
    }
}
