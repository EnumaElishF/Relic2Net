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

        InitWindowData();
        //加载本地化配置->到框架的 LocalizationSystem设置上
        LocalizationSystem.GlobalConfig = ResSystem.LoadAsset<LocalizationConfig>("GlobalLocalizationConfig");

        ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(true);//直接用的同步，因为文件很小，如果大了还是要用异步加载。
        EventSystem.AddTypeEventListener<GameSceneLaunchEvent>(OnGameSceneLaunchEvent);
        //SceneSystem.LoadScene("GameScene");
        UISystem.Show<UI_MainMenuWindow>();
        Debug.Log("InitClient 成功");

    }
    public void InitWindowData()
    {
        //做一个对框架上修改的窗口，不依赖于静态的配置
        UISystem.AddUIWindowData<UI_MainMenuWindow>(new UIWindowData(false, nameof(UI_MainMenuWindow), 0));
    }
    private void OnGameSceneLaunchEvent(GameSceneLaunchEvent @event)
    {
        ResSystem.InstantiateGameObject("ClientOnGameScene");
    }

}
