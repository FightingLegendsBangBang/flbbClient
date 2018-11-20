using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : INetworkObject
{
    private float startTime;
    private float lifeTime = 0.3f;

    private void Start()
    {
        if (!owner)
            return;

        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!owner)
        {
            transform.position = Vector3.Lerp(transform.position, nwm.NetworkObjects[objectId].position,
                Time.deltaTime * 50);
            transform.rotation = nwm.NetworkObjects[objectId].rotation;
            return;
        }

        if (startTime + lifeTime < Time.time)
            DestroyObject();

        transform.position += transform.forward * Time.deltaTime * 25;

        nwm.NetworkObjects[objectId].position = transform.position;
        nwm.NetworkObjects[objectId].rotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!owner)
            return;

        if (other.transform.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<INetworkObject>();

            if (player.playerId != playerId)
                DestroyObject();
        }
    }
}