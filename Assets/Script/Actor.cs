using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Actor : MonoBehaviour
{
    [Header( "- Actor" )]

    public StatusFloat HP;
    public StatusFloat AttackPower;

    public ETeamType TeamType;

    internal Rigidbody _rigidbody;

    #region UnityEvent

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        HP.Reset();
        AttackPower.Reset();
    }

    #endregion

    public virtual void AddActorForce( Vector3 force, EForceType forceType )
    {
        if ( _rigidbody == null )
        {
            return;
        }

        switch ( forceType )
        {
            case EForceType.ADD:
            {
                _rigidbody.AddForce( force, ForceMode.VelocityChange );
            } break;

            case EForceType.SET:
            {
                _rigidbody.velocity = force;
            }
            break;

            case EForceType.SET_WITHOUT_Y:
            {
                _rigidbody.velocity = new Vector3( force.x, _rigidbody.velocity.y, force.z );
            }
            break;
        }
    }
}

public enum ETeamType
{
    RED, BLUE, GREEN, NONE,
}

public enum EForceType
{
    ADD, SET, SET_WITHOUT_Y
}

[Serializable]
public struct StatusFloat
{
    public float max;
    public float Max
    {
        get { return max; }
        set
        {
            if ( max == value )
            {
                return;
            }

            float old = max;
            max = value;
            current = Math.Min( current, max );

            OnChangeMax?.Invoke( old, max );
        }
    }
    public delegate void DelChangeMax( float oldValue, float newValue );
    public event DelChangeMax OnChangeMax;

    [HideInInspector]
    public float current;
    public float Current
    {
        get { return current; }
        set
        {
            if ( current == value )
            {
                return;
            }

            float old = current;
            current = Mathf.Clamp( value, 0.0f, Max );

            OnChangeCurrent?.Invoke( old, current );
        }
    }
    public delegate void DelChangeCurrent( float oldValue, float newValue );
    public event DelChangeCurrent OnChangeCurrent;

    public void Reset()
    {
        current = max;
    }

    public void SetZero()
    {
        current = 0.0f;
    }
}

[Serializable]
public struct StatusInt
{
    public int max;
    public int Max
    {
        get { return max; }
        set
        {
            if ( max == value )
            {
                return;
            }

            int old = max;
            max = value;
            current = Math.Min( current, max );

            OnChangeMax?.Invoke( old, max );
        }
    }
    public delegate void DelChangeMax( int oldValue, int newValue );
    public event DelChangeMax OnChangeMax;

    [HideInInspector]
    public int current;
    public int Current
    {
        get { return current; }
        set
        {
            if ( current == value )
            {
                return;
            }

            int old = current;
            current = Mathf.Clamp( value, 0, Max );

            OnChangeCurrent?.Invoke( old, current );
        }
    }
    public delegate void DelChangeCurrent( int oldValue, int newValue );
    public event DelChangeCurrent OnChangeCurrent;

    public void Reset()
    {
        current = max;
    }

    public void SetZero()
    {
        current = 0;
    }
}
