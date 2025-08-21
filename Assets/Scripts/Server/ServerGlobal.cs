using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
/// <summary>
/// ServerGlobal作为全局所做的，他是不会被销毁的
/// </summary>
public class ServerGlobal : SingletonMono<ServerGlobal>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
