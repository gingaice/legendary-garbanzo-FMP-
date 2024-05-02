using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private NetworkVariable<PlayerNetworkData> _netstate = new NetworkVariable<PlayerNetworkData>(writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<Quaternion> _netRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 _vel;
    private float _rotVel;

    [SerializeField]
    private float _interpolationSpeed = 0.1f;
    [SerializeField]
    private bool _serverAuth;

    private Rigidbody _rigidBody;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();

        var permission = _serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner; // if serverauth is true then only the server can change _netstate stuff
        _netstate = new NetworkVariable<PlayerNetworkData> (writePerm: permission);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            //Destroy(transform.GetComponent<PlayerMovement>()); //this was bringing up errors as it was probably still trying to talk to the server in some way?
            transform.GetComponent<PlayerMovement>().enabled = false;
        }
    }
    void Update()
    {
        if (IsOwner)
        {
            TransmitState();
        }
        else
        {
            ConsumeState();
        }

        //if (IsOwner)
        //{ //this is for sending
        //    _netstate.Value = new PlayerNetworkData()
        //    {
        //        Pos = _rigidBody.position,
        //        Rot = transform.rotation.eulerAngles
        //    };
        //}
        //else
        //{ //this is for reading // adding some interpolation to make the characters move smoother on the other instance //do a better interpolation later pls future me
        //    _rigidBody.MovePosition(Vector3.SmoothDamp(transform.position, _netstate.Value.Pos, ref _vel, _interpolationSpeed));
        //    transform.rotation = Quaternion.Euler(0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _netstate.Value.Rot.y, ref _rotVel, _interpolationSpeed),0);
        //}
    }

    private void TransmitState() //to control sending the players data to the server
    {
        var state = new PlayerNetworkData
        {
            Pos = _rigidBody.position,
            Rot = transform.rotation.eulerAngles
        };

        if(IsServer || !_serverAuth)
        {
            _netstate.Value = state;
        }
        else
        {
            TransmitStateServerRpc(state);
        }
    }

    [ServerRpc]
    private void TransmitStateServerRpc(PlayerNetworkData state)
    {
        _netstate.Value = state;
    }

    private void ConsumeState() //to recieve the data sent from the server
    {
        _rigidBody.MovePosition(Vector3.SmoothDamp(transform.position, _netstate.Value.Pos, ref _vel, _interpolationSpeed));
        transform.rotation = Quaternion.Euler(0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _netstate.Value.Rot.y, ref _rotVel, _interpolationSpeed),0);
    }

    struct PlayerNetworkData : INetworkSerializable //this is to speed up the data being sent over the internet to stop it being choppy and is supported by the networkserialize below
    {
        private float _X, _Z;
        private short _YRot; //short is used to send less bytes over the web although a small amount as a short is equal too 2 bytes

        internal Vector3 Pos //internal is used when the designer dont want to show the variable everywhere in public but it still allows access outside of the function in the code.
        {
            get => new Vector3(_X, 0, _Z); 

            set
            {
                _X = value.x; //simple getter setter to change the location of the characters
                _Z = value.z;
            }
        }        
        internal Vector3 Rot
        {
            get => new (0, _YRot, 0);

            set
            {
                _YRot = (short)value.y;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _X);
            serializer.SerializeValue(ref _Z);
            serializer.SerializeValue(ref _YRot);
        }
    }

}
