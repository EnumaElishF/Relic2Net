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
    //玩家默认金币数量
    public int playerDefaultCoinCount;
    public float rootMotionMoveSpeedMultiply;
    public float airMoveSpeed;
    public float playerRotateSpeed;
    public float playerJumpHeightMultiply;
    [Header("地形")]
    public Dictionary<string,GameObject> terrainDic;
    [Header("物品配置")]
    public Dictionary<string, ItemConfigBase> itemConfigDic;
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

    //目前三种物品，后续如果有可以继续加
    [FolderPath] public string weaponConfigFolderPath;
    [FolderPath] public string consumablegConfigFolderPath;
    [FolderPath] public string materialConfigFolderPath;
    /// <summary>
    /// 服务端物品配置: 记得把物品几个文件夹的内容拖拽，然后手动set到ServerConfig里
    /// </summary>
    [Button]
    public void SetItemConfigDic()
    {
        if (itemConfigDic == null) itemConfigDic = new Dictionary<string, ItemConfigBase>();
        itemConfigDic.Clear();
        FindItems(weaponConfigFolderPath);
        FindItems(consumablegConfigFolderPath);
        FindItems(materialConfigFolderPath);
    }
    private void FindItems(string path)
    {
        string[] files = Directory.GetFiles(path);//包含*.meta 文件
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].EndsWith(".meta")) //不计入 *.meta文件
            {
                ItemConfigBase itemConfig = AssetDatabase.LoadAssetAtPath<ItemConfigBase>(files[i]);
                itemConfigDic.Add(itemConfig.name, itemConfig);
            }
        }
    }
#endif
}
