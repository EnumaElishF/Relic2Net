using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
public class ServerGlobal : SingletonMono<ServerGlobal>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
