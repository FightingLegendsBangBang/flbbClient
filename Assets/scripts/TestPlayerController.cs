using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

public class TestPlayerController : INetworkObject
{
    private Vector3 lookDirection = Vector3.zero;

    void Update()
    {
        if (!owner)
        {
            transform.position = Vector3.Lerp(transform.position, nwm.NetworkObjects[objectId].position,
                Time.deltaTime * 50);
            transform.rotation = nwm.NetworkObjects[objectId].rotation;
            return;
        }


        float horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);

        var newLookDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (newLookDirection != Vector3.zero)
            lookDirection = newLookDirection;

        transform.position += new Vector3(horizontal * Time.deltaTime * 10, 0, vertical * Time.deltaTime * 10);
        transform.rotation = Quaternion.LookRotation(lookDirection);

        if (Input.GetKeyDown(KeyCode.J))
            Shoot();

        nwm.NetworkObjects[objectId].position = transform.position;
        nwm.NetworkObjects[objectId].rotation = transform.rotation;
    }

    void Shoot()
    {
        nwm.InstantiateNetworkObject(1, playerId, transform.position, Quaternion.LookRotation(lookDirection));
    }
}