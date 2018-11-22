using LiteNetLib.Utils;
using UnityEngine;

public class PlayerObjectData : NetworkObjectData
{
    public PlayerObjectData(int objectType, int networkId, int playerId, int objectId, Vector3 position,
        Quaternion rotation) : base(objectType, networkId, playerId, objectId, position, rotation)
    {
    }

    public override void ReadData(NetDataReader reader)
    {
        base.ReadData(reader);
        //Debug.Log(reader.GetString());
    }

    public override void WriteData(NetDataWriter writer)
    {
        base.WriteData(writer);
        //writer.Put("this is random test data");
    }

    public override bool CheckChanges()
    {
        return base.CheckChanges();
    }
}