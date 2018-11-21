using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diagnostics : MonoBehaviour
{
    private NetworkManager nwm;
    private float deltaTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        nwm = NetworkManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }


    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.white;


        if (nwm.client != null)
        {
            int players = nwm.Players.Count;
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            float ping = nwm.client.FirstPeer.Ping;


            string playersText = string.Format("{0} players\n", players);
            string fpsText = string.Format("{0:0.0} ms ({1:0.} fps)\n", msec, fps);
            string pingText = string.Format("ping: {0} ms\n", ping);
            string bytesSendText = string.Format("bytes send: {0}\n", nwm.client.Statistics.BytesSent);
            string bytesReceivedText = string.Format("bytes received: {0}\n", nwm.client.Statistics.BytesReceived);
            string packagesSendText = string.Format("packages send: {0}\n", nwm.client.Statistics.PacketsSent);
            string packagesReceivedText = string.Format("packages received: {0}\n", nwm.client.Statistics.PacketsReceived);
            string text = playersText + fpsText + pingText + bytesSendText + bytesReceivedText + packagesSendText +
                          packagesReceivedText;

            GUI.Label(rect, text, style);
        }
    }
}