using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

//NetManager作为两边通用的，不分客户端还是服务端。算是通用部分。
public class NetManager : NetworkManager
{
    public static NetManager Instance { get; private set; }
    public UnityTransport unityTransport { get; private set; } //用到了NetCode的网络UTP
    public NetMessageManager netMessageManager { get; private set; }
    private Dictionary<GameObject, NetworkPrefabInstanceHandler> prefabHandlerDic;

    /// <summary>
    /// 最先进行的初始化，然后进行InitClient或者InitServer
    /// </summary>
    public void Init(bool isClient)
    {
        Instance = this;
        unityTransport = GetComponent<UnityTransport>();
        netMessageManager = GetComponent<NetMessageManager>();
        if (isClient) InitClient();
        else InitServer();
        netMessageManager.Init();

        prefabHandlerDic = new Dictionary<GameObject, NetworkPrefabInstanceHandler>(NetworkConfig.Prefabs.Prefabs.Count);
        //给每个网络对象预制体都绑定上handler
        foreach(NetworkPrefab item in NetworkConfig.Prefabs.Prefabs)
        {
            NetworkPrefabInstanceHandler handler = new NetworkPrefabInstanceHandler(item.Prefab);
            prefabHandlerDic.Add(item.Prefab, handler);
            PrefabHandler.AddHandler(item.Prefab, handler);
        }
    }

    public void InitClient()
    {
        StartClient();
    }

    public void InitServer()
    {
        StartServer();
    }
    /// <summary>
    /// 生成一个对象，但是不要网络同步和显示
    /// </summary>
    public NetworkObject SpawnObjectNoShow(ulong clientID, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        NetworkObject networkObject = prefabHandlerDic[prefab].Instantiate(clientID, position, rotation);
        networkObject.transform.position = position;
        //networkObject.SpawnWithOwnership(clientID); //注释掉 生成
        //networkObject.NetworkShow(clientID);
        return networkObject;
    }
    /// <summary>
    /// 网络对象通过对象池的生成
    /// </summary>
    public NetworkObject SpawnObject(ulong clientID,GameObject prefab,Vector3 position,Quaternion rotation)
    {
        //TODO 后续增加网络对象 对象池
        NetworkObject networkObject = prefabHandlerDic[prefab].Instantiate(clientID, position, rotation);
        networkObject.transform.position = position;
        networkObject.SpawnWithOwnership(clientID); //生成
        networkObject.NetworkShow(clientID);
        return networkObject;
    }
    /// <summary>
    /// 网络对象通过对象池的销毁
    /// </summary>
    public void DestroyObject(NetworkObject networkObject)
    {
        //Despawn: 在网络层面解除该对象的同步状态，使它不再被网络系统追踪和同步，但它既不会直接销毁游戏对象，也不会自动将其放入对象池。需要其他配置。
        networkObject.Despawn();
    }

}
