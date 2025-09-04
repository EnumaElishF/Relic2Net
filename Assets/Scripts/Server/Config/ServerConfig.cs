using JKFrame;
using UnityEngine;
/// <summary>
/// 服务端的全局配置
/// </summary>
[CreateAssetMenu(menuName = "Config/Server/ServerConfig")]
public class ServerConfig : ConfigBase
{
    ///玩家默认坐标
    public Vector3 playerDefaultPosition;
}
