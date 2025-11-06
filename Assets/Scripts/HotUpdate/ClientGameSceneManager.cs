using JKFrame;
using System.Collections;
using UnityEngine;

public class ClientGameSceneManager : MonoBehaviour
{
    void Start()
    {
        ClientMapManager.Instance.Init();//先初始化地图更合理一些
        MonsterClientManager.Instance.Init();
        PlayerManager.Instance.Init();
        StartCoroutine(LoadGame());
    }
    /// <summary>
    /// 加载游戏，进度条控制
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadGame()
    {
        UI_LoadingWindow loadingWindow = UISystem.Show<UI_LoadingWindow>();
        loadingWindow.Set("Loading");
        //申请进入游戏:发送网络消息
        NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_EnterGame, default(C_S_EnterGame));
        float progress = 0;
        loadingWindow.UpdateProgress(progress, 100);
        yield return CoroutineTool.WaitForFrame(); //等待一帧
        while (!ClientMapManager.Instance.IsLoadingCompleted())
        {
            yield return CoroutineTool.WaitForFrame();
            if (progress < 99)
            {
                progress += 0.1f;
                loadingWindow.UpdateProgress(progress, 100);
            }
        }
        progress = 99;
        loadingWindow.UpdateProgress(progress, 100);
        while (!PlayerManager.Instance.IsLoadingCompleted())
        {
            yield return CoroutineTool.WaitForFrame();
        }
        progress = 100;
        loadingWindow.UpdateProgress(progress, 100);
        UISystem.Close<UI_LoadingWindow>();
        UISystem.Show<UI_ChatWindow>();
    }


}
