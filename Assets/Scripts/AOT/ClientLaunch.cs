using JKFrame;
using UnityEngine;

public class ClientLaunch : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60; //客户端帧数设置

        GetComponent<HotUpdateSystem>().StartHotUpdate(null, (bool succeed) =>
        {
            if (succeed)
            {
                OnHotUpdateSucceed();
            }
        });
    }
    private void OnHotUpdateSucceed()
    {
        Debug.Log("ClientGlobal 加载开始");

        ResSystem.InstantiateGameObject("ClientGlobal");
        Debug.Log("ClientGlobal 加载结束");


        SceneSystem.LoadScene("GameScene");

        Debug.Log("InitClient 成功");
    }


}
