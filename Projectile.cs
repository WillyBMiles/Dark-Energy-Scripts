using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Projectile : SerializedMonoBehaviour
{

    public Ship source;
    public float speed;
    public bool playerTeam;

    public bool canHitAllies = false;
    public bool canHitEnemies = true;

    public HitType hitType;
    public bool useItemDamage;
    [HideIf(nameof(useItemDamage))]
    public float damage;
    public Damage.Type damageType;
    public Damage.DamageTag damageTags;


    public float knockback;
    float timer = 10f;
    public float maxDistance = Mathf.Infinity;

    [Space(20)]
    [Header("Optional")]
    public AudioClip hitClip;
    public GameObject createOnDestroy;

    public List<StatusEffect> statusEffectsApplied = new List<StatusEffect>();

    Vector3 startPosition;
    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position += speed * Time.deltaTime * transform.up;
        timer -= Time.deltaTime;
        if (timer<= 0f)
        {
            DestroyThis();
        }
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            DestroyThis();
        }
    }

    void DestroyThis()
    {
        if (hitClip)
        {
            GlobalSound.PlaySound(transform.position, hitClip);
        }
        if (createOnDestroy)
        {
            Instantiate(createOnDestroy, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Ship ship = collision.GetComponent<Ship>();
        if (ship != null && ((ship.playerTeam != playerTeam && canHitEnemies) ||
            (ship.playerTeam == playerTeam && canHitAllies))

            && !ship.IsDodging)
        {
            ship.DealDamage(damage, transform.up, transform.position, knockback, source, damageType, hitType, damageTags);

            if (statusEffectsApplied != null)
            {
                foreach (StatusEffect se in statusEffectsApplied)
                {
                    ship.stats.AddStatusEffect(se);
                }
            }

            DestroyThis();
        }
        if (ship == null && collision.gameObject.layer != LayerMask.NameToLayer("HitBox")
            && collision.gameObject.layer != LayerMask.NameToLayer("Ship"))
        {
            DestroyThis();
        }
    }
}
