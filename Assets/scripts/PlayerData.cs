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

    public PlayerData(int networkId, int playerId, bool isHost, string playerName)
    {
        this.networkId = networkId;
        this.playerId = playerId;
        this.isHost = isHost;
        this.playerName = playerName;
    }
}