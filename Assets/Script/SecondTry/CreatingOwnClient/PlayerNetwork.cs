using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerNetwork : NetworkBehaviour
{
    private NetworkVariable<PlayerNetworkData> _netstate = new NetworkVariable<PlayerNetworkData>(writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<Quaternion> _netRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 _vel;
    private float _rotVel;
    [SerializeField]
    private float _interpolationSpeed = 0.1f;
    void Update()
    {
        if (IsOwner)
        { //this is for sending
            _netstate.Value = new PlayerNetworkData()
            {
                Pos = transform.position,
                Rot = transform.rotation.eulerAngles
            };
        }
        else
        { //this is for reading // adding some interpolation to make the characters move smoother on the other instance
            transform.position = Vector3.SmoothDamp(transform.position, _netstate.Value.Pos, ref _vel, _interpolationSpeed);
            transform.rotation = Quaternion.Euler(0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _netstate.Value.Rot.y, ref _rotVel, _interpolationSpeed),0);
        }
    }

    struct PlayerNetworkData : INetworkSerializable //this is to speed up the data being sent over the internet to stop it being choppy and is supported by the networkserialize below
    {
        private float _X, _Z;
        private float _YRot;


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
            get => new Vector3 (0, _YRot, 0);

            set
            {
                _YRot = value.y;
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


/*
private NetworkVariable<Quaternion> _netRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
private NetworkVariable<Vector3> _netRot = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);

void update(){
if(IsOwner){
_netpos.value = transform.position;
_netrot.value = transform.rotation;
}}
else{
transform.position = _netpos.value;
transform.rotation = _netRot.value;
}
*/