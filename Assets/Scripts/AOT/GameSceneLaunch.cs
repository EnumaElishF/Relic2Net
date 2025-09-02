using JKFrame;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 这里承担掉客户端和服务端的一部分逻辑
/// 注: GameSceneLaunch作为AOT程序集的部分，是不做热更新的------<把之前的热更新逻辑，改到了现在AOT逻辑
/// </summary>
public class GameSceneLaunch : MonoBehaviour
{
    IEnumerator Start()
    {
        while(NetworkManager.Singleton == null)
        {
            yield return CoroutineTool.WaitForFrames(); //这个就是为null的功能，只不过框架上做了点gc优化
        }
        if (NetworkManager.Singleton.IsServer)
        {
            //可以直接写死，也可以再后期配置
            ResSystem.InstantiateGameObject("ServerOnGameScene");
        }
        else
        {
            ResSystem.InstantiateGameObject("ClientOnGameScene");

        }
        //因为是一次性的，只要上面的网络准备好了(就是NetworkManager单例不是空的了，做的兜底机制)，那么就把自己挂掉
        Destroy(gameObject);
    }
}
