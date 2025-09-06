using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 服务端配置管理器，一切资源由配置管理器通过一个SO文件引用关联到
/// </summary>
public static class ServerResSystem 
{
    public static ServerConfig serverConfig;
    static ServerResSystem()
    {
        serverConfig = ServerGlobal.Instance.ServerConfig;
    }
    public static NetManager InstantiateNetworkManager()
    {
        GameObject prefab = serverConfig.NetworkManagerPrefab;
        GameObject instance = GameObject.Instantiate(prefab);
        return instance.GetComponent<NetManager>();
    }    
    public static GameObject InstantiateServerOnGameScene()
    {
        GameObject prefab = serverConfig.ServerOnGameScenePrefab;
        GameObject instance = GameObject.Instantiate(prefab);
        return instance;
    }

    public static GameObject InstantiateTerrain(string resKey,Transform parent,Vector3 position)
    {
        GameObject prefab = serverConfig.terrainDic[resKey];
        GameObject instance = GameObject.Instantiate(prefab,position,Quaternion.identity,parent);

        //服务端的terrain如果要一口气把1600个terrain渲染下来，电脑负担很重，所以不建议开启这个，而且服务端也没有什么必要
        instance.GetComponent<Terrain>().enabled = false;
        return instance;
    }
}
