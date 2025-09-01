using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientsManager : SingletonMono<ClientsManager>
{
    //SingletonMono�����Singleton��ͨ�û�����죬

    public GameObject playerPrefab;

    /// <summary>
    /// ��ʼ��   ��Ҫ�ɷ�������������Ϊ�ͻ����ǵĹ����ߣ�����һ��λ�ã�˭ȥ�����ĳ�ʼ������õ������
    /// </summary>
    public void Init()
    {
        NetManager.Instance.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    private void OnClientConnectedCallback(ulong clientID)
    {
        //TODO ��¼ע��֮�������
        //TODO Ԥ���塢����Ⱥ�����������
        NetManager.Instance.SpawnObject(clientID, playerPrefab, Vector3.zero);
    }
}
