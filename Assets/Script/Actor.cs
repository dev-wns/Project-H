using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header( "- Actor" )]

    public float maxHP;
    public float MaxHP
    {
        get { return maxHP; }
        set { maxHP = value; }
    }
    protected float currentHP;
    public float HP
    {
        get { return currentHP; }
        set { currentHP = Mathf.Clamp( value, 0, MaxHP ); }
    }

    public float AttackPower { get; set; }

    public enum ETeamType
    {
        RED, BLUE, GREEN
    }
    public ETeamType TeamType { get; set; }

    private void OnTriggerEnter( Collider other )
    {

    }
}
