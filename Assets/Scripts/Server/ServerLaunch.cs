using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerLaunch : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60; //帧数设置
        InitServers();
        SceneManager.LoadScene("GameScene");
    }

    private void InitServers()
    {
        //NetManager继承的基类NetworkManager做了不会销毁的设置代码
        ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(false);//直接用的同步，因为文件很小，如果大了还是要用异步加载
        //  因为可能有其他的逻辑要走，所以我们没有用原生的函数NetworkManager而是自制->NetManager
        //--->我们自建一个服务网络启动
        //NetManager.Instance.InitServer();

        Debug.Log("InitServers Succeed");
    }
    
}
