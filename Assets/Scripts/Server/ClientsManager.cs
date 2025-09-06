using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientsManager : SingletonMono<ClientsManager>
{
    //SingletonMono加入对Singleton的通用基类改造，


    /// <summary>
    /// 初始化   需要由服务器开启，作为客户端们的管理者（考虑一个位置，谁去做他的初始化是最好的情况？
    /// </summary>
    public void Init()
    {
        NetManager.Instance.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    private void OnClientConnectedCallback(ulong clientID)
    {
        //TODO 登录注册之类的流程
        NetManager.Instance.SpawnObject(clientID, ServerResSystem.serverConfig.playerPrefab, ServerResSystem.serverConfig.playerDefaultPosition);
    }
}
