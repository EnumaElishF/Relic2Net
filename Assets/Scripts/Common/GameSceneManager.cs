using JKFrame;
using UnityEngine;

/// <summary>
/// ��Ϊ�����������п糡���л�ʹ�ã��������������ܣ��� Ӧ���ǳ����ĳ�ʼ�����̣���һ�������Ľű�
/// </summary>
public class GameSceneManager : MonoBehaviour
{

    void Start()
    {
        if (NetManager.Instance.IsServer)
        {
            //����ֱ��д����Ҳ�����ٺ�������
            ResSystem.InstantiateGameObject("ServerOnGameScene");
        }
        else
        {
            ResSystem.InstantiateGameObject("ClientOnGameScene");

        }
    }
}
