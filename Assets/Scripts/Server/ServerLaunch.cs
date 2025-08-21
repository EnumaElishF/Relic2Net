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
        ClientsManager.Instance.Init();
        //NetManager.Instance.StartServer(); 因为可能有其他的逻辑要走，所以我们没有用原生的函数
        //我们自建一个服务网络启动
        NetManager.Instance.InitServer();

        Debug.Log("InitServers Succeed");
    }
    
}
