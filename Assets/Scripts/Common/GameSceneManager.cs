using JKFrame;
using UnityEngine;

/// <summary>
/// 作为启动器，还有跨场景切换使用，（不做其他功能）， 应该是场景的初始化流程，第一个启动的脚本
/// </summary>
public class GameSceneManager : MonoBehaviour
{

    void Start()
    {
        if (NetManager.Instance.IsServer)
        {
            //可以直接写死，也可以再后期配置
            ResSystem.InstantiateGameObject("ServerOnGameScene");
        }
        else
        {
            ResSystem.InstantiateGameObject("ClientOnGameScene");

        }
    }
}
