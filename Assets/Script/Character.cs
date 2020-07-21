using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Actor
{
    [Header( "- Character" )]

    public float TargetingRange = 50.0f;

    [HideInInspector]
    public Actor currentTarget;

    public virtual bool IsTargeting()
    {
        return currentTarget != null;
    }
}
