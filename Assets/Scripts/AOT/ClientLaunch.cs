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

        ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(true);//直接用的同步，因为文件很小，如果大了还是要用异步加载
        ResSystem.InstantiateGameObject<ClientGlobal>("ClientGlobal");
        SceneSystem.LoadScene("GameScene");
        Debug.Log("InitClient 成功");
    }


}
