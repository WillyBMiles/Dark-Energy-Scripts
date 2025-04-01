using Mirror;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(ShipStats))]
public class Ship : NetworkBehaviour
{
    [ReadOnly]
    [SyncVar]
    public int shipID = -1;
    static int nextShipID = 0;

    public static Ship playerShip { get; set; }

    public bool playerTeam = false;
    public bool isPlayer = false;

    public bool bossScaling = false;

    [Tooltip("Set To true on enemies, allows players to knock back enemies on their screen")]
    public bool takesLocalHits = false;

    public float health;
    public float maxHealth;

    public float ActualMaxHealth {  get { return AdjustedValue(Stat.StatEnum.MaxHealth); } }

    public Rigidbody2D rb { get; private set; }
    public float power;
    public float maxPower;

    public float ActualMaxPower {  get {  return AdjustedValue(Stat.StatEnum.MaxPower); } }

    public int reward;

    public bool IsShielded;

    public bool IsDodging;

    [Tooltip("Only true for spawnables.")]
    public bool DestroyOnDeath = false;


    public SpriteRenderer shieldDamage;

    Animator animator;

    public float powerDelay = .8f;
    float currentPowerDelay = 0f;
    public float powerRegen = 3f;
    public bool delayPower = false;

    public int cells = 0;
    public int maxCells;

    public float explosionSizeMult = 1f;

    [Tooltip("Amount of damage you need to take to lose control.")]
    public float controlResistance;
    float controlDamage = 0f;

    public bool InControl { get; private set; }
    [Tooltip("Disabled/dead player")]
    public bool playerDisabled;
    [Tooltip("Immune to knockback damage")]
    public bool immuneToKnockback;
    [Tooltip("Animate to lock rotation e.g. for dashes")]
    public bool lockRotation;

    const float controlResistTime = 5f; //time before control damage fully recharges

    public static List<Ship> PlayerShips = new();
    [HideInInspector]
    public bool isResetting;

    public const string INTERRUPT_TRIGGER = "Interrupt";
    public const string DEAD_TRIGGER = "Dead";
    public const string IDLE_STATE = "Idle";

    public delegate void OnGotHit(Ship ship);
    public OnGotHit hitSignal;

    public bool IsAuthoritative { get { return (isPlayer && this == playerShip) || (!isPlayer && isServer); } }

    public NetworkRigidbodyReliable2D networkTransform { get; private set; }

    public Node node;
    public Node startingNode;

    public EnemyAI ai { get; private set; }
    public EnemyManager enemyManager;

    PlayerItems playerItems;
    public ShipStats stats { get; private set; }

    public delegate void VoidNoParams();
    public VoidNoParams DeathEvent;

    List<AudioSource> sources = new();

    bool prestarted = false;
    Vector3 startingLocalScale;
    float startingMass;

    private void Awake()
    {
        shipID = ++nextShipID;
    }

    // Start is called before the first frame update
    void Start()
    {
        Prestart();
    }
    

    public void Prestart()
    {
        if (prestarted)
            return;
        prestarted = true;

        
        if (startingNode == null)
        {
            startingNode = Node.FindNearestNode(transform.position);
        }
        sources.AddRange(GetComponentsInChildren<AudioSource>());

        node = startingNode; //precalculate?
        nodeTimer = nodeTime * Random.value;

        startingLocalScale = transform.localScale;
        //networkTransform = GetComponent<NetworkRigidbodyReliable2D>();
        playerItems = GetComponent<PlayerItems>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            startingMass = rb.mass;

        animator = GetComponent<Animator>();
        ai = GetComponent<EnemyAI>();
        enemyManager = GetComponent<EnemyManager>();
        stats = GetComponent<ShipStats>();
        AdjustBaseValues();
        animator.keepAnimatorStateOnDisable = true;
        if (isPlayer)
        {
            PlayerShips.Add(this);
        }
    }

    private void OnDestroy()
    {
        PlayerShips.Remove(this);
    }

    public bool isIdling { get; private set; }
    float lastPower = 0f;
    // Update is called once per frame
    void Update()
    {
        AdjustBaseValues();

        GetNode();

        isIdling = animator.GetCurrentAnimatorStateInfo(0).IsName(IDLE_STATE);
        InControl = !animator.GetCurrentAnimatorStateInfo(0).IsName(INTERRUPT_TRIGGER) && !playerDisabled;
        if (!InControl && !isPlayer)
        {
            ResetAllTriggers();
        }

        if (shieldDamage != null)
            shieldDamage.color = new Color(shieldDamage.color.r, shieldDamage.color.g, shieldDamage.color.b, shieldDamage.color.a -Time.deltaTime);
        
        if (!delayPower)
        {
            currentPowerDelay -= Time.deltaTime;
            if (currentPowerDelay <= Time.deltaTime)
            {
                power += AdjustedValue(Stat.StatEnum.PowerRegen) * Time.deltaTime * (IsShielded ?  AdjustedValue(Stat.StatEnum.ShieldPowerMultiplier) : 1f);
                power = Mathf.Min(power, ActualMaxPower);
            }
        }
        if (IsAuthoritative && lastPower != power)
        {
            if (isServer)
                RpcSyncPower(power);
            else
                CmdSyncPower(power);
        }
        lastPower = power;



        controlDamage -= Time.deltaTime * AdjustedValue(Stat.StatEnum.ControlResistance) * AdjustedValue(Stat.StatEnum.ControlRechargeSpeed) / controlResistTime;
        controlDamage = Mathf.Clamp(controlDamage, 0f, AdjustedValue(Stat.StatEnum.ControlResistance));

        if (isPlayer && animator.GetBool(DEAD_TRIGGER))
        {
            animator.SetBool(INTERRUPT_TRIGGER, false);
        }

        rb.freezeRotation = lockRotation;
    }


    Dictionary<Stat.StatEnum, float> adjustedValues = new();

    void AdjustBaseValues()
    {
        Prestart();

        RecordAdjustedValue(Stat.StatEnum.MaxHealth, maxHealth);
        if (ActualMaxHealth < health)
        {
            health = ActualMaxHealth;
        }

        RecordAdjustedValue(Stat.StatEnum.MaxPower, maxPower);
        RecordAdjustedValue(Stat.StatEnum.PowerRegen, powerRegen);
        RecordAdjustedValue(Stat.StatEnum.PowerDelay, powerDelay); 
        RecordAdjustedValue(Stat.StatEnum.ControlResistance, controlResistance);
        RecordAdjustedValue(Stat.StatEnum.NumberOfCells, maxCells);
        if (cells > AdjustedValue(Stat.StatEnum.NumberOfCells))
        {
            cells = (int) AdjustedValue(Stat.StatEnum.NumberOfCells);
        }


        RecordAdjustedValue(Stat.StatEnum.ShieldPowerMultiplier, .05f);
        RecordAdjustedValue(Stat.StatEnum.ControlRechargeSpeed, 1f);

        RecordAdjustedValue(Stat.StatEnum.Size, startingLocalScale.magnitude);
        transform.localScale = startingLocalScale.normalized * AdjustedValue(Stat.StatEnum.Size);

        RecordAdjustedValue(Stat.StatEnum.Mass, startingMass);
        if (rb != null)
        {
            rb.mass = AdjustedValue(Stat.StatEnum.Mass);
        }

        RecordAdjustedValue(Stat.StatEnum.AnimationSpeed, 1f);

        if (animator != null)
        {
            animator.speed = AdjustedValue(Stat.StatEnum.AnimationSpeed);
        }
    }

    void RecordAdjustedValue(Stat.StatEnum stat, float baseValue)
    {
        adjustedValues[stat] = ShipStats.AdjustValue(baseValue, stats, stat);
    }

    public float AdjustedValue(Stat.StatEnum stat)
    {
        if (!adjustedValues.ContainsKey(stat))
        {
            AdjustBaseValues();
        }
        
        return adjustedValues[stat];
    }


    const float COLLISION_DAMAGE_MULT = .2f;
    const float MIN_COLLISION_DAMAGE = 2f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*
         *
        //OLD COLLISION BASED DAMAGE SYSTEM
        float magnitude = collision.relativeVelocity.magnitude;
        float relativeSize = collision.rigidbody.mass / (collision.rigidbody.mass + collision.otherRigidbody.mass);
        Vector3 point = collision.collider.ClosestPoint(transform.position);
        float damage = magnitude * relativeSize * COLLISION_DAMAGE_MULT;
        if (damage > MIN_COLLISION_DAMAGE && !immuneToKnockback)
            DealDamage(damage, point - transform.position, point, 0f, this);
        */
    }



    [Command(requiresAuthority = false)]
    void CmdUpdateStats(float health)
    {
        this.health = health;
        RpcUpdateStats(health);
    }
    [ClientRpc]
    void RpcUpdateStats(float health)
    {
        this.health = health;
    }


    public void ResetAllTriggers()
    {
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                if (param.name != INTERRUPT_TRIGGER)
                    animator.ResetTrigger(param.name);
            }
            if (param.type == AnimatorControllerParameterType.Float)
            {
                animator.SetFloat(param.name, 0f);
            }
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                if (param.name != INTERRUPT_TRIGGER)
                    animator.SetBool(param.name, false);
            }
        }
    }

    #region POWER
    [Command(channel = Channels.Unreliable, requiresAuthority = false)]
    void CmdSyncPower(float value)
    {
        RpcSyncPower(value);
    }
    [ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
    void RpcSyncPower(float value)
    {
        if (!IsAuthoritative)
            power = value;
    }

    public void CostPower(float amount)
    {
        power -= amount;
        currentPowerDelay = AdjustedValue(Stat.StatEnum.PowerDelay);
    }
    public void CostPower()
    {
        CostPower(1);
    }
    #endregion

    #region DAMAGE

    const float DEATH_CHECK_DELAY = .25f; //time after a death message is recieved to wait to confirm
    int killHitId = -1; //only set non -1 if you are actively dying, gets reset when enemy is reset
    //Issues with this system:
    //      Doesn't work if an enemy is killed by two projectiles in quick succession


    //Checks if a damage should be real or fake
    //If a player is shooting, they decide when a hit happens
    //If a player is getting shot, they decide when a hit happens (unless they are getting shot by another player)
    //If a player isn't involved, the server decides when a hit happens
    public static bool ShouldDealRealDamage(Ship origin, Ship target)
    {
        if (origin.isPlayer && origin == playerShip)
            return true;
        else if (origin.isPlayer)
            return false;
        //Origin is not player...
        if (target.isPlayer && target == playerShip)
            return true;
        else if (target.isPlayer)
            return false;

        //neither target nor origin is player
        return origin.isServer;
    }


    List<int> lastHitIDs = new();
    //Super script to call whenever something takes damage
    public void DealDamage(float amount, Vector3 hitDirection, Vector3 hitPosition, float knockback, Ship origin, Damage.Type damageType, HitType hitType, Damage.DamageTag tags)
    {
        AnyDamageTaken();
        if (!ShouldDealRealDamage(origin, this))
            return;

        ControlDamage(origin.stats, amount);

        amount = AdjustDamageAmount(amount, origin, damageType, hitType, tags);
        int hitID = Random.Range(int.MinValue, int.MaxValue);
        lastHitIDs.Add(hitID);

        if (isServer) //we are on the server, so we either hit an enemy as the host or we go hit by an enemy as the host///or maybe two NPCs hit each other
        {
            if (IsShielded)
            {

                AuthoritativeShieldDamage(amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject);
                RpcShieldDamage(hitID, amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject);
            }
            else
            {
                AuthoritativeDamage(hitID, amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject, false, true);
                RpcDamage(hitID, amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject, false);
            }

        }
        else if (origin.isPlayer && this != playerShip) //we are on the client who hit a nonplayer ship
        {

            if (IsShielded)
            {
                CmdShieldDamage(hitID, amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject);
            }
            else
            {
                bool shouldKill = amount >= health;
                CmdDamage(hitID, amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject, shouldKill);
                if (shouldKill)
                {
                    ActualUnauthoritativeKill();
                }
            }
                
        }
        else if (this == playerShip) //we are getting hit
        {
            if (IsShielded)
            {
                AuthoritativeShieldDamage(amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject);
                CmdShieldDamage(hitID, amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject);
            }
            else
            {
                AuthoritativeDamage(hitID, amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject, false, true);
                CmdDamage(hitID, amount, hitDirection, hitPosition, knockback, origin == null ? null : origin.gameObject, false);
            }
            
        }
        VisualDamage(amount, hitDirection, hitPosition);
    }

    //Any damage, including not real damage
    public void AnyDamageTaken()
    {
        if (killHitId != -1)
        {
            ActualUnauthoritativeKill();
        }
    }

    //Adjusts damage by modifiers
    //Such as scaling for additional players
    public float AdjustDamageAmount(float amount, Ship origin, Damage.Type damageType, HitType hitType, Damage.DamageTag tags)
    {
        float final = amount;
        if (!origin.playerTeam) //We are an enemy, so apply scaling
        {
            final *= Mathf.Pow(EnemyScaling.DAMAGE_MULT_PER_PLAYER, NetworkPlayer.players.Count - 1);
        }

        ShipStats originStats = origin == null ? null : origin.stats;

        switch (damageType)
        {
            case Damage.Type.DarkEnergy:
                final=ShipStats.AdjustDamageValue(amount, originStats, Stat.StatEnum.DarkEnergyDamage, stats, Stat.StatEnum.DarkEnergyIncoming);
                break;
            case Damage.Type.Energy:
                final = ShipStats.AdjustDamageValue(amount, originStats, Stat.StatEnum.EnergyDamage, stats, Stat.StatEnum.EnergyIncoming);
                break;
            case Damage.Type.Physical:
                final = ShipStats.AdjustDamageValue(amount, originStats, Stat.StatEnum.PhysicalDamage, stats, Stat.StatEnum.PhysicalIncoming);
                break;
            case Damage.Type.Plasma:
                final = ShipStats.AdjustDamageValue(amount, originStats, Stat.StatEnum.PlasmaDamage, stats, Stat.StatEnum.PlasmaIncoming);
                break;
            case Damage.Type.Quantum:
                final = ShipStats.AdjustDamageValue(amount, originStats, Stat.StatEnum.QuantumDamage, stats, Stat.StatEnum.QuantumIncoming);
                break;
        }

       

        switch (hitType)
        {
            case HitType.light:
                final = ShipStats.AdjustDamageValue(final, originStats, Stat.StatEnum.LightDamage, stats, Stat.StatEnum.LightDamageIncoming);
                break;
            case HitType.heavy:
                final = ShipStats.AdjustDamageValue(final, originStats, Stat.StatEnum.HeavyDamage, stats, Stat.StatEnum.HeavyDamageIncoming);
                break;
            case HitType.sustain:
                final = ShipStats.AdjustDamageValue(final, originStats, Stat.StatEnum.SustainDamage, stats, Stat.StatEnum.SustainDamageIncoming);
                break;
        }

        if (Damage.FitsTag(tags, Damage.DamageTag.Missile))
        {
            ShipStats.AdjustDamageValue(final, originStats, Stat.StatEnum.MissileDamage, stats, Stat.StatEnum.MissileDamageIncoming);
        }


        final = ShipStats.AdjustDamageValue(final, originStats, Stat.StatEnum.AllDamage, stats, Stat.StatEnum.AllDamageIncoming);
        return final;
    }

    [Command(requiresAuthority = false)]
    void CmdShieldDamage(int hitID, float amount, Vector3 hitDirection, Vector3 hitPosition, float knockback, GameObject origin)
    {
        RpcShieldDamage(hitID, amount, hitDirection, hitPosition, knockback, origin);
    }
    [ClientRpc]
    void RpcShieldDamage(int hitID, float amount, Vector3 hitDirection, Vector3 hitPosition, float knockback, GameObject origin)
    {
        if (lastHitIDs.Contains(hitID))
            return;
        lastHitIDs.Add( hitID);
        if (IsAuthoritative)
        {
            AuthoritativeShieldDamage(amount, hitDirection, hitPosition, knockback, origin);
        }
        VisualDamage( amount, hitDirection, hitPosition);
    }


    //Only called if not the server (obviously)
    [Command(requiresAuthority = false)]
    void CmdDamage(int hitID, float amount, Vector3 hitDirection, Vector3 hitPosition, float knockback, GameObject origin, bool guaranteeDeath)
    {
        RpcDamage(hitID, amount, hitDirection, hitPosition, knockback, origin, guaranteeDeath);
    }

    [ClientRpc]
    void RpcDamage(int hitID, float amount, Vector3 hitDirection, Vector3 hitPosition, float knockback, GameObject origin, bool guaranteeDeath)
    {
        if (lastHitIDs.Contains(hitID))
            return;
        lastHitIDs.Add(hitID);
        if (IsAuthoritative)
        {
            AuthoritativeDamage(hitID, amount, hitDirection, hitPosition, knockback, origin, guaranteeDeath, false);
        }
        ControlDamage(origin.GetComponent<ShipStats>(), amount);

        VisualDamage(amount, hitDirection, hitPosition);
    }

    //Run only if: (isPlayer && isLocalPlayer) || (!isPlayer && isServer)
    public void AuthoritativeDamage(int hitID, float amount, Vector3 hitDirection, Vector3 hitPosition, float knockback, GameObject origin, bool guaranteeDeath, bool authoratativeSource)
    {
        health -= amount;
        if (guaranteeDeath && health > 0)
        {
            health = 0;
        }
        if (health <= 0)
            Explode(hitID, authoratativeSource);

        AuthoritativeAnyDamage(amount, hitDirection, hitDirection, knockback, origin);
    }


    void AuthoritativeShieldDamage(float amount, Vector3 hitDirection, Vector3 hitPosition, float knockback, GameObject origin)
    {
        float augmentedAmount = amount;
        if (isPlayer && playerItems != null && playerItems.GetLastShieldEfficiency() != 0f)
        {
            augmentedAmount /= playerItems.GetLastShieldEfficiency();
        }

        bool sendCommand = playerShip.gameObject == origin && !IsAuthoritative;
        if (augmentedAmount > power)
        {
            LoseControl(2f, sendCommand);
        }
        CostPower(augmentedAmount);

        AuthoritativeAnyDamage(amount, hitDirection, hitPosition, knockback, origin);
    }

    void AuthoritativeAnyDamage(float amount, Vector3 hitDirection, Vector3 hitPosition, float knockback, GameObject origin)
    {
        if (origin != null)
            hitSignal?.Invoke(origin.GetComponent<Ship>());


        ApplyKnockback(knockback * (hitDirection).normalized);

        CmdUpdateStats(health);
    }

    //Run on all Clients and Server
    public void VisualDamage(float amount, Vector3 hitDirection, Vector3 hitPosition)
    {
        HitMarker.instance.CreateHit(hitPosition, hitDirection, amount, IsShielded);
        if (IsShielded)
        {
            if (shieldDamage != null)
            {
                shieldDamage.color = new Color(shieldDamage.color.r, shieldDamage.color.g, shieldDamage.color.b, 1f);
            }
        }
    }

    #region Damage Effects
    public void ControlDamage(ShipStats source, float amount)
    {
        ShipStats.AdjustValue(amount, source, Stat.StatEnum.ControlDamage);

        controlDamage += amount;
        if (!takesLocalHits || playerShip == null || playerShip.stats != source)
        {
            return; //Only the current player can do control damage.
        }
        bool sendCommand = playerShip != null && playerShip.gameObject == source.gameObject && !IsAuthoritative;

        if (controlDamage >= AdjustedValue(Stat.StatEnum.ControlResistance) * 2f)
        {
            LoseControl(2f, sendCommand);
            controlDamage = 0f;
        }
        else if (controlDamage >= AdjustedValue(Stat.StatEnum.ControlResistance))
        {
            LoseControl(1.5f, sendCommand);
            controlDamage = 0f;
        }
    }

    public void LoseControl(float time, bool sendCommand)
    {
        foreach (AudioSource source in sources)
        {
            source.Stop();
        }
        animator.SetBool(INTERRUPT_TRIGGER, true);
       
        Invoke(nameof(RegainControl), time);

        if (sendCommand)
        {
            CmdLoseControl(time);
        }
    }

    [Command]
    void CmdLoseControl(float time)
    {
        LoseControl(time, false);
    }

    void RegainControl()
    {
        animator.SetBool(INTERRUPT_TRIGGER, false);
    }

    public void ApplyKnockback(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
    }

#endregion


    //An enemy is killed by damage on someone else's screen
    public void EnemyHasDied(int hitID)
    {
        killHitId = hitID; // Delay actually killing until a hit is registered -- see AnyDamageTaken()
        Invoke(nameof(ActualUnauthoritativeKill), DEATH_CHECK_DELAY);
    }

    //Kill an enemy on our screen, but not necessarily anywhere else (only if we've sent the message!)
    public void ActualUnauthoritativeKill()
    {
        if (gameObject && gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            Explode(killHitId, false);
        }
    }

    //All (authoritative) ship death passes through explode
    //authoratativeHit - Checked if this has been hit by an authoratative source (so we should disable immediately rather than waiting for the next hit)
    public void Explode(int hitID, bool authoritativeHit)
    {
        
        if (IsAuthoritative)
        {
            HitMarker.instance.CreateExplosion(shipID, transform.position, transform.localScale.x * explosionSizeMult, (int) ShipStats.AdjustValue(reward, stats, Stat.StatEnum.KillReward));
            animator.SetBool(DEAD_TRIGGER, true);
            if (isPlayer)
            {
                animator.SetBool(INTERRUPT_TRIGGER, false);
                lastDeath = transform.position;
                playerDisabled = true;
            }
            else
            {
                CmdKill(hitID);
                DeathEvent?.Invoke();
                if (authoritativeHit)
                    gameObject.SetActive(false);
                else
                    EnemyHasDied(hitID);
            }
        }
        else
        {
            HitMarker.instance.CreateExplosion(shipID, transform.position, transform.localScale.x * explosionSizeMult, (int)ShipStats.AdjustValue(reward, stats, Stat.StatEnum.KillReward), false);
        }
           
            
    }


    //Only called from Explode
    [Command(requiresAuthority = false)]
    void CmdKill(int hitID)
    {
        //Don't destroy players!!!
        if (isPlayer)
        {
            playerDisabled = true;
            RpcPlayerDestroy();
        }
        else
        {
            if (DestroyOnDeath)
            {
                Invoke(nameof(DestroyNetworked), DEATH_CHECK_DELAY);
            }
            RpcKill(hitID);
        }
       
    }

    //Only run on server!!
    [Server]
    void DestroyNetworked()
    {
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    void RpcKill(int hitID)
    {
        if (gameObject)
        {
            EnemyHasDied(hitID);
            DeathEvent?.Invoke();
        }

    }

    public Vector3 lastDeath { get; private set; }
    [ClientRpc]

    void RpcPlayerDestroy()
    {
        if (isPlayer)
        {
            playerDisabled = true;
            
        }
    }
    #endregion

    #region REVIVE

    public void Revive()
    {
        animator.SetBool(DEAD_TRIGGER, false);
        health = AdjustedValue(Stat.StatEnum.MaxHealth) / 2f;
        killHitId = -1;
        HitMarker.instance.ClearMyShipID(shipID);
    }

    [Command(requiresAuthority = false)]
    public void CmdRevive()
    {
        RpcRevive();
    }

    [ClientRpc]
    void RpcRevive()
    {
        Revive();
    }
    [HideInInspector]
    public Ship shipToRevive;
    public void TriggerRevive(Ship shipToRevive)
    {
        if (shipToRevive.playerDisabled && isIdling && cells > 0)
        {
            animator.SetTrigger("Revive");
            this.shipToRevive = shipToRevive;
        }
    }

    [Command]
    public void CmdStartReviving(GameObject ship)
    {
        RpcStartReviving(ship);
    }
    [ClientRpc]
    public void RpcStartReviving(GameObject ship)
    {
        shipToRevive = ship.GetComponent<Ship>();
        if (shipToRevive == playerShip)
        {
            animator.SetTrigger("BeRevived");
        }
    }

    public void ActualRevive()
    {
        if (shipToRevive != null && shipToRevive.playerDisabled)
        {
            shipToRevive.CmdRevive();
            cells--;
        }
    }

    [Command]
    public void CmdDoneResetting()
    {
        isResetting = false;
        //networkTransform.enabled = true;
    }

    #endregion

    #region Enemy/Animation Reset

    public void ResetLastHits()
    {
        lastHitIDs.Clear();
    }

    public void AnimationReset()
    {
        if (ai != null && gameObject.activeInHierarchy && !ai.hasAggro)
            return;
        ResetAllTriggers();
        animator.Rebind();
        animator.Update(0f);
    }

    public void EnemyReset(float healthAmount)
    {
        if (ai != null && gameObject.activeInHierarchy && !ai.hasAggro)
            return;
        
        killHitId = -1;
        health = healthAmount;
        power = maxPower;
        controlDamage = 0f;
        AnimationReset();
        ResetLastHits();


    }



    #endregion

    #region Get Node

    float nodeTimer;
    const float nodeTime = .1f;

    void GetNode()
    {
        nodeTimer -= Time.deltaTime;
        if (nodeTimer > 0f)
        {
            return;
        }
        if (ai != null && !ai.aggro)
        {
            return; 
        }
        nodeTimer = nodeTime;
        if (node == null)
        {
            node = Node.FindNearestNode(transform.position);
        }
        else
            node = Node.FindNearestNode(transform.position, node);
    }

    #endregion

    #region Editor
    public void Precalculate()
    {
        startingNode = Node.FindNearestNode(transform.position);
    }

    #endregion
}
