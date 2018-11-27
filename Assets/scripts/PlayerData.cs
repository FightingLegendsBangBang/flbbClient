using System;
using Boo.Lang.Environments;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;


public class PlayerData
{
    public int networkId;
    public int playerId;
    public bool isHost;
    public string playerName;
    public int characterId;
    public Color playerColor;


    public PlayerData(int networkId, int playerId, bool isHost, string playerName, int characterId, Color playerColor)
    {
        this.networkId = networkId;
        this.playerId = playerId;
        this.isHost = isHost;
        this.playerName = playerName;
        this.characterId = characterId;
        this.playerColor = playerColor;
    }

    public void SendUpdatedData()
    {
        var writer = new NetDataWriter();
        writer.Put((ushort) 4);
        writer.Put(playerId);
        writer.Put(isHost);
        writer.Put(playerName);
        writer.Put(characterId);
        writer.Put(playerColor.r);
        writer.Put(playerColor.g);
        writer.Put(playerColor.b);
        NetworkManager.Instance.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void ReadPlayerData(NetDataReader reader)
    {
        isHost = reader.GetBool();
        playerName = reader.GetString();
        characterId = reader.GetInt();
        playerColor = new Color(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
}