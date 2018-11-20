using UnityEngine;

public abstract class INetworkObject : MonoBehaviour
{
    public int playerId;
    public int netWorkId;
    public int objectId;
    protected bool owner;
    protected NetworkManager nwm;

    public void Init(int playerId, int objectId, int netWorkId)
    {
        this.playerId = playerId;
        this.netWorkId = netWorkId;
        this.objectId = objectId;
        owner = netWorkId == NetworkManager.Instance.MyNetworkId;
        nwm = NetworkManager.Instance;
    }
}