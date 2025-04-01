using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipDamageManager : NetworkBehaviour
{
    Ship ship;

    // Start is called before the first frame update
    void Awake()
    {
        ship = GetComponent<Ship>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static bool IsAuthoritativeDamage(Ship origin, Ship target)
    {
        if (origin.isPlayer && origin == Ship.playerShip)
            return true;
        else if (origin.isPlayer)
            return false;
        //Origin is not player...
        if (target.isPlayer && target == Ship.playerShip)
            return true;
        else if (target.isPlayer)
            return false;

        //neither target nor origin is player
        return origin.isServer;
    }

    public void DealDamage()
    {
        //Apply alldamageeffects
        //if authoritative do authoritative damage effects
    }


    public void AllDamageEffects()
    {
        /*
         Show hit marker
	    play hit sound
	    play shield hit (if shielding)
	    show shield hit (if shielding)
	    Knockback
         */
    }

    public void AuthoritativeDamageEffects()
    {
        /* Damage, kill
         * Shield damage, shield break
         * Control damage, control break
         * */
    }

    [Command]
    public void CmdApplyDamageEffects()
    {

    }

    [ClientRpc]
    public void RpcApplyDamageEffects()
    {

    }


}
