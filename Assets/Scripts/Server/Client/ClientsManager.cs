using Codice.Client.Commands;
using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientsManager : SingletonMono<ClientsManager> //SingletonMono加入对Singleton的通用基类改造，
{
    private Dictionary<ClientState, HashSet<Client>> clientDic;
    // Key : ClientID
    public Dictionary<ulong, Client> clientIDDic;

    /// <summary>
    /// 初始化   需要由服务器开启，作为客户端们的管理者（考虑一个位置，谁去做他的初始化是最好的情况？
    /// </summary>
    public void Init()
    {
        clientDic = new Dictionary<ClientState, HashSet<Client>>()
        {
            {ClientState.Connected,new HashSet<Client>(100) },
            {ClientState.Logined,new HashSet<Client>(100) },
            {ClientState.Gaming,new HashSet<Client>(100) },
        };
        clientIDDic = new Dictionary<ulong, Client>(100);

        NetManager.Instance.OnClientConnectedCallback += OnClientConnected;
        NetManager.Instance.OnClientDisconnectCallback += OnClientDisconnect;

        //NetMessageManager注册网络事件
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Register, OnClientRegister);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Login, OnClientLogin);
        
    }




    /// <summary>
    /// 连接成功
    /// </summary>
    private void OnClientConnected(ulong clientID)
    {
        //NetManager.Instance.SpawnObject(clientID, ServerResSystem.serverConfig.playerPrefab, ServerResSystem.serverConfig.playerDefaultPosition);

        //用对象池去处理，构建一个Client
        Client client = ResSystem.GetOrNew<Client>();
        client.clientID = clientID;
        clientDic[ClientState.Connected].Add(client);
    }
    /// <summary>
    /// 客户端退出，断开连接
    /// </summary>
    private void OnClientDisconnect(ulong clientID)
    {
        if(clientIDDic.Remove(clientID, out Client client))
        {
            clientDic[client.clientState].Remove(client);
            client.OnDestroy();
        }
    }
    /// <summary>
    /// 申请注册
    /// </summary>
    private void OnClientRegister(ulong arg1, INetworkSerializable serializable)
    {
        
    }
    /// <summary>
    /// 申请登录
    /// </summary>
    private void OnClientLogin(ulong arg1, INetworkSerializable serializable)
    {
        
    }


}
