using JKFrame;
using UnityEngine;

public class ClientLaunch : MonoBehaviour
{
    void Start()
    {
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
