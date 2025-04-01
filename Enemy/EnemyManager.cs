using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    Vector3 position;
    Quaternion rotation;
    float health;
    float maxHealth;
    float baseControlResistance;
    NetworkTransformUnreliable networkTransform;

    EnemyAI ai;
    Ship ship;
    // Start is called before the first frame update
    void Start()
    {
        ResetManager.instance.Reset += InitiateReset;
        ship = GetComponent<Ship>();
        ai = GetComponent<EnemyAI>();

        position = transform.position;
        rotation = transform.rotation;
        health = ship.health;
        maxHealth = ship.maxHealth;
        baseControlResistance = ship.controlResistance;

        networkTransform = GetComponent<NetworkTransformUnreliable>();

    }


    public void LeaveInterest()
    {
        ResetManager.instance.Reset -= InitiateReset;
    }

    public void EnterInterest()
    {
        ResetManager.instance.Reset += InitiateReset;
        InitiateReset();
    }

    private void OnDestroy()
    {
        ResetManager.instance.Reset -= InitiateReset;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitiateReset()
    {
        if (ship.isServer && ship.DestroyOnDeath)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }

        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);

        ship.EnemyReset(health);
        ai.ResetState();
        networkTransform.SetDirty();

    }

    public void AdjustEnemyBaseHealth()
    {
        int playerCount = Mathf.Clamp(NetworkPlayer.players.Count, 1, 100);
        float healthProportion = ship.health / ship.maxHealth;
        ship.controlResistance = baseControlResistance * Mathf.Pow(EnemyScaling.CONTROL_RESIST_MULT, playerCount - 1);
        if (ship.bossScaling)
        {
            float sum = (playerCount - 1) * EnemyScaling.BOSS_ENEMY_HEALTH_ADD + 1;
            ship.health *= sum;
            ship.maxHealth = maxHealth * sum;
        }
        else
        {
            ship.health *= Mathf.Pow(EnemyScaling.REGULAR_ENEMY_HEALTH_MULT, playerCount - 1);
            ship.maxHealth = maxHealth * Mathf.Pow(EnemyScaling.REGULAR_ENEMY_HEALTH_MULT, playerCount - 1);
        }
        ship.health = healthProportion * ship.maxHealth;
    }

}
