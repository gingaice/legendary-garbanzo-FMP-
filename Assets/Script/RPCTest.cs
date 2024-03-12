using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RPCTest : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if(!IsServer && IsOwner)
        {
            TestServerRpc(0, NetworkObjectId); //creates the host/server which would be classed as 0 as its first in the list
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void TestClientRPC(int value, ulong sourceNetowkrObjectId)
    {
        Debug.Log($"Clinet Revieved by the RPC #{value} on network object #{NetworkObjectId}");
        if (IsOwner)
        {
            TestServerRpc(value +1, sourceNetowkrObjectId); //adds it so it is a different client to the last one added
        }
    }

    [Rpc(SendTo.Server)]
    void TestServerRpc(int value, ulong sourceNetowkrObjectId)
    {
        Debug.Log($"Server Revieved the RPC #{value} on network object {sourceNetowkrObjectId}");
        TestClientRPC(value, sourceNetowkrObjectId);
    }
}
