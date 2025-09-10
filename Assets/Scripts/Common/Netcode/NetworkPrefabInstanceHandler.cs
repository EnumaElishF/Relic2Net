using JKFrame;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 所有网络对象类型都通过对象池生成与销毁
/// </summary>
public class NetworkPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private GameObject prefab;

    public NetworkPrefabInstanceHandler(GameObject prefab)
    {
        this.prefab = prefab;
    }

    public void Destroy(NetworkObject networkObject)
    {
        networkObject.GameObjectPushPool();
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        NetworkObject networkObject = PoolSystem.GetGameObject<NetworkObject>(prefab.name);
        if(networkObject == null)
        {
            networkObject = GameObject.Instantiate(prefab).GetComponent<NetworkObject>();
            networkObject.name = prefab.name;
        }
        networkObject.transform.position = position;
        networkObject.transform.rotation = rotation;
        return networkObject;
    }
}
