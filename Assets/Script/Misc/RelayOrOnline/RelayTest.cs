using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayTest : MonoBehaviour
{
    [SerializeField] private TMP_Text _joinCodeText;
    [SerializeField] private TMP_InputField _joinInput;
    [SerializeField] private GameObject _buttons;

    private UnityTransport _transport; //this is to connect it too the relay servers -- as the unity relay is a connection in the middle to save port forwarding and getting firewalls protected
    private const int MaxPlayers = 5;

    private async void Awake()
    {
        _transport = FindObjectOfType<UnityTransport>();

        _buttons.SetActive(false);

        await Authenticate();

        _buttons.SetActive(true);
    }

    private static async Task Authenticate() //this is to connect to unities online system (A cheat system through netwokring)
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //doesnt mena to log them in such as krunker you can have a player save there profile but for now anonympus works perfect
    }

    public async void CreateGame()
    {
        _buttons.SetActive(false);

        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers); //can add an extra parameter which would decide which region i want it in... since it is null it chooses the best one for me (eu)
        _joinCodeText.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId); //changes the text to send to a friend which saves time sending over IP's

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData); //this is using unity transport but if i were to put it on steam or other services may have to change to use there certain version of information transport

        NetworkManager.Singleton.StartHost(); //create as a host.
    }

    public async void JoinGame()
    {
        _buttons.SetActive(false);

        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(_joinInput.text); //checks that the server is created and like made.

        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }
}
