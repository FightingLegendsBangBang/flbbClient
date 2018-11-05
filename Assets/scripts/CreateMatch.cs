using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CreateMatch : MonoBehaviour
{
    public GameObject networkManager;

    public void ClickCreateMatch()
    {
        Debug.Log("creating match");

        NetWorker server;
        server = new UDPServer(64);

        ((UDPServer) server).Connect(port: 15937, natHost: Constants.MasterServerIP,
            natPort: (ushort) Constants.MasterServerNatPort);


        server.playerTimeout += (player, sender) => { Debug.Log("Player " + player.NetworkId + " timed out"); };

        Connected(server);
    }

    private void Connected(NetWorker networker)
    {
        if (!networker.IsBound)
        {
            Debug.LogError("NetWorker failed to bind");
            return;
        }

        var mgr = Instantiate(networkManager).GetComponent<NetworkManager>();


        JSONNode masterServerData = null;

        string serverId = "myGame";
        string serverName = "Forge Game " + Random.Range(1000, 9999);
        string type = "Deathmatch";
        string mode = "Teams";
        string comment = "kiprok";

        masterServerData = mgr.MasterServerRegisterData(networker, serverId, serverName, type, mode, comment,
            false, 0);

        mgr.Initialize(networker, Constants.MasterServerIP, (ushort) Constants.MasterServerPort, masterServerData);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}