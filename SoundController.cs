using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundController : MonoBehaviour
{
    static SoundController singleton;

    public static float soundVolume;
    public static float musicVolume;
    public AudioMixer mixer;


    const string SOUND_VOLUME = "SOUND_VOLUME";
    const string MUSIC_VOLUME = "MUSIC_VOLUME";

    private void Awake()
    {
        if (singleton)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;
        DontDestroyOnLoad(gameObject);

        soundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME, 0f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME, 0f);
        
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(SOUND_VOLUME, soundVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME, musicVolume);
    }

    private void Update()
    {
        mixer.SetFloat("Music", musicVolume);
        mixer.SetFloat("SFX", soundVolume);
    }
}
