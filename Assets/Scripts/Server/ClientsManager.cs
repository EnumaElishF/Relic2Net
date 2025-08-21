# if UNITY_SERVER || SERVER_EDITOR_TEST
using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientsManager : SingletonMono<ClientsManager>
{
    //SingletonMono�����Singleton��ͨ�û�����죬

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
        //TODO Ԥ���塢����Ⱥ�����������
        NetManager.Instance.SpawnObject(clientID, playerPrefab, Vector3.zero);
    }
}
#endif