using JKFrame;
using System.Collections.Generic;
/// <summary>
/// 作为分布类partial：可以多个文件组成一个类（ClientsManager内容太多，所以拆分成功能系统）
/// </summary>
public partial class ClientsManager : SingletonMono<ClientsManager> //SingletonMono加入对Singleton的通用基类改造，
{
    #region 主要数据
    private Dictionary<ClientState, HashSet<Client>> clientStateDic;
    // Key : ClientID
    public Dictionary<ulong, Client> clientIDDic;
    //Key:账号 Value:ClientID
    private Dictionary<string, ulong> accountDic;
    #endregion

    #region 自身逻辑_主要逻辑
    /// <summary>
    /// 初始化   需要由服务器开启，作为客户端们的管理者（考虑一个位置，谁去做他的初始化是最好的情况？
    /// </summary>
    public void Init()
    {
        clientStateDic = new Dictionary<ClientState, HashSet<Client>>()
        {
            {ClientState.Connected,new HashSet<Client>(100) },
            {ClientState.Logined,new HashSet<Client>(100) },
            {ClientState.Gaming,new HashSet<Client>(100) },
        };
        clientIDDic = new Dictionary<ulong, Client>(100);
        accountDic = new Dictionary<string, ulong>(100);

        NetManager.Instance.OnClientConnectedCallback += OnClientConnected;
        NetManager.Instance.OnClientDisconnectCallback += OnClientNetCodeDisconnect;

        //NetMessageManager注册网络事件->已被迁移拆分成不同系统
        InitLoginSystem();
        InitChatSystem();
        InitItemSystem();
    }

    /// <summary>
    /// 标准的状态切换的函数，方便管理后续状态切换
    /// </summary>
    private void SetClientState(ulong clientID, ClientState newState)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client))
        {
            clientStateDic[client.clientState].Remove(client);
            clientStateDic[newState].Add(client);
            client.clientState = newState;
        }
    }
    #endregion

    #region 连接与退出
    /// <summary>
    /// 连接成功
    /// </summary>
    private void OnClientConnected(ulong clientID)
    {

        //用对象池去处理，构建一个Client
        Client client = ResSystem.GetOrNew<Client>();
        client.clientID = clientID;
        clientIDDic.Add(clientID, client);
        SetClientState(clientID, ClientState.Connected);

    }
    /// <summary>
    /// 客户端完全退出，断开连接
    /// </summary>
    private void OnClientNetCodeDisconnect(ulong clientID)
    {
        if (clientIDDic.Remove(clientID, out Client client))
        {
            clientStateDic[client.clientState].Remove(client);
            if (client.playerData != null) accountDic.Remove(client.playerData.name);
            // 如果不使用下面这条，那么采用的是NetCode自己的管理，也就是客户端掉线会自动清除所属的网络对象
            if (client.playerController != null) NetManager.Instance.DestroyObject(client.playerController.mainController.NetworkObject);
            client.playerData = null;
            client.playerController = null;
            client.OnDestroy();
        }
    }
 

    #endregion




}
