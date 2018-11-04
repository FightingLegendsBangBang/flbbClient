using BeardedManStudios.SimpleJSON;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeardedManStudios.Forge.Networking.Unity
{
	public class ServerBrowser : MonoBehaviour
	{
		private string masterServerHost = "77.171.168.30";
		private int masterServerPort = 15940;
		private string natServerHost = "77.171.168.30";
		private ushort natServerPort = 15941;

		public string gameId = "myGame";
		public string gameType = "any";
		public string gameMode = "all";

		public Transform content = null;
		public GameObject serverOption = null;
		public GameObject networkManager = null;
		TCPClient client = null;

		private void Awake()
		{
			MainThreadManager.Create();
		}

		private void Start()
		{
			Refresh();
		}

		public void CreateServerOption(string name, UnityEngine.Events.UnityAction callback)
		{
			MainThreadManager.Run(() =>
			{
				var option = Instantiate(serverOption);
				option.transform.SetParent(content);
				var browserItem = option.GetComponent<ServerBrowserItem>();
				if (browserItem != null)
					browserItem.SetData(name, callback);
			});
		}

		public void Refresh()
		{
			Debug.Log("test222");
			// Clear out all the currently listed servers
			for (int i = content.childCount - 1; i >= 0; --i)
				Destroy(content.GetChild(i).gameObject);

			// The Master Server communicates over TCP
			client = new TCPMasterClient();

			// Once this client has been accepted by the master server it should sent it's get request
			client.serverAccepted += (sender) =>
			{
				try
				{
					Debug.Log("connected");
					// Create the get request with the desired filters
					JSONNode sendData = JSONNode.Parse("{}");
					JSONClass getData = new JSONClass();
					getData.Add("id", gameId);
					getData.Add("type", gameType);
					getData.Add("mode", gameMode);

					sendData.Add("get", getData);

					// Send the request to the server
					client.Send(Frame.Text.CreateFromString(client.Time.Timestep, sendData.ToString(), true, Receivers.Server, MessageGroupIds.MASTER_SERVER_GET, true));
				}
				catch
				{
					Debug.Log("test");
					// If anything fails, then this client needs to be disconnected
					client.Disconnect(true);
					client = null;
				}
			};

			// An event that is raised when the server responds with hosts
			client.textMessageReceived += (player, frame, sender) =>
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

								// name, address, port, comment, type, mode, players, maxPlayers, protocol
								CreateServerOption(name, () =>
								{
									// Determine which protocol should be used when this client connects
									NetWorker socket = null;

									if (protocol == "udp")
									{
										socket = new UDPClient();
										((UDPClient)socket).Connect(address, port, natServerHost, natServerPort);
										Debug.Log("connecting");
									}
									else if (protocol == "tcp")
									{
										socket = new TCPClient();
										((TCPClient)socket).Connect(address, port);
									}
									#if !UNITY_IOS && !UNITY_ANDROID
									else if (protocol == "web")
									{
										socket = new TCPClientWebsockets();
										((TCPClientWebsockets)socket).Connect(address, port);
									}
									#endif
									if (socket == null)
										throw new Exception("No socket of type " + protocol + " could be established");

									Connected(socket);
								});
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
			};

			client.Connect(masterServerHost, (ushort)masterServerPort);
		}

		public void Connected(NetWorker networker)
		{
			Debug.Log("connecteddddd");
			if (!networker.IsBound)
			{
				Debug.LogError("NetWorker failed to bind");
				return;
			}

			if (networkManager == null)
			{
				Debug.LogWarning("A network manager was not provided, generating a new one instead");
				GameObject obj = new GameObject("Network Manager");
				obj.AddComponent<NetworkManager>().Initialize(networker);
			}
			else
				Instantiate(networkManager).GetComponent<NetworkManager>().Initialize(networker);
			
			Debug.Log(networker.Players.Count);
			Debug.Log(networker.Players[0].Ip);
			
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
	}
}
