using JKFrame;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 服务端的全局配置
/// </summary>
[CreateAssetMenu(menuName = "Config/Server/ServerConfig")]
public class ServerConfig : ConfigBase
{
    [Header("通用")]
    public GameObject NetworkManagerPrefab;
    public GameObject ServerOnGameScenePrefab;
    [Header("玩家")]
    public GameObject playerPrefab;
    ///玩家默认坐标
    public Vector3 playerDefaultPosition;
    [Header("地形")]
    public Dictionary<string,GameObject> terrainDic;
#if UNITY_EDITOR
    [FolderPath] public string terrainFolderPath; //文件夹路径
    /// <summary>
    /// 自动工具: 地形文件自动加入到字典
    /// </summary>
    [Button]
    public void SetTerrainDic()
    {
        if (terrainDic == null) terrainDic = new Dictionary<string, GameObject>();
        terrainDic.Clear();
        string[] files = Directory.GetFiles(terrainFolderPath);//包含*.meta 文件
        for(int i = 0; i < files.Length; i++)
        {
            if (!files[i].EndsWith(".meta")) //不计入 *.meta文件
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(files[i]);
                terrainDic.Add(prefab.name, prefab);
            }
        }
    }
#endif
}
