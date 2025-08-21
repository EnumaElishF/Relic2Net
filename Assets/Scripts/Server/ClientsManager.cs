# if UNITY_SERVER || SERVER_EDITOR_TEST
using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientsManager : SingletonMono<ClientsManager>
{
    //SingletonMono加入对Singleton的通用基类改造，

    public GameObject playerPrefab;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    public void Init()
    {
        NetManager.Instance.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    private void OnClientConnectedCallback(ulong clientID)
    {
        //TODO 预制体、坐标等后续基于配置
        NetManager.Instance.SpawnObject(clientID, playerPrefab, Vector3.zero);
    }
}
#endif