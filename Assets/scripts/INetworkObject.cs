using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public abstract class INetworkObject : MonoBehaviour
{
    public enum rpcTarget
    {
        one = 0,
        other = 1,
        all = 2
    }

    public int playerId;
    public int netWorkId;
    public int objectId;
    protected bool owner;
    protected NetworkManager nwm;

    protected bool interpolatePosition = true;
    protected bool interpolateRotation = true;
    protected float interPolationAmountPosition = 50f;
    protected float interPolationAmountRotation = 50f;

    public void Init(int playerId, int objectId, int netWorkId)
    {
        this.playerId = playerId;
        this.netWorkId = netWorkId;
        this.objectId = objectId;
        owner = netWorkId == NetworkManager.Instance.MyNetworkId;
        nwm = NetworkManager.Instance;
    }

    private void Update()
    {
        if (!owner)
        {
            transform.position = interpolatePosition
                ? Vector3.Lerp(transform.position, nwm.NetworkObjects[objectId].position,
                    0.2f)
                : nwm.NetworkObjects[objectId].position;
            transform.rotation = interpolateRotation
                ? Quaternion.Lerp(transform.rotation, nwm.NetworkObjects[objectId].rotation,
                    0.2f)
                : nwm.NetworkObjects[objectId].rotation;
            return;
        }

        ObjectUpdate();

        nwm.NetworkObjects[objectId].position = transform.position;
        nwm.NetworkObjects[objectId].rotation = transform.rotation;
    }

    public void DestroyObject()
    {
        nwm.DestroyNetworkObject(objectId);
        gameObject.SetActive(false);
    }

    public void SendRPC(string rpcName, int sender, rpcTarget target, params string[] args)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort) 201);
        writer.Put((byte) target);
        writer.Put(rpcName);
        writer.Put(objectId);
        writer.Put(sender);
        foreach (var o in args)
        {
            writer.Put(o);
        }

        nwm.Send(writer, DeliveryMethod.ReliableUnordered);
    }

    public abstract void ObjectUpdate();
}