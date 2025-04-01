using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class PlayerMovement : NetworkBehaviour
{

    Rigidbody2D m_Rigidbody2D;

    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    [Range(0, .3f)][SerializeField] private float m_AngleSmoothing = .05f;
    [Range(1f, 10f)][SerializeField] private float m_TurnSpeed = .05f;
    public float maxVelocity = 0f;

    private Vector3 m_Velocity = Vector3.zero;
    private float m_AngularVelocity = 0f;

    public bool inControl = true;

    [SerializeField]
    public List<SpriteRenderer> colors = new();

    Animator animator;

    Ship ship;

    [Space(30)]
    public Vector2 boostMoveSpeed;
    public bool boostInMoveDirection;
    Vector2 lastMoveDirection;
    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        ship = GetComponent<Ship>();

        ResetManager.instance.Reset += InitiateReset;

    }

    private void OnDestroy()
    {
        ResetManager.instance.Reset -= InitiateReset;
    }
    Quaternion lastTargetRotation;

    float resetCountdown = 2f;

    // Update is called once per frame
    void Update()
    {

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        bool isIdling = info.IsName("Idle");
        bool isSustaining1 = info.IsName("Sustain1") || info.IsName("SustainIn1") || info.IsName("SustainOut1");
        bool isSustaining2 = info.IsName("Sustain2") || info.IsName("SustainIn2") || info.IsName("SustainOut2");

        if (ship.InControl)
        {

            float actualMaxVelocity = ShipStats.AdjustValue(maxVelocity, ship.stats, Stat.StatEnum.MoveSpeed);
            float actualTurnSpeed = ShipStats.AdjustValue(m_TurnSpeed, ship.stats, Stat.StatEnum.TurnSpeed);

            if (isIdling)
                animator.SetFloat("Engine", m_Rigidbody2D.velocity.magnitude / actualMaxVelocity);
            else
                animator.SetFloat("Engine", 0f);

            //Controls
            if (ship == Ship.playerShip)
            {
                Vector2 move = Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxis("Vertical");

                if (!isIdling)
                {
                    if (boostInMoveDirection && lastMoveDirection.sqrMagnitude > 0f)
                    {
                        move = boostMoveSpeed.x * Vector2.Perpendicular(lastMoveDirection) + boostMoveSpeed.y * lastMoveDirection;
                    }
                    else
                    {
                        move = boostMoveSpeed.x * transform.right + boostMoveSpeed.y * transform.up;
                    }
                }


                Vector3 targetVelocity = move * actualMaxVelocity;
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

                Vector2 rawMove = Vector2.right * Input.GetAxisRaw("Horizontal") + Vector2.up * Input.GetAxisRaw("Vertical");
                //You have to lock in move direction before dashing
                //if we didn't have these conditions, you could never dash in any direction
                if (rawMove.sqrMagnitude > 0.1f && isIdling) 
                {
                    lastMoveDirection = rawMove.normalized;
                }
                else if (isIdling){
                    lastMoveDirection = new Vector2(0f, 0f);
                }

                Vector2 mouseP = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                m_Rigidbody2D.angularVelocity = Mathf.SmoothDamp(m_Rigidbody2D.angularVelocity, 0f, ref m_AngularVelocity, m_AngleSmoothing);

                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, mouseP - (Vector2)transform.position);
                if (!isIdling && !isSustaining1 && !isSustaining2)
                {
                    targetRotation = lastTargetRotation;
                }

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * actualTurnSpeed);

                if (Input.GetKeyDown(KeyCode.Q) && ship.cells > 0)
                {
                    animator.SetTrigger("Heal");
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    animator.SetTrigger("Roll");
                }

                if (ship.power <= 0f)
                {
                    ship.ResetAllTriggers();
                    animator.SetBool("Sustain1", false);
                    animator.SetBool("Sustain2", false);
                }

                

                lastTargetRotation = targetRotation;
            }
            
            //endControls


        }
        else
        {
            animator.SetFloat("Engine", 0f);
        }



       if (isServer)
        {
            bool liveFound = false;
            foreach (Ship s in Ship.PlayerShips)
            {
                if (!s.playerDisabled)
                {
                    liveFound = true;
                    break;
                }
            }
            if (!liveFound)
            {
                resetCountdown -= Time.deltaTime;
                if (resetCountdown <= 0f)
                {
                    NetworkController.instance.TryRestart();
                    resetCountdown = 5f;
                }
            }
        }
        
    }

    public void Heal()
    {
        if (ship.cells > 0)
        {
            ship.health = 
                Mathf.Clamp(ship.health + ShipStats.AdjustValue(ship.ActualMaxHealth, ship.stats, Stat.StatEnum.CellHealing), ship.health, ship.ActualMaxHealth);
            ship.cells--;
            animator.ResetTrigger("Heal");
        }

    }

    public void Recolor(Color color)
    {
        foreach (SpriteRenderer sr in colors)
        {
            sr.color = color;
        }
    }

    public void InitiateReset()
    {
        if (ship == Ship.playerShip && ship.playerDisabled)
        {

            Scrap.instance.PlaceScrap(ship.lastDeath, PlayerRPG.localRpg.currency);
            PlayerRPG.localRpg.currency = 0;
            
        }
        GetComponent<PlayerItems>().ResetGame();

        transform.position = Station.GetSpawnPosition();
        ship.ResetAllTriggers();
        ship.Revive();
        ship.AnimationReset();
        ship.ResetLastHits();

        ship.cells = (int) ship.AdjustedValue(Stat.StatEnum.NumberOfCells);
        ship.health = ship.ActualMaxHealth;

    }

    [Command]
    public void CmdUpdateStats(Dictionary<Stat.StatEnum, int> newStats)
    {
        RpcUpdateStats(newStats);
    }
    [ClientRpc]
    public void RpcUpdateStats(Dictionary<Stat.StatEnum, int> newStats)
    {
        if (this == Ship.playerShip )
            return;
        if (!TryGetComponent<PlayerRPG>(out var rpg))
            return;

        rpg.stats = newStats;
        rpg.UpdateStats();
    }


}
