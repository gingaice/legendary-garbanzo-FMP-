using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; } //singleton

    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnPause; //using events to tell the rest of the players whats going on by openly giving out information to anyone listening ( checkout dms with mo for a good reference)
    public event EventHandler OnUnpause;

    public Material testmat;
    private enum State
    {
        waitingToStart,
        GamePlaying,// current only states will need a end game state soon for a timer that will be synced and shared across the game space
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.waitingToStart); //chooses the first state incase it tries to switch to a different one from load
    private bool isLocalPlayerReady = false;
    private Dictionary<ulong, bool> PlayerReadyDictionary;
    private Dictionary<ulong, bool> PlayerPauseDictionary;
    private bool isLocalPlayerPaused = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);

    private void Awake()
    {
        Instance = this;
        testmat.color = Color.gray;
        PlayerReadyDictionary = new Dictionary<ulong, bool>(); //use a dictionary to hold a large amount of different numbers of them, so infinite players (although i maxed it at 5)
        PlayerPauseDictionary = new Dictionary<ulong, bool>(); //use ulong as it then fits the clientid inside of it instead of using a string with can run out and do a 0x0004
    }

    private void Update()
    {
        switch (state.Value)
        {
            case State.waitingToStart: //currently doesnt need to do anything in this section as its for gaining players in the "lobby"
                //if (isLocalPlayerReady)
                //{
                //    state.Value = State.GamePlaying; 
                //}
                break;
            case State.GamePlaying: //i change the color to red so that it proves that it enters this next state will eventually be able to change this with a timer so that the game will eventually end
                testmat.color = Color.red; 
                break;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePaused();
        }
    }
    private void TogglePaused()
    {
        isLocalPlayerPaused = !isLocalPlayerPaused; //each time escape is pressed it jumps between these two
        if (isLocalPlayerPaused)
        {
            PauseGameServerRpc();
            Time.timeScale = 0; //this is for the local player as later on it will only freeze it for other players on the server
            OnPause?.Invoke(this, EventArgs.Empty); //this sends the information to everyone listening too it but it ignores EVERYTHING if they dont want to listen
        }
        else
        {
            UnpauseGameServerRpc();
            Time.timeScale = 1; // ^^
            OnUnpause?.Invoke(this, EventArgs.Empty);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams rpcParams = default)
    {
        PlayerPauseDictionary[rpcParams.Receive.SenderClientId] = true; //this calls to tell the server that it has paused the players that it has found
        TestGamePause();
    }    
    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams rpcParams = default)
    {
        PlayerPauseDictionary[rpcParams.Receive.SenderClientId] = false;
        TestGamePause();
    }

    private void TestGamePause()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(PlayerPauseDictionary.ContainsKey(clientId) && PlayerPauseDictionary[clientId])
            {
                //means that this local player paused, which then would turn isGAmePaused.value true for everyone in turn making the time timescale none so it freezes
                isGamePaused.Value = true;
                Time.timeScale = 0;
                return;
            }
        }

        isGamePaused.Value = false;
        Time.timeScale = 1;
    }
    private void OnButtonAction()
    {
        if(state.Value == State.waitingToStart) //if they havent moved on already then it allows them into this state
        {
            isLocalPlayerReady = true;

            SetPlayerReadyServerRpc();

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty); //this logs that the locla player has clicked the button saying that they are ready to move into the actual game
        }
    }

    public void readyButtonPressed()
    {
        OnButtonAction();
    }

    [ServerRpc(RequireOwnership =false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        PlayerReadyDictionary[rpcParams.Receive.SenderClientId] = true; //adds the local player to the dictionary of being ready to move into the playing stage

        bool allPlayersReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(!PlayerReadyDictionary.ContainsKey(clientId) || !PlayerReadyDictionary[clientId])
            {
                allPlayersReady = false; //after the player clicks the button it will run through all of the current players and check if they have also checked it
                break;
            }
        }

        if(allPlayersReady)
        {
            state.Value = State.GamePlaying; //changes the state into playing which will chang ethe floor currently
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
