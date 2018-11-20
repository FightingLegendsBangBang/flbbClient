using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiteNetLib;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using Random = UnityEngine.Random;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    public Thread networkThread;
    public Thread mainThread;
    public EventBasedNetListener networkListener;
    public NetManager client;
    public int MyNetworkId;
    private string _myName;
    public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();
    public Dictionary<int, NetworkObjectData> NetworkObjects = new Dictionary<int, NetworkObjectData>();
    public GameObject[] networkPrefabs;

    /*public List<Tuple<int, int, GameObject, Vector3, Quaternion>> playersToCreate =
        new List<Tuple<int, int, GameObject, Vector3, Quaternion>>();

    public List<int> playersToDestroy = new List<int>();*/

    private List<NetworkObjectData> objectsToCreate = new List<NetworkObjectData>();
    private List<int> objectsToDestroy = new List<int>();


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _myName = "player " + Random.Range(1000, 9999);

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
    }

    void UpdateNetwork()
    {
        networkListener = new EventBasedNetListener();

        networkListener.NetworkReceiveEvent += ReceivePackage;

        client = new NetManager(networkListener);
        client.Start();
        client.Connect("127.0.0.1", 9050, "SomeConnectionKey");
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
                        client?.FirstPeer?.Send(writer, DeliveryMethod.Unreliable);
                    }
                }
            }

            client.PollEvents();
            Thread.Sleep(16);
        }

        client.Stop();
    }

    void ReceivePackage(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        NetDataWriter writer = new NetDataWriter();

        ushort msgid = reader.GetUShort();
        Debug.Log(msgid);
        switch (msgid)
        {
            case 1: //register to server
                var networkId = reader.GetInt();
                var playerId = reader.GetInt();
                var isHost = reader.GetBool();
                var pName = _myName;
                MyNetworkId = networkId;


                InitPlayer(networkId, playerId, pName, isHost);


                writer.Put((ushort) 1);
                writer.Put(playerId);
                writer.Put(pName);
                writer.Put(isHost);

                client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);

                writer.Reset();
                InstantiateNetworkObject(0, playerId, Vector3.zero, Quaternion.identity);
                break;

            case 2: //register new player;
                var newNetworkId = reader.GetInt();
                var newPlayerId = reader.GetInt();
                var npName = reader.GetString();
                var nIsHost = reader.GetBool();
                InitPlayer(newNetworkId, newPlayerId, npName, nIsHost);
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
                var OBnetObj = new NetworkObjectData(
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
        client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void InitPlayer(int netId, int playerId, string pName, bool isHost)
    {
        var p = new PlayerData(netId, playerId, isHost, pName);
        Players.Add(playerId, p);
    }

    private void OnApplicationQuit()
    {
        client.Stop();
    }
}