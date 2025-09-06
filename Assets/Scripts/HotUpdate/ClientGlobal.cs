using JKFrame;
using UnityEngine;

public class ClientGlobal : SingletonMono<ClientGlobal>
{
    protected override void Awake()
    {
        base.Awake();
        //Global有个特点，就是不能销毁
        DontDestroyOnLoad(gameObject);

        //实例化NetworkManager之前，完成 网络变量序列化
        NetworkVariableSerializationBinder.Init();
        ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(true);//直接用的同步，因为文件很小，如果大了还是要用异步加载。
        EventSystem.AddTypeEventListener<GameSceneLaunchEvent>(OnGameSceneLaunchEvent);
        SceneSystem.LoadScene("GameScene");
        Debug.Log("InitClient 成功");

    }
    private void OnGameSceneLaunchEvent(GameSceneLaunchEvent @event)
    {
        ResSystem.InstantiateGameObject("ClientOnGameScene");
    }

}
