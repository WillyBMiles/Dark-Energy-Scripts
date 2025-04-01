using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    [Header("Custom Properties")]
    [Scene]
    public string menuScene;

    public bool returnToMenuOnDC = true;

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        if (returnToMenuOnDC)
            SceneManager.LoadScene(menuScene);
    }
}
