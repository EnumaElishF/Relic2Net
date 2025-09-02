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

        ResSystem.InstantiateGameObject("ClientGlobal");
        SceneSystem.LoadScene("GameScene");
        Debug.Log("InitClient 成功");
    }


}
