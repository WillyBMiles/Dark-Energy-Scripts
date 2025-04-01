using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSound : MonoBehaviour
{
    static GlobalSound instance;

    public List<AudioSource> soundFXSources = new();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void PlaySound(Vector3 point, AudioClip clip)
    {
        if (instance)
        {
            instance.LocalPlaySound(point, clip);
        }
    }

    void LocalPlaySound(Vector3 point, AudioClip clip)
    {
        AudioSource source = GetFirstAvailable();
        source.transform.position = point;
        source.clip = clip;
        source.Play();
    }

    AudioSource GetFirstAvailable()
    {
        foreach (AudioSource source in soundFXSources)
        {
            if (!source.isPlaying)
                return source;
        }
        return soundFXSources[Random.Range(0, soundFXSources.Count)];
    }
}
