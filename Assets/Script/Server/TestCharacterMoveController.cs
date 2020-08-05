using System;
using System.Collections.Generic;
using UnityEngine;

class TestCharacterMoveController : MonoBehaviour
{
    private float moveSpeed = 10;

    private void Update()
    {
        if ( Input.GetKey(KeyCode.W ) )
        {
            transform.Translate( transform.forward * moveSpeed * Time.deltaTime );
        }
        if ( Input.GetKey( KeyCode.S ) )
        {
            transform.Translate( -transform.forward * moveSpeed * Time.deltaTime );
        }
        if ( Input.GetKey( KeyCode.D ) )
        {
            transform.Translate( transform.right * moveSpeed * Time.deltaTime );
        }
        if ( Input.GetKey( KeyCode.A ) )
        {
            transform.Translate( -transform.right * moveSpeed * Time.deltaTime );
        }
    }
}