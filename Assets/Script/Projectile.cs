using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Collider myCollider;
    public Actor parent;

    [Header( "- Hit" )]
    public StatusFloat reHitDelay;
    public StatusFloat duration;

    [Header( "- Force" )]
    public ForceMode forceMode;
    public Vector3 forceDirection;
    public float forcePower;

    [Header( "- Damage" )]
    public float damage;

    void Awake()
    {
        myCollider = gameObject.GetComponent<Collider>();
        duration.Reset();
        reHitDelay.SetZero();
        forceDirection.Normalize();
    }

    void Update()
    {
        if ( duration.Max > 0.0f )
        {
            if ( duration.Current <= 0.0f )
            {
                gameObject.SetActive( false );
                return;
            }
            duration.current -= Time.deltaTime;
        }

        reHitDelay.current -= Time.deltaTime;
    }

    protected void OnTriggerEnter( Collider other )
    {
        if ( reHitDelay.Current <= 0.0f )
        {
            OnHit( other );
        }
    }

    protected void OnTriggerStay( Collider other )
    {
        if ( reHitDelay.Max <= 0.0f )
        {
            return;
        }

        if ( reHitDelay.Current <= 0.0f )
        {
            OnHit( other );
        }
    }

    protected void OnHit( Collider other )
    {
        Actor target = other.GetComponent<Actor>();
        if ( target == null )
        {
            Debug.LogError( "target is null. " + other );
            return;
        }

        if ( parent != null && parent.TeamType == target.TeamType )
        {
            return;
        }

        float totalDamage = damage;
        if ( parent != null )
        {
            totalDamage *= parent.AttackPower.Current;
        }

        target.HP.Current -= totalDamage;

        if ( forcePower > 0.0f )
        {
            Rigidbody rigidBody = other.GetComponent<Rigidbody>();
            Vector3 newDirection = transform.rotation * forceDirection;
            rigidBody?.AddForce( newDirection * forcePower, forceMode );
        }

        Debug.Log( other.gameObject + ", hp = " + target.HP.Current );

        reHitDelay.Reset();
    }
}
