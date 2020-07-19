using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header( "- Actor" )]

    public StatusFloat HP;
    public StatusFloat AttackPower;

    public enum ETeamType
    {
        RED, BLUE, GREEN
    }
    public ETeamType TeamType { get; set; }

    #region UnityEvent

    protected virtual void Awake()
    {
        HP.Reset();
        AttackPower.Reset();
    }

    protected void OnTriggerEnter( Collider other )
    {

    }

    #endregion
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
            current = Mathf.Clamp( value, 0, Max );

            OnChangeCurrent?.Invoke( old, current );
        }
    }
    public delegate void DelChangeCurrent( float oldValue, float newValue );
    public event DelChangeCurrent OnChangeCurrent;

    public void Reset()
    {
        current = max;
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
}
