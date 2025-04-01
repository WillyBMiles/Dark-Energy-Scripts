using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    public Slider musicSlider;
    public Slider soundSlider;

    private void OnEnable()
    {
        musicSlider.value = ReverseConvertToDB( SoundController.musicVolume);
        soundSlider.value = ReverseConvertToDB( SoundController.soundVolume);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMusicChanged()
    {
        SoundController.musicVolume = ConvertToDB(musicSlider.value);
    }
    public void OnSFXChanged()
    {
        SoundController.soundVolume = ConvertToDB(soundSlider.value);
    }

    float ConvertToDB(float percent)
    {
        return Mathf.Lerp(-80, 20, Mathf.Pow(percent,.5f));
    }
    float ReverseConvertToDB(float db)
    {
        return Mathf.Pow((db + 80f) / 100f, 2f);
    }
}
