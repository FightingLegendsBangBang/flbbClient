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
    public int MyNetworkId;
    private string _myName;
    public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();

    public List<Tuple<int, int, GameObject, Vector3, Quaternion>> playersToCreate =
        new List<Tuple<int, int, GameObject, Vector3, Quaternion>>();

    public List<int> playersToDestroy = new List<int>();

    public GameObject playerPrefab;

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
        if (playersToCreate.Count > 0)
        {
            for (int i = 0; i < playersToCreate.Count; i++)
            {
                var pl = playersToCreate[i];
                var pobj = Instantiate(pl.Item3, pl.Item4, pl.Item5);

                var pc = pobj.GetComponent<TestPlayerController>();
                pc.Init(pl.Item2, pl.Item1);

                Players[pl.Item2].playerController = pc;
            }

            playersToCreate.Clear();
        }

        if (playersToDestroy.Count > 0)
        {
            Debug.Log("test");
            for (int i = 0; i < playersToDestroy.Count; i++)
            {
                List<int> dl = new List<int>();
                foreach (var player in Players)
                {
                    if (player.Value.networkId == playersToDestroy[i])
                    {
                        Destroy(Players[player.Key].playerController.gameObject);
                        dl.Add(player.Key);
                    }
                }

                foreach (int i1 in dl)
                {
                    Players.Remove(i1);
                }
            }

            playersToDestroy.Clear();
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
            foreach (var player in Players)
            {
                if (player.Value.networkId == MyNetworkId)
                {
                    if (player.Value.position != player.Value.oldPosition)
                    {
                        player.Value.WritePlayerData(writer);
                        client?.FirstPeer?.Send(writer, DeliveryMethod.Unreliable);
                        writer.Reset();
                        player.Value.oldPosition = player.Value.position;
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


                InitPlayer(networkId, playerId, pName, isHost, Vector3.zero);


                writer.Put((ushort) 1);
                writer.Put(playerId);
                writer.Put(pName);
                writer.Put(isHost);

                client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);


                break;

            case 2: //register new player;
                var newNetworkId = reader.GetInt();
                var newPlayerId = reader.GetInt();
                var npName = reader.GetString();
                var nIsHost = reader.GetBool();
                var posx = reader.GetFloat();
                var posy = reader.GetFloat();
                var posz = reader.GetFloat();
                var pos = new Vector3(posx, posy, posz);
                InitPlayer(newNetworkId, newPlayerId, npName, nIsHost, pos);

                break;
            case 3: //remove disconnected player
                var rpid = reader.GetInt();
                playersToDestroy.Add(rpid);
                break;
            case 101:
                int pid = reader.GetInt();
                foreach (var player in Players)
                {
                    if (pid == player.Key)
                        player.Value.ReadPlayerData(reader);
                }

                break;
        }

        reader.Recycle();
    }

    private void InitPlayer(int netId, int playerId, string pName, bool isHost, Vector3 position)
    {
        var p = new PlayerData(netId, playerId, isHost, pName, position);
        p.CreatePlayerObject(playerPrefab);
        Players.Add(playerId, p);
    }

    private void OnApplicationQuit()
    {
        client.Stop();
    }
}