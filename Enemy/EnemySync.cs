using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemySync : NetworkBehaviour
{
    EnemyAI enemyAI;
    Ship ship;

    [SyncVar]
    public int state;

    [SyncVar]
    public Vector3 serverPosition;

    [SyncVar]
    public Quaternion serverRotation;

    [SyncVar]
    public GameObject target;
    [SyncVar]
    public bool isLeashing;


    // Start is called before the first frame update
    void Awake()
    {
        
        enemyAI = GetComponent<EnemyAI>();
        ship = GetComponent<Ship>();

        enemyAI.canMove = true;
    }

    private void Start()
    {
        
    }

    private void OnConnectedToServer()
    {
        transform.SetPositionAndRotation(serverPosition, serverRotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (!enemyAI.IsInDistanceInterest())
        {
            return;
        }

        if (!isServer)
        {
            enemyAI.SetAIState(state);
            if ((enemyAI.target == null && target != null) || enemyAI.target.gameObject != target)
            {
                enemyAI.SetTarget(target);
            }
            enemyAI.SetLeashing(isLeashing);
        }
        else
        {
            serverPosition = transform.position;
            serverRotation = transform.rotation;
            target = enemyAI.target == null ? null : enemyAI.target.gameObject;
            state = enemyAI.GetAIState();
            isLeashing = enemyAI.leashing;
        }
        
    }
}
