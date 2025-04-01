using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterCreation : SerializedMonoBehaviour
{

    public string menu;
    public string game;
    ShipSave save = new();
    public TMPro.TMP_InputField inputField;
    public Image colorImage;

    public ItemInfo weapon1;
    public ItemInfo weapon2;

    public Slider HSlider, SSlider, VSlider;

    // Start is called before the first frame update
    void Start()
    {
        save.fileName = SaveManager.DATA_PATH + System.DateTime.UtcNow.Ticks.ToString() + Random.Range(0,1000) + SaveManager.SHIP_FILE_EXTENSION;
        save.Weapon1 = weapon1;
        save.Weapon2 = weapon2;
        save.inventory.Add(weapon1);
        save.inventory.Add(weapon2);
        save.health = 10f;
        VSlider.value = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        save.name = inputField.text;
        save.color = Color.HSVToRGB(HSlider.value,SSlider.value / 2f,VSlider.value/ 2f + .5f);
        colorImage.color = save.color;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(menu);
    }

    public void Play()
    {
        
        SaveManager.Save(save);
        SaveManager.LoadShipToGame(save);
        SceneManager.LoadScene(game);
    }
}
