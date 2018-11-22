using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : INetworkObject
{
    private float startTime;
    private float lifeTime = 0.3f;

    private void Start()
    {
        interPolationAmountPosition = 100;

        if (!owner)
            return;

        startTime = Time.time;
    }


    public override void ObjectUpdate()
    {
        if (startTime + lifeTime < Time.time)
            DestroyObject();

        transform.position += transform.forward * Time.deltaTime * 25;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!owner)
            return;

        if (other.transform.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<INetworkObject>();

            if (player.playerId != playerId)
            {
                player.SendRPC("RPC_TakeDamage", playerId, rpcTarget.all, Random.value.ToString(),
                    Random.value.ToString(),
                    Random.value.ToString());
                DestroyObject();
            }
        }
    }
}