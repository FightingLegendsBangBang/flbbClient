using System;
using Boo.Lang.Environments;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerData
    {
        public int playerId;
        public TestPlayerController playerController;
        public bool isHost;
        public string playerName;
        public Vector3 position;

        public PlayerData(int playerId, bool isHost, string playerName,
            Vector3 position)
        {
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
            float posX = position.x;
            float posY = position.y;
            float posZ = position.z;
            
            writer.Put((ushort) 101);
            writer.Put(playerId);
            writer.Put(posX);
            writer.Put(posY);
            writer.Put(posZ);
        }

        public void CreatePlayerObject(GameObject p)
        {
            NetworkManager.Instance.playersToCreate.Add(
                new Tuple<int, GameObject, Vector3, Quaternion>(playerId, p, position, Quaternion.identity));
        }
    }
}