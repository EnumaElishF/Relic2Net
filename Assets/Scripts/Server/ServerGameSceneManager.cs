using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameSceneManager �൱���� �������Ĺ���
/// SeverGameSceneManager ��ר����������˵�
/// </summary>
public class ServerGameSceneManager : MonoBehaviour
{
    //����Ϊһ���Ƿ�������еĽű�
    void Start()
    {
        ClientsManager.Instance.Init();
    }

    void Update()
    {
        
    }
}
