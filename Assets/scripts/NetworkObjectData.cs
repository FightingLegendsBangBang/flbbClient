using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectData
{
    public int networkID;
    public int playerID;
    public int objectID;
    public Vector3 position;
    private Vector3 oldPosition;
    public Quaternion rotation;
    private Quaternion oldRotation;


    public NetworkObjectData(int networkId, int playerId, int objectId, Vector3 position, Quaternion rotation)
    {
        networkID = networkId;
        playerID = playerId;
        objectID = objectId;
        this.position = position;
        this.rotation = rotation;
    }
}
