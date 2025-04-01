using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerPing : NetworkBehaviour
{
    [SerializeField]
    public GameObject pingPrefab;

    NetworkPlayer player;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<NetworkPlayer>();
    }
    const int MAX_AMMO = 5;
    int ammo;
    float timer;
    const float MAX_TIMER = 4f;

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;
        if (ammo > 0 && Input.GetButtonDown("Ping"))
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            point = new Vector3(point.x, point.y, 0f);
            Ping(point);
            CmdPing(point);
            ammo--;
            timer = MAX_TIMER;
        }
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ammo = MAX_AMMO;
        }
        
    }

    [Command(channel = Channels.Unreliable)]
    void CmdPing(Vector3 position)
    {
        RpcPing(position);
    }

    [ClientRpc(channel = Channels.Unreliable)]
    void RpcPing(Vector3 position)
    {
        if (isLocalPlayer)
            return;
        Ping(position);
    }

    void Ping(Vector3 position)
    {
        GameObject go = Instantiate(pingPrefab, position, transform.rotation);
        go.GetComponentInChildren<SpriteRenderer>().color = player.save.color;
    }
}
