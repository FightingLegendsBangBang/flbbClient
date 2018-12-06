using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class TestPlayerController : INetworkObject
{
    private Vector3 lookDirection = Vector3.forward;

    private void Start()
    {
        interpolateRotation = false;
    }

    public override void ObjectUpdate()
    {
        float horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);

        var newLookDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (newLookDirection != Vector3.zero)
            lookDirection = newLookDirection;

        transform.position += new Vector3(horizontal, 0, vertical).normalized * Time.deltaTime * 10;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        if (Input.GetKeyDown(KeyCode.J))
            Shoot();

        if (Input.GetKeyDown(KeyCode.Escape) && nwm.IsHost)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((ushort) 301);
            writer.Put(-2);
            nwm.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    [NetRPC]
    public void RPC_TakeDamage(float r, float g, float b)
    {

        Color newColor = new Color(r, g, b, 1.0f);
        GetComponent<Renderer>().material.color = newColor;
    }

    void Shoot()
    {
        nwm.InstantiateNetworkObject(1, playerId, transform.position, Quaternion.LookRotation(lookDirection));
    }
}