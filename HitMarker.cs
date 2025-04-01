using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMarker : NetworkBehaviour
{
    public static HitMarker instance;
    public GameObject hitPrefab;
    public GameObject shieldPrefab;
    public GameObject explosionPrefab;

    float lastTime;
    List<int> explodedShipIDs = new(); // all HitIDs that have caused explosions, to prevent dupes, plus a time
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        explodedShipIDs.Clear();
    }

    public void ClearAllExplosions()
    {
        explodedShipIDs.Clear();
    }
    public void ClearMyShipID(int shipID) {
        explodedShipIDs.Remove(shipID);
    }

    public void CreateHit(Vector3 position, Vector3 direction, float damage, bool ShieldHit)
    {
        GameObject prefab = ShieldHit ? shieldPrefab : hitPrefab;

        GameObject go = Instantiate(prefab, position, Quaternion.LookRotation(Vector3.forward, direction));
        float scale = Mathf.Min(Mathf.Sqrt(damage) / Mathf.Sqrt( 5 ) + .2f, 2);
        if (lastTime == Time.time) //only one sound at a time please
        {
            go.GetComponent<AudioSource>().volume = 0f;
        }
        go.transform.localScale = new Vector3(go.transform.localScale.x * scale, go.transform.localScale.y * scale, go.transform.localScale.z);
        lastTime = Time.time;
    }

    public void CreateExplosion(int shipID, Vector3 position, float scale, int reward, bool sendCmd = true)
    {
        if (explodedShipIDs.Contains(shipID))
        {
            return;
        }

        
        if (sendCmd)
        {
            CmdCreateExplosion(shipID, position, scale, reward);
        }

        GameObject go = Instantiate(explosionPrefab, position, Quaternion.identity);
        go.transform.localScale = new Vector3(go.transform.localScale.x * scale, go.transform.localScale.y * scale, go.transform.localScale.z);
        explodedShipIDs.Add(shipID);

        PlayerRPG.GiveReward((int) ShipStats.AdjustValue(reward, Ship.playerShip.stats, Stat.StatEnum.KillRewardEarned ));

    }

    [Command(requiresAuthority = false)]
    void CmdCreateExplosion(int shipId, Vector3 position, float scale, int reward)
    {
        RpcCreateExplosion(shipId, position, scale, reward);
    }

    [ClientRpc]
    void RpcCreateExplosion(int shipId, Vector3 position, float scale, int reward)
    {
        CreateExplosion(shipId, position, scale, reward, false);
    }
}
