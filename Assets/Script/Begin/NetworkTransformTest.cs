using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformTest : NetworkBehaviour //this script is to check how up to date the movement can be, they are meant to overlap
{
    public float theta;
    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            theta = Time.frameCount / 100.0f;
            transform.position = new Vector3((float)Mathf.Cos(theta), 0.0f, (float)Mathf.Sin(theta));
        }       
    }
}
