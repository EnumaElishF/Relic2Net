using JKFrame;
using UnityEngine;

public class ClientGlobal : SingletonMono<ClientGlobal>
{
    public GameSetting gameSetting { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        //Global有个特点，就是不能销毁
        DontDestroyOnLoad(gameObject);
        LoadGameSetting();

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
    /// <summary>
    /// 注册Windows
    /// </summary>
    public void InitWindowData()
    {
        //做一个对框架上修改的窗口，不依赖于静态的配置
        UISystem.AddUIWindowData<UI_MainMenuWindow>(new UIWindowData(false, nameof(UI_MainMenuWindow), 0));
        //弹窗的层级给高一些，给个4吧。
        //弹窗需要缓存，所以设置true,因为这个是高频使用的弹窗
        UISystem.AddUIWindowData<UI_MessagePopupWindow>(new UIWindowData(true, nameof(UI_MessagePopupWindow), 4));
        UISystem.AddUIWindowData<UI_RegisterWindow>(new UIWindowData(false, nameof(UI_RegisterWindow), 1));
        UISystem.AddUIWindowData<UI_LoginWindow>(new UIWindowData(false, nameof(UI_LoginWindow), 1));
    }
    private void OnGameSceneLaunchEvent(GameSceneLaunchEvent @event)
    {
        ResSystem.InstantiateGameObject("ClientOnGameScene");
    }

    private void LoadGameSetting()
    {
        gameSetting = SaveSystem.LoadSetting<GameSetting>();
        if (gameSetting == null)
        {
            gameSetting = new GameSetting();
            SaveSystem.SaveSetting(gameSetting);
        }
    }
    public void SaveGameSetting()
    {
        //低频的，写到磁盘里去
        SaveSystem.SaveSetting(gameSetting);
    }
    public void RememberAccount(string name,string password)
    {
        gameSetting.rememberPlayerName = name;
        gameSetting.rememberPassword = password;
        SaveGameSetting();
    }
}
