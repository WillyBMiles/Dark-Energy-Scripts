using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class MenuSteamController : MonoBehaviour
{
    public GameObject JoinFailedInfoBox;
    protected Callback<GameLobbyJoinRequested_t> joinRequested;

    void Start()
    {
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t joinRequest)
    {
        JoinFailedInfoBox.SetActive(true);
    }


    private void OnDestroy()
    {
        if (joinRequested != null)
            joinRequested.Dispose();
    }



}