using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.SimpleJSON;
using UnityEngine;
using Nakama;
using IClient = Nakama.IClient;

public class testscript : MonoBehaviour
{
    private readonly IClient clientNakama = new Client("defaultkey", "192.168.2.4", 7350, false);
    private ISocket socket;

    private string masterServerHost = "77.171.168.30";
    private int masterServerPort = 15940;
    private string natServerHost = "77.171.168.30";
    private ushort natServerPort = 15941;

    private TCPMasterClient client;

    /*
    private async void Awake()
    {
        var deviceid = SystemInfo.deviceUniqueIdentifier;
        var session = await client.AuthenticateDeviceAsync(deviceid);

        Debug.LogFormat("User id '{0}'", session.UserId);
        Debug.LogFormat("User username '{0}'", session.Username);
        Debug.LogFormat("Session has expired: {0}", session.IsExpired);
        Debug.LogFormat("Session expires at: {0}", session.ExpireTime); // in seconds.

        var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        date = date.AddSeconds(session.ExpireTime).ToLocalTime();
        Debug.LogFormat("Session expires on: '{0}'", date);
        
        socket = client.CreateWebSocket();
        socket.OnConnect += (sender, args) =>
        {
            Debug.Log("Socket connected.");
        };
        socket.OnDisconnect += (sender, args) =>
        {
            Debug.Log("Socket disconnected.");
        };
        await socket.ConnectAsync(session);
        
        var match = await socket.CreateMatchAsync();
        Debug.LogFormat("Created match with ID '{0}'.", match.Id);
        
        var connectedOpponents = new List<IUserPresence>(0);
        socket.OnMatchPresence += (_, presence) =>
        {
            connectedOpponents.AddRange(presence.Joins);
            foreach (var leave in presence.Leaves)
            {
                connectedOpponents.RemoveAll(item => item.SessionId.Equals(leave.SessionId));
            };
        };
    }
    */

    // Start is called before the first frame update
    void Start()
    {
        client = new TCPMasterClient();

        client.serverAccepted += SendMatchRequest;

        client.textMessageReceived += RecieveMatchData;

        client.Connect(masterServerHost, (ushort) masterServerPort);
    }


    private void SendMatchRequest(NetWorker sender)
    {
        try
        {
            Debug.Log("connected");
            // Create the get request with the desired filters
            JSONNode sendData = JSONNode.Parse("{}");
            JSONClass getData = new JSONClass();
            getData.Add("id", "myGame");
            getData.Add("type", "any");
            getData.Add("mode", "all");

            sendData.Add("get", getData);

            // Send the request to the server
            client.Send(BeardedManStudios.Forge.Networking.Frame.Text.CreateFromString(client.Time.Timestep,
                sendData.ToString(), true, Receivers.Server, MessageGroupIds.MASTER_SERVER_GET, true));
        }
        catch
        {
            Debug.Log("test");
            // If anything fails, then this client needs to be disconnected
            client.Disconnect(true);
            client = null;
        }
    }

    private void RecieveMatchData(NetworkingPlayer player, Text frame, NetWorker sender)
    {
        try
        {
            Debug.Log("getting data");
            // Get the list of hosts to iterate through from the frame payload
            JSONNode data = JSONNode.Parse(frame.ToString());
            Debug.Log(data);
            if (data["hosts"] != null)
            {
                MasterServerResponse response = new MasterServerResponse(data["hosts"].AsArray);

                if (response != null && response.serverResponse.Count > 0)
                {
                    // Go through all of the available hosts and add them to the server browser
                    foreach (MasterServerResponse.Server server in response.serverResponse)
                    {
                        string protocol = server.Protocol;
                        string address = server.Address;
                        ushort port = server.Port;
                        string name = server.Name;

                        Debug.Log("Name: " + server.Name);
                        Debug.Log("Address: " + server.Address);
                        Debug.Log("Port: " + server.Port);
                        Debug.Log("Comment: " + server.Comment);
                        Debug.Log("Type: " + server.Type);
                        Debug.Log("Mode: " + server.Mode);
                        Debug.Log("Players: " + server.PlayerCount);
                        Debug.Log("Max Players: " + server.MaxPlayers);
                        Debug.Log("Protocol: " + server.Protocol);
                    }
                }
            }
        }
        finally
        {
            if (client != null)
            {
                // If we succeed or fail the client needs to disconnect from the Master Server
                client.Disconnect(true);
                client = null;
            }
        }
    }


// Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter(Collision other)
    {
    }

    private void FixedUpdate()
    {
    }
}