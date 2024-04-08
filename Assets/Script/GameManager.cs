using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum State
    {
        waitingToStart,
        GamePlaying,
    }

    private State state;
    private bool isLocalPlayerReady;
    private Dictionary<ulong, bool> PlayerReadyDictionary;


    private void Awake()
    {
        Instance = this;

        PlayerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Update()
    {
        switch (state)
        {
            case State.waitingToStart:
                if (isLocalPlayerReady)
                {
                    state = State.GamePlaying; 
                }
                break;
            case State.GamePlaying:
                break;
        }
    }
    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }    
    public bool IsWaitingToStart()
    {
        return state == State.waitingToStart;
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    [ServerRpc(RequireOwnership =false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        PlayerReadyDictionary[rpcParams.Receive.SenderClientId] = true;

        bool allPlayersReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!PlayerReadyDictionary.ContainsKey(clientId) || !PlayerReadyDictionary[clientId])
            {
                allPlayersReady = false;
                break;
            }
        }

        if(allPlayersReady)
        {
            state = State.GamePlaying;
        }
    }

}
