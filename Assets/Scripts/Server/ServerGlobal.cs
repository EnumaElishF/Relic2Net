using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
/// <summary>
/// ServerGlobal��Ϊȫ�������ģ����ǲ��ᱻ���ٵ�
/// </summary>
public class ServerGlobal : SingletonMono<ServerGlobal>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
