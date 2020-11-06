using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invector.vCharacterController;
using UnityEngine;

public class Projectile : Actor
{
    public Actor parent;
    
    public class HitInfo
    {
        public uint hitCount;
        public float hitDelay;
    }
    protected Dictionary<GameObject, HitInfo> hitInfos = new Dictionary<GameObject, HitInfo>();

    [Header( "- Hit" )]
    public uint maxHitCount;
    public float reHitDelay;
    public StatusFloat duration;

    [Header( "- Force" )]
    public EForceType forceType;
    public Vector3 forceDirection;
    public float forcePower;
    public float attractPower;
    public float repelPower;

    [Header( "- Damage" )]
    public float damage;

    protected override void Awake()
    {
        base.Awake();
        duration.Reset();
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

        foreach ( GameObject key in hitInfos.Keys )
        {
            hitInfos[ key ].hitDelay -= Time.deltaTime;
        }
    }

    protected void OnTriggerEnter( Collider other )
    {
        if ( hitInfos.ContainsKey( other.gameObject ) == false )
        {
            hitInfos.Add( other.gameObject, new HitInfo() );
            OnHit( other );
        }
    }

    protected void OnTriggerStay( Collider other )
    {
        if ( reHitDelay <= Mathf.Epsilon || hitInfos.ContainsKey( other.gameObject ) == false )
        {
            return;
        }

        HitInfo info = hitInfos[ other.gameObject ];
        if ( info.hitDelay <= 0.0f && ( info.hitCount <= maxHitCount || maxHitCount == 0 ) )
        {
            OnHit( other );
        }
    }

    protected void OnTriggerExit( Collider other )
    {
        if ( maxHitCount > 0 || hitInfos.ContainsKey( other.gameObject ) == false )
        {
            return;
        }

        hitInfos.Remove( other.gameObject );
    }


    protected void OnHit( Collider other )
    {
        Actor target = other.GetComponent<Actor>();
        if ( target == null )
        {
            Debug.LogError( "target is null. " + other );
            return;
        }

        if ( target.TeamType == ETeamType.NONE )
        {
            return;
        }

        if ( parent != null && parent.TeamType == target.TeamType )
        {
            return;
        }

        float prevHP = target.HP.Current;
        target.HP.Current -= GetTotalDamage();
        if ( forcePower > 0.0f )
        {
            target.AddActorForce( transform.rotation * forceDirection * forcePower, forceType );
        }

        if ( attractPower > 0.0f )
        {
            Vector3 direction = ( transform.position - target.transform.position ).normalized;
            target.AddActorForce( direction * forcePower, forceType );
        }

        if ( repelPower > 0.0f )
        {
            Vector3 direction = ( target.transform.position - transform.position ).normalized;
            target.AddActorForce( direction * forcePower, forceType );
        }

        HitInfo info = hitInfos[ other.gameObject ];
        ++info.hitCount;
        info.hitDelay = reHitDelay;

        if ( Mathf.Abs( prevHP - target.HP.Current ) > Mathf.Epsilon )
        {
            Debug.Log( other.gameObject.name + ", HP = " + target.HP.Current );
        }
    }

    protected float GetTotalDamage()
    {
        if ( parent == null )
        {
            return damage;
        }

        return damage * parent.AttackPower.Current;
    }
}
