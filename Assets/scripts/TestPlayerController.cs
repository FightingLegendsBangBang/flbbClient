using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    public int playerID;
    public int netWorkID;
    private bool owner;
    private NetworkManager nwm;

    public void Init(int playerID,int netWorkID)
    {
        this.playerID = playerID;
        this.netWorkID = netWorkID;
        owner = netWorkID == NetworkManager.Instance.MyNetworkId;
        nwm = NetworkManager.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!owner)
        {
            transform.position = Vector3.Lerp(transform.position, nwm.Players[playerID].position, Time.deltaTime * 50);
            return;
        }


        float horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);

        transform.position += new Vector3(horizontal * Time.deltaTime * 10, 0, vertical * Time.deltaTime * 10);

        nwm.Players[playerID].position = transform.position;
    }
}