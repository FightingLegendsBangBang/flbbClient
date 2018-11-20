using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using UnityEngine;

public class NetworkObjectData
{
    public int objectType;
    public int networkId;
    public int playerId;
    public int objectId;
    public INetworkObject networkObject;
    public Vector3 position;
    private Vector3 oldPosition;
    public Quaternion rotation;
    private Quaternion oldRotation;


    public NetworkObjectData(int objectType, int networkId, int playerId, int objectId, Vector3 position,
        Quaternion rotation)
    {
        this.objectType = objectType;
        this.networkId = networkId;
        this.playerId = playerId;
        this.objectId = objectId;
        this.position = position;
        this.rotation = rotation;
    }

    public void ReadData(NetDataReader reader)
    {
        position.x = reader.GetFloat();
        position.y = reader.GetFloat();
        position.z = reader.GetFloat();
        rotation.x = reader.GetFloat();
        rotation.y = reader.GetFloat();
        rotation.z = reader.GetFloat();
        rotation.w = reader.GetFloat();
    }

    public void WriteData(NetDataWriter writer)
    {
        writer.Put((ushort) 103);
        writer.Put(objectId);
        writer.Put(position.x);
        writer.Put(position.y);
        writer.Put(position.z);
        writer.Put(rotation.x);
        writer.Put(rotation.y);
        writer.Put(rotation.z);
        writer.Put(rotation.w);
    }

    public bool CheckChanges()
    {
        return position == oldPosition && rotation == oldRotation;
    }


    public void Instantiate(GameObject obj)
    {
        networkObject = GameObject.Instantiate(obj, position, rotation).GetComponent<INetworkObject>();
        networkObject.Init(playerId, objectId, networkId);
    }
}