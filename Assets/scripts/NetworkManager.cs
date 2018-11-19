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
    public int myId;
    private string _myName;
    public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();

    public List<Tuple<int, GameObject, Vector3, Quaternion>> playersToCreate =
        new List<Tuple<int, GameObject, Vector3, Quaternion>>();

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
                var pobj = Instantiate(pl.Item2, pl.Item3, pl.Item4);

                var pc = pobj.GetComponent<TestPlayerController>();
                pc.Init(pl.Item1);

                Players[pl.Item1].playerController = pc;
            }

            playersToCreate.Clear();
        }

        if (playersToDestroy.Count > 0)
        {
            Debug.Log("test");
            for (int i = 0; i < playersToDestroy.Count; i++)
            {
                Destroy(Players[playersToDestroy[i]].playerController.gameObject);
                Players.Remove(playersToDestroy[i]);
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
            if (Players.ContainsKey(myId) && client != null)
            {
                if (Players[myId] != null)
                {
                    if (Players[myId].position != Players[myId].oldPosition)
                    {
                        Players[myId].WritePlayerData(writer);
                        client?.FirstPeer?.Send(writer, DeliveryMethod.Unreliable);
                        writer.Reset();
                        Players[myId].oldPosition = Players[myId].position;
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
                var mid = reader.GetInt();
                var isHost = reader.GetBool();
                var pName = _myName;
                myId = mid;


                InitPlayer(mid, pName, isHost, Vector3.zero);


                writer.Put((ushort) 1);
                writer.Put(pName);
                writer.Put(isHost);

                client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);


                break;

            case 2: //register new player;
                var npid = reader.GetInt();
                var npName = reader.GetString();
                var nIsHost = reader.GetBool();
                var posx = reader.GetFloat();
                var posy = reader.GetFloat();
                var posz = reader.GetFloat();
                var pos = new Vector3(posx, posy, posz);
                InitPlayer(npid, npName, nIsHost, pos);

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

    private void InitPlayer(int pid, string pName, bool isHost, Vector3 position)
    {
        var p = new PlayerData(pid, isHost, pName, position);
        p.CreatePlayerObject(playerPrefab);
        Players.Add(pid, p);
    }

    private void OnApplicationQuit()
    {
        client.Stop();
    }
}