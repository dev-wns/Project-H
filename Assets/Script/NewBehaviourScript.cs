using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    Transform myTransform;
    // Start is called before the first frame update
    void Start()
    {
        myTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Hello JunHwan!");
        myTransform.Rotate( 0.0f, 100.0f * Time.deltaTime, 0.0f );
    }
}
