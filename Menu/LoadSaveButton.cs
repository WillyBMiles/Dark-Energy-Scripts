using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadSaveButton : MonoBehaviour
{
    public ShipSave save;
    public Button button;
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(LoadSave);
         
    }

    void LoadSave()
    {
        SaveManager.LoadShipToGame( save );
        MainMenu.StaticContinue();
    }

    public void DeleteSave()
    {
        ConfirmPopup.instance.Show($"Are you sure you want to delete save: {save.name}",ConfirmDeleteSave, NoDeleteSave, 3f);
    }
    void ConfirmDeleteSave()
    {
        File.Delete(save.fileName);
        MainMenu.instance.OpenLoadMenu();
    }
    void NoDeleteSave()
    {
        //pass
    }

    // Update is called once per frame
    void Update()
    {
        text.text = $"Load: {save.name} (Lvl: {save.level})";
    }
}
