using UnityEngine;

namespace DefaultNamespace
{
    public class ObjectDataFactory
    {
        public NetworkObjectData createObjectData(int objectType, int netId, int playerId, int objectId,
            Vector3 position, Quaternion rotation)
        {
            switch (objectType)
            {
                case 0:
                    return new PlayerObjectData(objectType, netId, playerId, objectId, position, rotation);

                default:
                    return new NetworkObjectData(objectType, netId, playerId, objectId, position, rotation);
            }
        }
    }
}