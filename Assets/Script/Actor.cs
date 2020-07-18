using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    float HP
    {
        get;
        set;
    }
    float AttackPower
    {
        get;
        set;
    }
    enum TeamType
    {
        RED, BLUE, GREEN
    }
    TeamType Team
    {
        get;
        set;
    }

    private void OnTriggerEnter( Collider other )
    {

    }
}
