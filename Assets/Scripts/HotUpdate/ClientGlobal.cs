using JKFrame;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ClientGlobal : SingletonMono<ClientGlobal>
{
    public GameSetting gameSetting { get; private set; }

    public GameBasicSetting basicSetting { get; private set; }
    public static Vector2 canvasSize = new Vector2(1920, 1080); 

    private bool activeMouse;
    /// <summary>
    /// 鼠标指针显示和关闭的控制
    /// </summary>
    public bool ActiveMouse
    {
        get => activeMouse;
        set
        {
            activeMouse = value;
            Cursor.lockState = activeMouse ? CursorLockMode.None : CursorLockMode.Locked;
            //鼠标显示的修改事件_触发类型事件
            EventSystem.TypeEventTrigger(new MouseActiveStateChangedEvent { activeState = activeMouse });
        }
    }
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
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_Disconnect, OnDisconnect); //注册使用OnDisconnect方法
        Debug.Log("InitClient 成功");
        EnterLoginScene();

    }

    private void Update()
    {
        //Tab 鼠标取反
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ActiveMouse = !ActiveMouse;
        }
    }

    /// <summary>
    /// 注册Windows
    /// </summary>
    public void InitWindowData()
    {
        //TODO 每次新加的窗口，都需要在这里定义UI_WindowData

        //做一个对框架上修改的窗口，不依赖于静态的配置
        UISystem.AddUIWindowData<UI_MainMenuWindow>(new UIWindowData(false, nameof(UI_MainMenuWindow), 0));
        //弹窗的层级给高一些，给个4吧。
        //弹窗需要缓存，所以设置true,因为这个是高频使用的弹窗
        UISystem.AddUIWindowData<UI_MessagePopupWindow>(new UIWindowData(true, nameof(UI_MessagePopupWindow), 4));
        UISystem.AddUIWindowData<UI_RegisterWindow>(new UIWindowData(false, nameof(UI_RegisterWindow), 1));
        UISystem.AddUIWindowData<UI_LoginWindow>(new UIWindowData(false, nameof(UI_LoginWindow), 1));
        UISystem.AddUIWindowData<UI_GamePopupWindow>(new UIWindowData(false, nameof(UI_GamePopupWindow), 3));
        UISystem.AddUIWindowData<UI_GameSettingsWindow>(new UIWindowData(false, nameof(UI_GameSettingsWindow), 3));
        //把框架的2层，设置为2层 之间的ui互相不会遮挡， 比如商店背包互相拖拽，还有聊天
        UISystem.AddUIWindowData<UI_ChatWindow>(new UIWindowData(false, nameof(UI_ChatWindow), 2));
        //背包内容多，还需要高频开关使用，所以开缓存true。
        //开了缓存的，在关掉的时候我们做逻辑，给他放入对象池！！
        UISystem.AddUIWindowData<UI_BagWindow>(new UIWindowData(true, nameof(UI_BagWindow), 2));
        UISystem.AddUIWindowData<UI_ItemInfoPopupWindow>(new UIWindowData(true, nameof(UI_ItemInfoPopupWindow), 2));
        //道具栏：常驻窗口，不需要去缓存
        UISystem.AddUIWindowData<UI_ShortcutBarWindow>(new UIWindowData(false, nameof(UI_ShortcutBarWindow), 2));
        UISystem.AddUIWindowData<UI_ShopWindow>(new UIWindowData(true, nameof(UI_ShopWindow), 2));

    }
    private void OnGameSceneLaunchEvent(GameSceneLaunchEvent @event)
    {
        ResSystem.InstantiateGameObject("ClientOnGameScene");
    }

    private void LoadGameSetting()
    {
        gameSetting = SaveSystem.LoadSetting<GameSetting>();
        basicSetting = SaveSystem.LoadSetting<GameBasicSetting>();
        if (gameSetting == null)
        {
            gameSetting = new GameSetting();
            SaveGameSetting();
        }
        if (basicSetting == null)
        {
            //初始化
            basicSetting = new GameBasicSetting();
            basicSetting.languageType = Application.systemLanguage == SystemLanguage.ChineseSimplified ? LanguageType.SimplifiedChinese : LanguageType.English;
            SaveGameSetting();
        }
        //本地化的语言在初始化游戏的时候重新检测赋值为本地化语言类型
        LocalizationSystem.LanguageType = basicSetting.languageType;

        AudioSystem.BGVolume = gameSetting.musicVolume;
        AudioSystem.EffectVolume = gameSetting.soundEffectVolume;
    }
    public void SaveGameSetting()
    {
        //低频的，写到磁盘里去
        SaveSystem.SaveSetting(gameSetting);
    }
    public void SaveGameBasicSetting()
    {
        SaveSystem.SaveSetting(basicSetting);
    }
    public void RememberAccount(string name,string password)
    {
        gameSetting.rememberPlayerName = name;
        gameSetting.rememberPassword = password;
        SaveGameSetting();
    }
    public void EnterLoginScene()
    {
        UISystem.CloseAllWindow();
        //LoginScene，我们通过Addressables来做加载
        Addressables.LoadSceneAsync("LoginScene").WaitForCompletion();
    }
    public void EnterGameScene()
    {
        UISystem.CloseAllWindow();
        SceneSystem.LoadScene("GameScene");
    }
    private void OnDisconnect(ulong clientID, INetworkSerializable serializable)
    {
        S_C_Disconnect message = (S_C_Disconnect)serializable;
        if (message.errorCode != ErrorCode.None)
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalizationKey(message.errorCode.ToString(), Color.red);
            //延迟1秒，进入登录场景
            Invoke(nameof(EnterLoginScene), 1);
        }
        else EnterLoginScene();
        Debug.Log("退出到登录场景");
    }
}
