using JKFrame;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientsManager : SingletonMono<ClientsManager> //SingletonMono加入对Singleton的通用基类改造，
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

        //NetMessageManager注册网络事件
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Register, OnClientRegister);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Login, OnClientLogin);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_EnterGame, OnClientEnterGame);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_Disconnect, OnClientDisconnect);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_ChatMessage, OnClientChatMessage);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_GetBagData, OnClientGetBagData);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_UseItem, OnClientUseItem);

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
            if (client.playerController != null) NetManager.Instance.DestroyObject(client.playerController.NetworkObject);
            client.playerData = null;
            client.playerController = null;
            client.OnDestroy();
        }
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
            NetManager.Instance.DestroyObject(client.playerController.NetworkObject);
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
        };
        playerData.name = accountInfo.playerName;
        playerData.password = accountInfo.password;
        playerData.bagData = new BagData();

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
                            NetManager.Instance.DestroyObject(oldClient.playerController.NetworkObject);
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
        NetworkObject playerObject = NetManager.Instance.SpawnObject(clientID, ServerResSystem.serverConfig.playerPrefab, characterData.position, Quaternion.Euler(0, characterData.rotation_Y, 0));
        client.playerController = playerObject.GetComponent<PlayerController>();
        //TODO 玩家可能使用不同的武器之类的实例化

    }
    #endregion


    #region 聊天
    /// <summary>
    /// 当客户端发来聊天消息。服务器对客户端的聊天消息发送: 广播给全部的游戏中玩家
    /// </summary>
    private void OnClientChatMessage(ulong clientID, INetworkSerializable serializable)
    {
        string chatMessage = ((C_S_ChatMessage)serializable).message;
        if (string.IsNullOrWhiteSpace(chatMessage)) return; //消息有效性验证
        if (!clientIDDic.TryGetValue(clientID, out Client sourceClient) && sourceClient.playerData == null) return;//检查源头客户端的有效性
        //发送给所有游戏状态下的客户端
        if (clientStateDic.TryGetValue(ClientState.Gaming, out HashSet<Client> clients))
        {
            S_C_ChatMessage message = new S_C_ChatMessage { playerName = sourceClient.playerData.name, message = chatMessage };
            foreach (Client client in clients)
            {
                //这样就把消息转发出去了
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ChatMessage, message, client.clientID);
            }
        }

    }
    #endregion

    #region 物品
    /// <summary>
    /// 当客户端请求背包数据
    /// </summary>
    private void OnClientGetBagData(ulong clientID, INetworkSerializable serializable)
    {
        C_S_GetBagData message = (C_S_GetBagData)serializable;
        if(clientIDDic.TryGetValue(clientID,out Client client) && client.playerData != null)
        {
            S_C_GetBagData result = new S_C_GetBagData 
            { 
                haveBagData = client.playerData.bagData.dataVersion != message.dataVersion 
            };
            //客户端这边新版背包数据不为空，且客户端的背包版本发生变化，才会执行
            if (result.haveBagData) result.bagData = client.playerData.bagData;
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_GetBagData, result, clientID);
        }
    }
    /// <summary>
    /// 当客户端请求使用物品
    /// </summary>
    private void OnClientUseItem(ulong clientID, INetworkSerializable serializable)
    {
        C_S_UseItem message = (C_S_UseItem)serializable;
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            //TODO 具体的物品使用
            BagData bagData = client.playerData.bagData;
            //背包数据的TryUseItem原本是放服务器这边的，但是出现Common程序集获取内容的错误，应该是Unity打包识别不足够好，
            //最后我们还是把数据获取内容，转移到BagData
            ItemDataBase itemData = bagData.TryUseItem(message.itemIndex);
            ItemType itemType = ItemType.Empty;
            if (itemData != null) itemType = itemData.GetItemType();
            S_C_UpdateItem result = new S_C_UpdateItem
            {
                itemIndex = message.itemIndex,
                bagDataVersion = bagData.dataVersion,
                newItemData = itemData,
                itemType = itemType
            };
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_UpdateItem, result, clientID);
        }

    }
    #endregion

}
