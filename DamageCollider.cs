using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DamageCollider : SerializedMonoBehaviour
{
    public bool UseWeaponDamage;

    [HideIf(nameof(UseWeaponDamage))]
    public float damage;
    public Damage.Type damageType;
    public Damage.DamageTag damageTags;

    public bool playerTeam;
    public bool canHitAllies = false;
    public bool canHitEnemies = true;

    public float knockback;
    public Vector3 knockbackDirection = Vector3.up;

    [Tooltip("Use when knockback should be with relative positions between target and origin")]
    public bool knockbackUpIsAway = false;

    public Ship thisShip;
    public PlayerItems playerItems;

    [Tooltip("Time between hits.")]
    public float hitLockout = .5f;

    public HitType hitType;

    public Dictionary<Ship, float> hits = new();

    public List<StatusEffect> statusEffectsApplied = new List<StatusEffect>();

    // Start is called before the first frame update
    void Start()
    {
        thisShip = GetComponentInParent<Ship>();
        if (UseWeaponDamage)
        {
            playerItems = GetComponentInParent<PlayerItems>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Ship>(out var ship))
            return;
        if (hits.ContainsKey(ship))
        {
            if (hits[ship] +hitLockout > Time.time)
            {
                return;
            }
            else
                hits.Remove(ship);
        }
        if (ship.IsDodging)
            return;
        
        if ((ship.playerTeam != playerTeam && canHitEnemies) || 
            (ship.playerTeam == playerTeam && canHitAllies) )
        {
            hits.Add(ship, Time.time);
            Vector3 hitDirection = transform.localToWorldMatrix.MultiplyVector(knockbackDirection);
            if (knockbackUpIsAway)
            {
                Vector3 away = (Vector2) (ship.transform.position - thisShip.transform.position);
                Vector3 right= Vector2.Perpendicular(away);
                hitDirection = knockbackDirection.x * right + knockbackDirection.y * away;

            }
            ship.DealDamage(GetDamage(), hitDirection, collision.ClosestPoint(transform.position), knockback, thisShip, damageType, hitType, damageTags);
            if (statusEffectsApplied != null)
            {
                foreach (StatusEffect se in statusEffectsApplied)
                {
                    ship.stats.AddStatusEffect(se);
                }
            }

            
        }
            

    }

    public float GetDamage()
    {
        if (UseWeaponDamage && playerItems != null)
        {
            return playerItems.GetLastWeaponDamage(hitType);
        }
        return damage;
    }
}
