using ParrelSync;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class LobbyTest : MonoBehaviour
{
    [SerializeField] private GameObject _buttons;

    private Lobby _connectedLobby;
    private QueryResponse _lobbies;
    private UnityTransport _transport;
    private const string JoinCodeKey = "j";
    private string _playerId;

    [SerializeField] private TMP_Text _joinCodeText;
    [SerializeField] private TMP_InputField _joinInput;
    [SerializeField] private Toggle _privGame;
    private async void Awake()
    {
        _transport = FindObjectOfType<UnityTransport>();
        await Authenticate();
    } 

    #region buttons
    public async void CreateLobbyButton()
    {
        //await Authenticate();

        _connectedLobby = await CreateLobby();

        if (_connectedLobby != null) _buttons.SetActive(false);
    }
    public async void QuickJoinLobbyButton()
    {

        _connectedLobby = await QuickJoinLobby();

        if (_connectedLobby != null) _buttons.SetActive(false);
    }    
    
    public async void LeaveLobbyButton()
    {
        if(_connectedLobby != null) _connectedLobby = await LeaveLobby();

        if (_connectedLobby == null) _buttons.SetActive(true);
    }
    public async void JoinCodeLobby()
    {
        //await Authenticate(); 

        _connectedLobby = await JoinLobbyByCode(_joinInput.text);

        if(_connectedLobby != null) _buttons.SetActive(false);
    }

    private async Task<Lobby> JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            //await Lobbies.Instance.JoinLobbyByIdAsync(_joinInput.text);

            var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

            //await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);
            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            SetTransformAsClient(a);

            NetworkManager.Singleton.StartClient();
            return lobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            return null;
        }
    }


    #endregion
    private async Task Authenticate()
    {
        var options = new InitializationOptions();

        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");

        await UnityServices.InitializeAsync(options);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        _playerId = AuthenticationService.Instance.PlayerId;
    }

    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            // Attempt to join a lobby in progress
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

            // If we found one, grab the relay allocation details
            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            // Set the details to the transform
            SetTransformAsClient(a);

            // Join the game room as a client
            NetworkManager.Singleton.StartClient();
            return lobby;
        }
        catch (Exception e)
        {
            Debug.Log($"No lobbies available via quick join");
            return null;
        }
    }    
    
    private async Task<Lobby> CreateLobby()
    {
        try
        {
            const int maxPlayers = 5;

            // Create a relay allocation and generate a join code to share with the lobby
            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId); //creates relay join code

            //DataObject.VisibilityOptions t = (_privGame.isOn ? DataObject.VisibilityOptions.Public : DataObject.VisibilityOptions.Public) ; for changing the enetire lobby into only host allowed in to see

                // Create a lobby, adding the relay join code to the lobby data
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } } //join code should be added into lobby data
            };

            if (_privGame.isOn)
            {
                options.IsPrivate = true;

            }
            else
            {
                options.IsPrivate = false;
            }

            var lobby = await Lobbies.Instance.CreateLobbyAsync("Useless Lobby Name", maxPlayers, options);

            //_joinCodeText.text = joinCode;
            _joinCodeText.text = lobby.LobbyCode; //lobby.lobbycode it to connect to a pre existing lobby

            // Send a heartbeat every 15 seconds to keep the room alive
            StartCoroutine(HeartBeatLobbyCoroutine(lobby.Id, 15)); //after 30 seconds of inactivity it auto closes
            StartCoroutine(HeartBeatLobbyCoroutine(lobby.Id, 1.1f)); //after 30 seconds of inactivity it auto closes

            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            NetworkManager.Singleton.StartHost();
            return lobby;
        }
        catch (Exception e)
        {
            Debug.LogFormat("Failed creating a lobby");
            return null;
        }
    }

    private void SetTransformAsClient(JoinAllocation a)
    {
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }

    private IEnumerator HeartBeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
    private IEnumerator LobbyPollCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.GetLobbyAsync(lobbyId);
            
            yield return delay;
        }
    }

    private async Task<Lobby> LeaveLobby()
    {
        try
        {
            if (_connectedLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                //LeaveLobbyButton();
                await Lobbies.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                NetworkManager.Singleton.Shutdown();

            }
            else
            {
                await Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            
            _connectedLobby = null;
            return _connectedLobby;
        }
        catch(LobbyServiceException ex) 
        { 
            Debug.LogException(ex);
            return null;
        }
    }

    private void OnDestroy()
    {
        try
        {
            StopAllCoroutines();
            // todo: Add a check to see if you're host
            if (_connectedLobby != null)
            {
                if (_connectedLobby.HostId == _playerId) Lobbies.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                else Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error shutting down lobby: {e}");
        }
    }
}

//{
//    private Lobby hostLobby;

//    private string playerName;

//    private async void Start()
//    {
//        await UnityServices.InitializeAsync();

//        AuthenticationService.Instance.SignedIn += () =>
//        {
//            Debug.Log("signed in " + AuthenticationService.Instance.PlayerId);
//        };

//        await AuthenticationService.Instance.SignInAnonymouslyAsync();
//        playerName = "bong" + Random.Range(1, 36);
//        Debug.Log("Id is: " + playerName);
//    }


//    private IEnumerator HeartBeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
//    {
//        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
//        while (true)
//        {
//            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
//            yield return delay;
//        }
//    }


