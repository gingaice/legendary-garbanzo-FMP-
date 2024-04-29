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
using UnityEditor.Networking.PlayerConnection;
using Palmmedia.ReportGenerator.Core.Common;


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
    [SerializeField] private TMP_Dropdown _KickList;
    [SerializeField] private Button _KickButton;

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
        if (_connectedLobby != null) _connectedLobby = await LeaveLobby();

        if (_connectedLobby == null) _buttons.SetActive(true);
    }

    public async void JoinCodeLobby()
    {
        //await Authenticate(); 

        _connectedLobby = await JoinLobbyByCode(_joinInput.text);

        if(_connectedLobby != null) _buttons.SetActive(false);
    }


    public async void KickFromLobbyButton()
    {
        if (_connectedLobby != null) _connectedLobby = await KickedFromGame();

    }
    #endregion
    private async Task<Lobby> KickedFromGame()
    {
        //string kickedPlayerId = null;
        //NetworkManager.Singleton.ConnectedClients.Values = kickedPlayerId;
        try
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if(clientId.ToString() == _KickList.value.ToString())
                {
                    if(_connectedLobby.HostId == _KickList.value.ToString()) break;
                    NetworkManager.Singleton.DisconnectClient(clientId);
                }
            }

            KickedFromLobby();

            //_playerId = _KickList.value

            //await Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, kickedPlayerId);
            //NetworkManager.Singleton.DisconnectClient()
            return _connectedLobby;
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
            return null;
        }
    }

    private void KickedFromLobby()
    {
        var playerToRemove = _connectedLobby.Players[_KickList.value];

        Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, playerToRemove.Id);
        GameManager.Instance.restart();
        lobbyRestart();

    }

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

            _joinCodeText.text = lobby.LobbyCode;

            _KickList.gameObject.SetActive(false);
            _KickButton.gameObject.SetActive(false);
            //_KickList.AddOptions(new List<string> { _playerId});

            // Join the game room as a client
            NetworkManager.Singleton.StartClient();
            //GameManager.Instance.playersJoined.Add(_playerId);
            return lobby;
        }
        catch (Exception e)
        {
            Debug.Log($"No lobbies available via quick join: " + e);
            return null;
        }
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
            _joinCodeText.text = lobbyCode;

            _KickList.gameObject.SetActive(false);
            _KickButton.gameObject.SetActive(false);

            // Join the game room as a client
            NetworkManager.Singleton.StartClient();
            
            return lobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            return null;
        }
    }
    private async void refreshKickDD()
    {
        _KickList.ClearOptions();
        List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();

        string playerName = null;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            TMP_Dropdown.OptionData newData = new TMP_Dropdown.OptionData();
            playerName = clientId.ToString();
            
            //Debug.Log(clientId + " clizzy");
            newData.text = playerName;
            data.Add(newData);
        }

        if(data.Count == 0 ) 
        {
            TMP_Dropdown.OptionData newData = new TMP_Dropdown.OptionData();
            newData.text = "bah humbug";
            data.Add(newData);
        }
        _KickList.AddOptions(data);
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
            StartCoroutine(whosIn( 5));

            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
            NetworkManager.Singleton.StartHost();
            //GameManager.Instance.playersJoined.Add(_playerId);
            return lobby;
        }
        catch (Exception e)
        {
            Debug.LogFormat("Failed creating a lobby: " + e);
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
    
    private IEnumerator whosIn(float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            //foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            //{
            //Debug.Log(_KickList.value + " this valie is the one to look at");
            refreshKickDD();
            //}
            yield return delay;
        }
    }
    private async Task<Lobby> LeaveLobby()
    {
        try
        {
            StopAllCoroutines();
            if (_connectedLobby.HostId == _playerId)
            {

                //foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                //{
                //    if (_connectedLobby == null) _buttons.SetActive(true);
                //}
                await Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                NetworkManager.Singleton.Shutdown();
                await Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
            }
           
            //if (_connectedLobby == null) _buttons.SetActive(true);
            GameManager.Instance.restart();
            lobbyRestart();
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
    private void lobbyRestart()
    {
        _connectedLobby = null;
        _joinCodeText.text = null;
        if (_connectedLobby == null) _buttons.SetActive(true);
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


