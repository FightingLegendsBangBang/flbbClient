using System;
using Boo.Lang.Environments;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerData
    {
        public int networkId;
        public int playerId;
        public TestPlayerController playerController;
        public bool isHost;
        public string playerName;
        public Vector3 position;
        public Vector3 oldPosition;

        public PlayerData(int networkId, int playerId, bool isHost, string playerName,
            Vector3 position)
        {
            this.networkId = networkId;
            this.playerId = playerId;
            this.isHost = isHost;
            this.playerName = playerName;
            this.position = position;
        }

        public void ReadPlayerData(NetDataReader reader)
        {
            position.x = reader.GetFloat();
            position.y = reader.GetFloat();
            position.z = reader.GetFloat();
        }

        public void WritePlayerData(NetDataWriter writer)
        {

            writer.Put((ushort) 101);
            writer.Put(playerId);
            writer.Put(position.x);
            writer.Put(position.y);
            writer.Put(position.z);
        }

        public void CreatePlayerObject(GameObject p)
        {
            NetworkManager.Instance.playersToCreate.Add(
                new Tuple<int, int, GameObject, Vector3, Quaternion>(networkId, playerId, p, position,
                    Quaternion.identity));
        }
    }
}