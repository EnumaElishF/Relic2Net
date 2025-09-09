using UnityEngine;

/// <summary>
/// GameSceneManager 相当于是 启动器的功能
/// SeverGameSceneManager 是专门启动服务端的
/// </summary>
public class ServerGameSceneManager : MonoBehaviour
{
    //他作为一定是服务端运行的脚本
    void Start()
    {
        AOIManager.Instance.Init();
        ClientsManager.Instance.Init();
        //暂时是立马生成，以后可能加逻辑
        ServerMapManager.Instance.Init();
    }
}
