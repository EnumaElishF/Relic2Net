using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

//NetManager作为两边通用的，不分客户端还是服务端。算是通用部分。
public class NetManager : NetworkManager
{
    //
    public static NetManager Instance { get; private set; }

    public UnityTransport unityTransport { get; private set; }
    private void Awake()
    {
        Instance = this;
        unityTransport = GetComponent<UnityTransport>();

    }
    public void InitClient()
    {
        StartClient();
    }

    public void InitServer()
    {
        StartServer();
    }

    public NetworkObject SpawnObject(ulong clientID,GameObject prefab,Vector3 position)
    {
        //TODO 后续增加网络对象 对象池
        NetworkObject networkObject = Instantiate(prefab).GetComponent<NetworkObject>();
        networkObject.transform.position = position;
        networkObject.SpawnWithOwnership(clientID); //生成
        // networkObject.NetworkShow(clientID);

        return networkObject;
    }

}
