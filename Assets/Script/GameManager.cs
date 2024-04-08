using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnLocalPlayerReadyChanged;

    private enum State
    {
        waitingToStart,
        GamePlaying,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.waitingToStart);
    private bool isLocalPlayerReady;
    private Dictionary<ulong, bool> PlayerReadyDictionary;


    private void Awake()
    {
        Instance = this;

        PlayerReadyDictionary = new Dictionary<ulong, bool>();
    }

    private void Update()
    {
        switch (state.Value)
        {
            case State.waitingToStart:
                if (isLocalPlayerReady)
                {
                    state.Value = State.GamePlaying; 
                }
                break;
            case State.GamePlaying:
                break;
        }

        readyButtonPressed();
    }

    private void OnButtonAction()
    {
        if(state.Value == State.waitingToStart)
        {
            isLocalPlayerReady = true;

            SetPlayerReadyServerRpc();

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public void readyButtonPressed()
    {
        OnButtonAction();
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
            state.Value = State.GamePlaying;
        }
    }
    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }
    public bool IsWaitingToStart()
    {
        return state.Value == State.waitingToStart;
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }
}
