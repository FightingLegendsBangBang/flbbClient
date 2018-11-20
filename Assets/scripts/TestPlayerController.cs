using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

public class TestPlayerController : INetworkObject
{
    // Update is called once per frame
    void Update()
    {
        if (!owner)
        {
            transform.position = Vector3.Lerp(transform.position, nwm.NetworkObjects[objectId].position,
                Time.deltaTime * 50);
            transform.rotation = Quaternion.Lerp(transform.rotation, nwm.NetworkObjects[objectId].rotation,
                Time.deltaTime * 50);
            return;
        }


        float horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);

        transform.position += new Vector3(horizontal * Time.deltaTime * 10, 0, vertical * Time.deltaTime * 10);

        
        nwm.NetworkObjects[objectId].position = transform.position;
    }
}