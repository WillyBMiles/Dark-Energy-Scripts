using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class EnemyAI : NetworkBehaviour
{
    Ship ship;
    Rigidbody2D m_Rigidbody2D;
    Animator animator;

    [Range(0, .2f)][SerializeField] private float m_SpeedSmoothing = .02f;
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    [Range(0, .3f)][SerializeField] private float m_AngleSmoothing = .05f;
    [Range(1f, 10f)] private float m_TurnSpeed = .05f;
    public float defaultVelocity = 0f;

    float maxVelocity;

    public Ship target = null;

    private Vector3 m_Velocity = Vector3.zero;
    private Vector3 m_Acceleration = Vector3.zero;
    private float m_AngularVelocity = 0f;

    public Dictionary<string, Vector3> tempPoints = new();
    public Dictionary<string, float> tempFloats = new();

    Vector3 startLocation;
    public float leashRange = 99999f;
    public float leashTime = 3f;
    float leashTimer;

    public float distanceInterestTime = 1f;
    float distanceInterestTimer = 0f;
    public float distanceInterest = 15f;
    float squareDistanceInterest;

    public float deaggroTime = 7f;
    float deaggroTimer = 0f;
    
    public bool leashing;

    public bool aggro { get; private set;} = false; //Tracks whether this is aggroed right now
    
    [SyncVar]
    public bool hasAggro= false; //Tracks whether this has ever been aggroed

    [HideInInspector]
    //TEMPORARY VARIABLE WHILE TESTING ENEMYSYNC
    public bool canMove = false;

    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        ship = GetComponent<Ship>();
        ship.hitSignal += OnGotHit;
        animator = GetComponent<Animator>();
        manager = GetComponent<AIManager>();
        startLocation = transform.position;
        distanceInterestTimer = Random.value * distanceInterestTime;
        squareDistanceInterest = distanceInterest * distanceInterest;

        ResetState();

        Invoke(nameof(CheckLeash), leashTime);
    }

    AIManager manager;
    [HideInInspector]
    public AIState currentState;

    float lastTargetAngle;

    public Vector2 boostMoveSpeed;

    // Update is called once per frame
    void Update()
    {
        if (!IsInDistanceInterest())
            return;

        if (ship.InControl)
        {
            bool isIdling = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");


            if (isServer || canMove)
            {
                if (isServer)
                {
                    CheckDeagro();
                    CheckLeash();
                }
                
                if (aggro)
                {
                    hasAggro = true;
                }
                m_Rigidbody2D.isKinematic = !aggro;


                m_TurnSpeed = ShipStats.AdjustValue(currentState.angularSpeed, ship.stats, Stat.StatEnum.TurnSpeed);
                maxVelocity = ShipStats.AdjustValue(currentState.velocityMult * defaultVelocity, ship.stats, Stat.StatEnum.MoveSpeed);


                Vector3 targetPosition;
                if (target != null)
                {
                    aggro = true;
                    if (Node.IsInLOS(target.transform.position, transform.position))
                    {
                        targetPosition = currentState.GetPoint(ship, target);
                    }
                    else
                    {
                        targetPosition = Node.Pathfind(transform.position, ship.node, target.transform.position, target.node);
                    }
                }
                else
                    targetPosition = startLocation;

                if (leashing)
                {
                    targetPosition = Node.Pathfind(transform.position, ship.node, startLocation, ship.startingNode);
                }
                


                if (currentState.freezeMoveDuringAttack && !isIdling)
                {
                    targetPosition = transform.position;
                    
                }

                Vector3.SmoothDamp(transform.position, targetPosition, ref m_Velocity, m_SpeedSmoothing);//This will tell us the desired velocity if we were smoothly damping positions

                if (m_Velocity.magnitude > maxVelocity)
                {
                    m_Velocity = m_Velocity.normalized * maxVelocity; //this caps velocity

                }
                if (!isIdling)
                {
                    m_Velocity += (boostMoveSpeed.x * transform.right + boostMoveSpeed.y * transform.up); //this adds boost if necessary
                }
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, m_Velocity, ref m_Acceleration, m_MovementSmoothing);
                //this damps our velocity between current and target velocity

                animator.SetFloat("Engine", m_Velocity.magnitude / maxVelocity);


                //Rotation of ship
                float targetAngle = currentState.GetAngle(ship, target);
                if (currentState.freezeTurnDuringAttack && !isIdling)
                {
                    targetAngle = lastTargetAngle;
                }

                m_Rigidbody2D.angularVelocity = Mathf.SmoothDamp(m_Rigidbody2D.angularVelocity, 0f, ref m_AngularVelocity, m_AngleSmoothing);
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_TurnSpeed);
                lastTargetAngle = transform.rotation.eulerAngles.z;
            }

            //Setting attack commands and changing state
            if (isServer)
            {
                if (currentState.ShouldAttack(ship, target, this))
                {
                    animator.SetTrigger("Attack" + currentState.attack);
                }
                else
                {
                    animator.ResetTrigger("Attack" + currentState.attack);
                }
                
                if (isIdling && manager.ShouldChangeState(this))
                {
                    currentState = manager.GetNextState(this);
                }
                if (isIdling && currentState.ShouldGetNewTarget(this))
                {
                    target = currentState.GetTarget(ship, this);
                }
            }
        }

        else
        {
            //PASS:Ship not in control
        }

    }

    void CheckLeash()
    {
        if (!aggro)
            return;

        leashTimer -= Time.deltaTime;
        if (leashTimer > 0f)
            return;
        leashTimer = leashTime; 

        if (gameObject == null)
        {
            return;
        }
        Invoke(nameof(CheckLeash), leashTime);
        if (target == null)
        {
            leashing = true;

            if (deaggroTimer == Mathf.Infinity)
                deaggroTimer = deaggroTime;
            return;
        }
        if (Vector3.Distance(target.transform.position, startLocation) > leashRange)
        {
            leashing = true;
            if (deaggroTimer == Mathf.Infinity)
                deaggroTimer = deaggroTime;
        }
        else
        {
            leashing = false;
            deaggroTimer = Mathf.Infinity;
        }
    }

    void CheckDeagro()
    {
        if (!aggro)
            return;

        if (leashing && deaggroTime > 0f)
        {
            deaggroTimer -= Time.deltaTime;
        }
        if (leashing && deaggroTime == 0f)
        {
            if (Vector3.Distance(transform.position, startLocation) < .1f)
            {
                Deagro();
            }
            else deaggroTime = 1f;
        }
           
    }

    void Deagro()
    {
        currentState = manager.GetFirstState(this);
        aggro = false;
    }

    bool wasInInterest = false;
    public bool IsInDistanceInterest()
    {
        if (aggro)
        {
            wasInInterest = true;
            return true;
        }
        if (distanceInterestTimer > 0f)
        {
            distanceInterestTimer -= Time.deltaTime;
            return wasInInterest;
        }
        distanceInterestTimer = distanceInterestTime;
        foreach (Ship s in Ship.PlayerShips)
        {
            if (Vector2.SqrMagnitude(transform.position - s.transform.position) < squareDistanceInterest)
            {
                wasInInterest = true;
                return true;
            }
        }
        wasInInterest = false;
        return false;
    }

    void OnGotHit(Ship source)
    {
        if (!isServer)
            return;
        if (target == null)
        {
            target = source;
        }
        leashing = false;
    }

    public void ActuallyAttack()
    {
        currentState.ActuallyAttack(this);
    }

    public void ResetState()
    {
        Deagro();
        if (isServer)
            hasAggro = false;
        target = null;
        maxVelocity = 0f;
        leashing = false;
        lastTargetAngle = transform.rotation.eulerAngles.z;
        leashTimer = leashTime;
        m_Rigidbody2D.isKinematic = true;

        boostMoveSpeed = new();
    }


    //Sync specific
    public void SetAIState(int state)
    {
        if (state == -1)
            currentState = manager.startState;
        currentState = manager.states[state];
    }

    public int GetAIState()
    {
        if (manager.states.Contains(currentState))
        {
            return manager.states.IndexOf(currentState);
        }
        return -1;
    }

    public void SetTarget(GameObject target)
    {
        this.target = target.GetComponent<Ship>();
    }

    public void SetLeashing(bool value)
    {
        leashing = value;
    }
}
