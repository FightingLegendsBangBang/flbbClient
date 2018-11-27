using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteNetLib;
using System.Threading;
using System.Threading.Tasks;
using DefaultNamespace;
using LiteNetLib.Utils;
using Random = UnityEngine.Random;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    public Thread networkThread;
    public Thread mainThread;
    public EventBasedNetListener networkListener;
    public NetManager client;
    public readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
    public int MyNetworkId;
    private string _myName;
    private Color _myColor;
    public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();
    public Dictionary<int, NetworkObjectData> NetworkObjects = new Dictionary<int, NetworkObjectData>();
    public GameObject[] networkPrefabs;

    private ObjectDataFactory objectDataFactory = new ObjectDataFactory();

    private List<Tuple<string, int, byte[]>> rpcList = new List<Tuple<string, int, byte[]>>();
    private List<NetworkObjectData> objectsToCreate = new List<NetworkObjectData>();
    private List<int> objectsToDestroy = new List<int>();
    private int LevelToLoad = -1;


    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        _myName = "player " + Random.Range(1000, 9999);
        _myColor = new Color(Random.value, Random.value, Random.value);
        mainThread = Thread.CurrentThread;
        networkThread = new Thread(UpdateNetwork);
        networkThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (objectsToCreate.Count > 0)
        {
            foreach (var obj in objectsToCreate)
            {
                obj.Instantiate(networkPrefabs[obj.objectType]);
                NetworkObjects.Add(obj.objectId, obj);
            }

            objectsToCreate.Clear();
        }

        if (objectsToDestroy.Count > 0)
        {
            foreach (var obj in objectsToDestroy)
            {
                Destroy(NetworkObjects[obj].networkObject.gameObject);
                NetworkObjects.Remove(obj);
            }

            objectsToDestroy.Clear();
        }

        if (rpcList.Count > 0)
        {
            foreach (var rpc in rpcList)
            {
                NetworkObjects[rpc.Item2].networkObject
                    .SendMessage(rpc.Item1, rpc.Item3, SendMessageOptions.DontRequireReceiver);
            }

            rpcList.Clear();
        }

        if (LevelToLoad != -1)
        {
            InfoManager.Instance.Levels[LevelToLoad].LoadLevel();

            foreach (var player in Players)
            {
                if (player.Value.networkId == MyNetworkId)
                {
                    InstantiateNetworkObject(InfoManager.Instance.Characters[player.Value.characterId].CharacterPrefabId, player.Key, Vector3.zero, Quaternion.identity);
                }
            }

            LevelToLoad = -1;
        }
    }

    void UpdateNetwork()
    {
        networkListener = new EventBasedNetListener();

        networkListener.NetworkReceiveEvent += ReceivePackage;

        client = new NetManager(networkListener);
        client.Start();
        client.Connect("localhost", 9050, "SomeConnectionKey");
        NetDataWriter writer = new NetDataWriter();
        while (true)
        {
            foreach (var obj in NetworkObjects)
            {
                if (obj.Value.networkId == MyNetworkId)
                {
                    if (!obj.Value.CheckChanges())
                    {
                        writer.Reset();
                        obj.Value.WriteData(writer);
                        Send(writer, DeliveryMethod.Unreliable);
                    }
                }
            }

            client.PollEvents();
            Thread.Sleep(20);
        }

        client.Stop();
    }

    void ReceivePackage(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        client.Statistics.BytesReceived += (ulong) reader.RawDataSize;
        client.Statistics.PacketsReceived++;


        NetDataWriter writer = new NetDataWriter();
        ushort msgid = reader.GetUShort();
        Debug.Log(msgid);
        switch (msgid)
        {
            case 1: //register to server
                var networkId = reader.GetInt();
                var playerId = reader.GetInt();
                var isHost = reader.GetBool();
                var charId = 0;
                var playerColor = _myColor;
                var pName = _myName;
                MyNetworkId = networkId;


                InitPlayer(networkId, playerId, pName, isHost, charId, playerColor);


                writer.Put((ushort) 1);
                writer.Put(playerId);
                writer.Put(pName);
                writer.Put(isHost);
                writer.Put(charId);
                writer.Put(playerColor.r);
                writer.Put(playerColor.g);
                writer.Put(playerColor.b);

                Send(writer, DeliveryMethod.ReliableOrdered);

                writer.Reset();
                LobbyManager.Instance.NeedUpdate = true;
                break;

            case 2: //register new player;
                var newNetworkId = reader.GetInt();
                var newPlayerId = reader.GetInt();
                var npName = reader.GetString();
                var nIsHost = reader.GetBool();
                var newCharId = reader.GetInt();
                var newPlayerColor = new Color(
                    reader.GetFloat(),
                    reader.GetFloat(),
                    reader.GetFloat());
                InitPlayer(newNetworkId, newPlayerId, npName, nIsHost, newCharId, newPlayerColor);

                LobbyManager.Instance.NeedUpdate = true;

                break;
            case 3: //remove disconnected player
                var rnid = reader.GetInt();
                List<int> plrs = new List<int>();
                foreach (var pl in Players)
                {
                    if (pl.Value.networkId == rnid)
                        plrs.Add(pl.Key);
                }

                foreach (var i in plrs)
                {
                    Players.Remove(i);
                }

                foreach (var no in NetworkObjects)
                {
                    if (no.Value.networkId == rnid)
                        objectsToDestroy.Add(no.Key);
                }

                LobbyManager.Instance.NeedUpdate = true;

                break;
            case 4:
                Players[reader.GetInt()].ReadPlayerData(reader);
                LobbyManager.Instance.NeedUpdate = true;
                break;
            case 101:
                var OBobjectType = reader.GetInt();
                var OBobjectId = reader.GetInt();
                var OBplayerId = reader.GetInt();
                var OBnetId = reader.GetInt();
                var OBposX = reader.GetFloat();
                var OBposY = reader.GetFloat();
                var OBposZ = reader.GetFloat();
                var OBrotX = reader.GetFloat();
                var OBrotY = reader.GetFloat();
                var OBrotZ = reader.GetFloat();
                var OBrotW = reader.GetFloat();
                var OBpos = new Vector3(OBposX, OBposY, OBposZ);
                var OBrot = new Quaternion(OBrotX, OBrotY, OBrotZ, OBrotW);
                var OBnetObj = objectDataFactory.createObjectData(
                    OBobjectType,
                    OBnetId,
                    OBplayerId,
                    OBobjectId,
                    OBpos,
                    OBrot);

                objectsToCreate.Add(OBnetObj);

                break;

            case 102:
                var objectToDelete = reader.GetInt();
                objectsToDestroy.Add(objectToDelete);
                break;

            case 103:
                var UobjectId = reader.GetInt();
                if (NetworkObjects.ContainsKey(UobjectId))
                    NetworkObjects[UobjectId].ReadData(reader);
                break;
            case 201:
                var rpcTarget = reader.GetByte();
                var rpcName = reader.GetString();
                var rpcObjectId = reader.GetInt();
                rpcList.Add(new Tuple<string, int, byte[]>(rpcName, rpcObjectId, reader.GetRemainingBytes()));

                break;

            case 301:
                var levelId = reader.GetInt();

                LevelToLoad = levelId;

                break;
        }

        reader.Recycle();
    }

    public void InstantiateNetworkObject(int objectType, int playerId, Vector3 position, Quaternion rotation)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort) 101);
        writer.Put(objectType);
        writer.Put(playerId);
        writer.Put(position.x);
        writer.Put(position.y);
        writer.Put(position.z);
        writer.Put(rotation.x);
        writer.Put(rotation.y);
        writer.Put(rotation.z);
        writer.Put(rotation.w);
        Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void DestroyNetworkObject(int objectId)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort) 102);
        writer.Put(objectId);
        Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void InitPlayer(int netId, int playerId, string pName, bool isHost, int charId, Color playerColor)
    {
        var p = new PlayerData(netId, playerId, isHost, pName, charId, playerColor);
        Players.Add(playerId, p);
    }

    public void Send(NetDataWriter writer, DeliveryMethod method)
    {
        client.Statistics.BytesSent += (ulong) writer.Data.Length;
        client.Statistics.PacketsSent++;

        client?.FirstPeer?.Send(writer, method);
    }

    private void OnApplicationQuit()
    {
        client.Stop();
    }
}