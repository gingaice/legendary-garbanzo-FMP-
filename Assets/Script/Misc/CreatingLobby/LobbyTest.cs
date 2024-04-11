using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyTest : MonoBehaviour
{
    private Lobby hostLobby;

    private string playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName = "bong" + Random.Range(1, 36);
        Debug.Log("Id is: " + playerName);
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "bingbong";
            int maxPlayers = 5;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions 
            {
                IsPrivate = true,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                { {"PlayerName" ,new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) } }
                },
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;

            StartCoroutine(HeartBeatLobbyCoroutine(lobby.Id, 15));

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
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

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter> {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0" , QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("lobbies found " + queryResponse.Results.Count);
            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void joinLobbyByCode(string lobbyCode)
    {
        try
        {
            await Lobbies.Instance.JoinLobbyByIdAsync(lobbyCode);

        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }

    }    
    
    private async void quickJoinLobby()
    {
        try
        {
            await Lobbies.Instance.QuickJoinLobbyAsync();

        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }

    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }
}
