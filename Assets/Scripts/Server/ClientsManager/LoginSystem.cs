using JKFrame;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 负责登录系统的部分
/// </summary>
public partial class ClientsManager : SingletonMono<ClientsManager>
{
    /// <summary>
    /// 虽然是一个文件，都是ClientsManager，但是我们还是按照系统的思路来
    /// </summary>
    public void InitLoginSystem()
    {
        //NetMessageManager注册网络事件
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Register, OnClientRegister);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Login, OnClientLogin);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_EnterGame, OnClientEnterGame);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Disconnect, OnClientDisconnect);
    }
    #region 注册与登录
    /// <summary>
    /// 申请注册
    /// </summary>
    private void OnClientRegister(ulong clientID, INetworkSerializable serializable)
    {
        C_S_Register netMessage = (C_S_Register)serializable;
        AccountInfo accountInfo = netMessage.accountInfo;
        S_C_Register result = new S_C_Register { errorCode = ErrorCode.None };
        //校验格式
        if (!AccountFormatUtility.CheckName(accountInfo.playerName)
            || !AccountFormatUtility.CheckPassword(accountInfo.password))
        {
            result.errorCode = ErrorCode.AccountFormat;
        }
        //校验是否已有玩家
        else if (DataBaseManager.Instance.GetPlayerData(accountInfo.playerName) != null)
        {
            result.errorCode = ErrorCode.NameDuplication;
        }
        else
        {
            CreateDefaultPlayerData(accountInfo);
        }
        //回复客户端
        NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_Register, result, clientID);

    }
    /// <summary>
    /// 生成默认的账号数据
    /// </summary>
    private PlayerData CreateDefaultPlayerData(AccountInfo accountInfo)
    {
        PlayerData playerData = ResSystem.GetOrNew<PlayerData>();
        playerData.characterData = new CharacterData
        {
            position = ServerResSystem.serverConfig.playerDefaultPosition,
            usedWeaponName = "Weapon_0"
        };
        playerData.name = accountInfo.playerName;
        playerData.password = accountInfo.password;

        playerData.bagData.itemList[0] = (new WeaponData() { id = "Weapon_0" });
        playerData.bagData.itemList[1] = (new WeaponData() { id = "Weapon_1" });
        playerData.bagData.itemList[2] = (new ConsumableData() { id = "Consumable_0", count = 1 });
        playerData.bagData.itemList[3] = (new ConsumableData() { id = "Consumable_1", count = 2 });
        playerData.bagData.itemList[4] = (new ConsumableData() { id = "Consumable_2", count = 3 });
        playerData.bagData.itemList[5] = (new ConsumableData() { id = "Consumable_3", count = 4 });
        playerData.bagData.itemList[6] = (new ConsumableData() { id = "Consumable_4", count = 5 });
        playerData.bagData.itemList[7] = (new MaterialData() { id = "Material_0", count = 5 });
        playerData.bagData.itemList[8] = (new MaterialData() { id = "Material_1", count = 7 });
        playerData.bagData.itemList[9] = (new MaterialData() { id = "Material_2", count = 8 });
        playerData.bagData.shortcutBarIndexs[0] = 0;
        playerData.bagData.coinCount = ServerResSystem.serverConfig.playerDefaultCoinCount;
        DataBaseManager.Instance.CreatePlayerData(playerData);
        return playerData;
    }
    /// <summary>
    /// 申请登录
    /// </summary>
    private void OnClientLogin(ulong clientID, INetworkSerializable serializable)
    {
        C_S_Login netMessage = (C_S_Login)serializable;
        AccountInfo accountInfo = netMessage.accountInfo;
        S_C_Login result = new S_C_Login { errorCode = ErrorCode.None };
        //校验格式
        if (!AccountFormatUtility.CheckName(accountInfo.playerName)
            || !AccountFormatUtility.CheckPassword(accountInfo.password))
        {
            result.errorCode = ErrorCode.AccountFormat;
        }
        else
        {
            //检查是否有这个玩家，并且账号信息正确
            PlayerData playerData = DataBaseManager.Instance.GetPlayerData(accountInfo.playerName);
            if (playerData == null || playerData.password != accountInfo.password)
            {
                result.errorCode = ErrorCode.NameOrPassword;
            }
            else
            {
                //检查 挤号
                if (accountDic.TryGetValue(accountInfo.playerName, out ulong oldClientID))
                {
                    //通知旧客户端
                    NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_Disconnect, new S_C_Disconnect
                    {
                        errorCode = ErrorCode.AccountRepeatLogin
                    }, oldClientID);
                    //设置旧客户端为已连接但是未登录状态
                    SetClientState(oldClientID, ClientState.Connected);
                    //可能存在的角色需要销毁,因为登录的人还不一定产生了角色
                    if (clientIDDic.TryGetValue(oldClientID, out Client oldClient))
                    {
                        if (oldClient.playerController != null)
                        {
                            NetManager.Instance.DestroyObject(oldClient.playerController.mainController.NetworkObject);
                            oldClient.playerController = null;
                        }
                        oldClient.playerData = null;
                    }
                }
                //修改当前值，能自动新增或修改。accountDic[key] = value 的方式赋值时，如果这个键不存在，会自动新增一个键值对
                accountDic[accountInfo.playerName] = clientID;

                //玩家登录成功,关联Client和PlayerData
                Client client = clientIDDic[clientID];
                client.playerData = playerData;
                client.playerData.bagData.dataVersion = 0; //背包版本号默认为0
                SetClientState(clientID, ClientState.Logined);
            }
        }
        //回复客户端
        NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_Login, result, clientID);
    }

    /// <summary>
    /// 玩家进入游戏
    /// </summary>
    private void OnClientEnterGame(ulong clientID, INetworkSerializable serializable)
    {
        //无需回复客户端，直接创建角色
        Client client = clientIDDic[clientID];
        if (client.clientState == ClientState.Gaming) return;
        SetClientState(clientID, ClientState.Gaming);

        PlayerData playerData = client.playerData;
        CharacterData characterData = playerData.characterData;
        //生成游戏对象
        NetworkObject playerObject = NetManager.Instance.SpawnObjectNoShow(clientID, ServerResSystem.serverConfig.playerPrefab, characterData.position, Quaternion.Euler(0, characterData.rotation_Y, 0));
        // 初始化玩家的服务端控制脚本
        if(!playerObject.TryGetComponent(out PlayerServerController serverController))
        {
            serverController = playerObject.gameObject.AddComponent<PlayerServerController>();
            serverController.FirstInit(playerObject.GetComponent<PlayerController>());
        }
        serverController.Init();

        //！生成游戏对象后续操作，网络同步和显示：为什么要这样做呢？是为了处理上面的serverController的FirstInit的PlayerController里有对PlayerServerController的使用情况，保证PlayerServerController先加载完
        playerObject.SpawnWithOwnership(clientID); //生成, !!!重点关注这里对应的OnNetWorkSpawn
        playerObject.NetworkShow(clientID);

        serverController.mainController.playerName.Value = playerData.name;
        //玩家可能使用不同的武器之类的实例化
        serverController.mainController.usedWeaponName.Value = playerData.characterData.usedWeaponName;
        //Debug.Log($"服务端设置初始武器：{playerData.characterData.usedWeaponName}");
        client.playerController = serverController;
    }

    /// <summary>
    /// 客户端退出到开始菜单场景
    /// </summary>
    private void OnClientDisconnect(ulong clientID, INetworkSerializable serializable)
    {
        Client client = clientIDDic[clientID];
        //设置旧客户端为已连接但是未登录状态
        SetClientState(clientID, ClientState.Connected);
        //销毁角色
        if (client.playerController != null)
        {
            NetManager.Instance.DestroyObject(client.playerController.mainController.NetworkObject);
            client.playerController = null;
        }
        if (client.playerData != null)
        {
            //退出账号
            accountDic.Remove(client.playerData.name);
            client.playerData = null;
        }
        //注：这里对于client是存在的，不会移除

        //回复消息
        NetMessageManager.Instance.SendMessageToClient<S_C_Disconnect>(MessageType.S_C_Disconnect, default, clientID);
    }
    #endregion


}