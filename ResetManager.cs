using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetManager : NetworkBehaviour
{
    public static ResetManager instance;

    public delegate void ResetType();
    public ResetType Reset;

    AudioSource myAudio;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        myAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitiateReset()
    {
        CmdReset();
    }


    [Command(requiresAuthority = false)]
    void CmdReset()
    {
        RpcReset();
        foreach (Ship s in Ship.PlayerShips)
        {
            if (s == Ship.playerShip)
                continue;
            s.isResetting = true;
            s.networkTransform.enabled = false;
            s.transform.position = Station.GetSpawnPosition();
        }
        
    }

    [ClientRpc]
    void RpcReset()
    {
        
        ActualReset();
    }


    public void ActualReset()
    {
        Reset?.Invoke();
        FadeIn.instance.ResetFade();
        CameraFollow.SetCamera(Station.GetSpawnPosition());

        Ship.playerShip.CmdDoneResetting();

        SaveManager.SaveCurrentShip();

        HitMarker.instance.ClearAllExplosions();

        myAudio.Stop();
        myAudio.Play();
    }
}
