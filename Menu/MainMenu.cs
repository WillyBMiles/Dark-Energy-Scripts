using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    [SerializeField]
    GameObject ContinueButton;

    [SerializeField]
    GameObject loadMenu;

    [SerializeField]
    GameObject lagButton;

    [SerializeField]
    Transform loadMenuParent;
    [SerializeField]
    GameObject loadButtonPrefab;

    [SerializeField]
    Texture2D cursor;

    private void Awake()
    {
        if (Debug.isDebugBuild)
        {
            lagButton.SetActive(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        instance = this;
        NetworkManager manager = FindObjectOfType<NetworkManager>();
        if (manager != null)
        {
            manager.StopHost();
            NetworkClient.Disconnect();
            Destroy(manager.gameObject);
        }
        SteamController steamController = SteamController.singleton;

        if (steamController != null)
        {
            steamController.CloseLobby();
            steamController.LeaveLobby();
            Destroy(steamController.gameObject);
        }

        Cursor.SetCursor(cursor, new Vector2(), CursorMode.Auto);
    }

    // Update is called once per frame
    void Update()
    {
        if (ShipSave.currentSave == null)
        {
            ContinueButton.SetActive(false);
        }
    }

    public string CharacterCreationScene;
    public string GameScene;

    public void GoToCharacterCreation()
    {
        SceneManager.LoadScene(CharacterCreationScene);
    }

    List<ShipSave> saves = new();
    public void OpenLoadMenu()
    {
        loadMenu.SetActive(true);

        SaveManager.LoadAllShips(saves);
        for (int i = loadMenuParent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(loadMenuParent.GetChild(i).gameObject);
        }

        foreach (ShipSave s in saves)
        {
            GameObject go = Instantiate(loadButtonPrefab, loadMenuParent);//CONTINUE THIS
            go.GetComponent<LoadSaveButton>().save = s;
        }
    }
    public void CloseLoadMenu()
    {
        loadMenu.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Continue()
    {
        if (ShipSave.currentSave == null)
            return;
        SceneManager.LoadScene(GameScene);
    }

    public void LagZone()
    {
        if (ShipSave.currentSave == null)
            return;
        SceneManager.LoadScene("LagZone");
    }

    public static void StaticContinue()
    {
        if (instance != null)
            instance.Continue();
    }
}
