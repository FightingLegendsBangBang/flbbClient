using System.Net;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.SimpleJSON;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Analytics;


public class ServerBrowser : MonoBehaviour
{
    private string masterServerHost = "77.171.168.30";
    private int masterServerPort = 15940;
    private string natServerHost = "77.171.168.30";
    private ushort natServerPort = 15941;

    private TCPClient client;

    public Transform serverListObject;
    public GameObject serverListingObject;


    void Start()
    {
        Rpc.MainThreadRunner = MainThreadManager.Instance;
        NetWorker.localServerLocated += LocalServerLocated;
        RefreshClick();
    }


    private void SendMatchRequest(NetWorker sender)
    {
        try
        {
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
            // If anything fails, then this client needs to be disconnected
            client.Disconnect(true);
            client = null;
        }
    }

    private void RecieveMatchData(NetworkingPlayer player, Text frame, NetWorker sender)
    {
        try
        {
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
                        CreateServerListing(server.Name, server.Comment, server.PlayerCount,
                            server.MaxPlayers, server.Address, server.Port);
                    }
                }
            }
        }
        finally
        {
            if (client != null)
            {
                Debug.Log(serverListingObject);

                Debug.Log("closed");

                // If we succeed or fail the client needs to disconnect from the Master Server
                client.Disconnect(true);
                client = null;
            }
        }
    }

    public void CreateServerListing(string serverName, string ownerName, int totalPlayers, int maxPlayers,
        string serverAdres, int serverPort)
    {
        MainThreadManager.Run(() =>
        {
            GameObject sl = Instantiate(serverListingObject, serverListObject, false);
            sl.GetComponent<ServerListing>()
                .Initialize(serverName, ownerName, totalPlayers, maxPlayers, serverAdres, serverPort);
        });
    }

    public void LocalServerLocated(NetWorker.BroadcastEndpoints endpoint, NetWorker sender)
    {
        Debug.Log("Found endpoint: " + endpoint.Address + ":" + endpoint.Port + "/");

        CreateServerListing(endpoint.Address, "local", 1, 2, endpoint.Address, endpoint.Port);
    }

    public void RefreshClick()
    {
        for (int i = serverListObject.childCount - 1; i >= 0; --i)
            Destroy(serverListObject.GetChild(i).gameObject);
        client = new TCPMasterClient();
        client.serverAccepted += SendMatchRequest;
        client.textMessageReceived += RecieveMatchData;
        client.Connect(masterServerHost, (ushort) masterServerPort);
        NetWorker.RefreshLocalUdpListings();
    }

    private void OnApplicationQuit()
    {
        if (client != null)
        {
            Debug.Log("disconnected");
            client.Disconnect(true);
        }
    }
}