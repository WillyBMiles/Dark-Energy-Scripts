using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;

public class EscapeMenu : MonoBehaviour
{
    SteamController steamController;

    public GameObject parent;
    public GameObject openText;
    public GameObject closeText;

    public TextMeshProUGUI pingText;

    public List<GameObject> parents = new();

    public string MenuName;
    // Start is called before the first frame update
    void Start()
    {
        steamController = FindAnyObjectByType<SteamController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            parent.SetActive(!parent.activeInHierarchy);
            SwapToParent(0);
        }
        openText.SetActive(!lobbyOpen && !inLobby);
        closeText.SetActive(lobbyOpen && !inLobby);

        inLobby = steamController.IsInOtherLobby();

        if (parent.activeInHierarchy)
        {
            pingText.text = $"Ping: {(int)(NetworkTime.rtt * 1000f)}ms";
        }
    }


    public bool lobbyOpen;
    public bool inLobby;
    public void StartSteamLobby()
    {
        lobbyOpen = true;
        steamController.OpenLobby();
    }

    public void CloseSteamLobby()
    {
        lobbyOpen = false;
        steamController.CloseLobby();
    }

    public void LeaveLobby()
    {
        steamController.LeaveLobby();
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene(MenuName);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void SwapToParent(int index)
    {
        for (int i =0; i < parents.Count; i++)
        {
            if (i == index)
            {
                parents[i].SetActive(true);
            }
            else
                parents[i].SetActive(false);
        }
    }
}
