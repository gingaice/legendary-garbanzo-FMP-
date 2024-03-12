using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerTest : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Move();
        }
    }

    public void Move()
    {
        SubmitPositionRequestServerRpc();
    }

    //[Rpc(SendTo.Server)]
    //void SubmitPositionRequestServerRpc(RpcParams rpcParams = default)
    //{
    //    var randomPosition = GetRandomPositionOnPlane(); //calls the random spot
    //    transform.position = randomPosition; //changes the position of what this is attached too to where it should be
    //    Position.Value = randomPosition;//so that it is updatable over the network
    //}    
    
    [Rpc(SendTo.Server)]
    void SubmitPositionRequestServerRpc(RpcParams rpcParams = default)
    {
        var leftPosition = GetRandomPositionLeft(); //calls the random spot
        transform.position = leftPosition; //changes the position of what this is attached too to where it should be
        Position.Value = leftPosition;//so that it is updatable over the network
    }

    static Vector3 GetRandomPositionLeft()
    {
        return new Vector3(0f, 1f, Random.Range(-3f, 3f)); //random spot in the ranges
    }    
    //static Vector3 GetRandomPositionOnPlane()
    //{
    //    return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f)); //random spot in the ranges
    //}

    void Update()
    {
        transform.position = Position.Value; //just updates position to where the button changed it too
        /*
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.position = new Vector3(transform.position.x - 1.0f, transform.position.y, transform.position.z);
        }        
        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.position = new Vector3(transform.position.x + 1.0f, transform.position.y, transform.position.z);
        }
        */
    }
}

