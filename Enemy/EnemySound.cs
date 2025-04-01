using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnemySound : SerializedMonoBehaviour
{
    AudioSource source;
    public bool playSound;
    [ShowIf(nameof(playContinuously ))]
    public bool stopSound;

    public bool playContinuously;



    bool lastStopSound;
    bool lastPlaySound;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();


    }
    private void Update()
    {
        if (playSound && !lastPlaySound)
        {
            PlaySound();
        }
        lastPlaySound = playSound;
        if (stopSound && !lastStopSound)
        {
            source.Stop();
        }
        lastStopSound = stopSound;

        source.loop = playContinuously;
    }

    public void PlaySound()
    {
        source.Play();
    }
}
