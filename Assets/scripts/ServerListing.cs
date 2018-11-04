using System;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ServerListing : MonoBehaviour
    {
        public TextMeshProUGUI serverNameText;
        public TextMeshProUGUI playerCountText;
        public TextMeshProUGUI ownerNameText;

        public GameObject networkManager;

        private string serverAdres;
        private int serverPort;

        public void Initialize(string serverName, string ownerName, int totalPlayers, int maxPlayers,
            string serverAdres, int serverPort)
        {
            serverNameText.SetText(serverName);
            playerCountText.SetText(totalPlayers + "/" + maxPlayers);
            ownerNameText.SetText("Owner:" + ownerName);

            this.serverAdres = serverAdres;
            this.serverPort = serverPort;
        }

        public void ClickEvent()
        {
            Debug.Log(serverAdres + ":" + serverPort);
            NetWorker socket = null;
            socket = new UDPClient();
            ((UDPClient) socket).Connect(serverAdres, (ushort) serverPort, Constants.MasterServerIP,
                (ushort) Constants.MasterServerNatPort);
            Debug.Log("connecting");

            Connected(socket);
        }

        public void Connected(NetWorker networker)
        {
            if (!networker.IsBound)
            {
                Debug.LogError("NetWorker failed to bind");
                return;
            }


            Instantiate(networkManager).GetComponent<NetworkManager>().Initialize(networker);


            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}