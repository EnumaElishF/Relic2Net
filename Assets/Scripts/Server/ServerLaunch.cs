using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerLaunch : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60; //֡������
        InitServers();
        SceneManager.LoadScene("GameScene");
    }

    private void InitServers()
    {
        ClientsManager.Instance.Init();
        //NetManager.Instance.StartServer(); ��Ϊ�������������߼�Ҫ�ߣ���������û����ԭ���ĺ���
        //�����Խ�һ��������������
        NetManager.Instance.InitServer();

        Debug.Log("InitServers Succeed");
    }
    
}
