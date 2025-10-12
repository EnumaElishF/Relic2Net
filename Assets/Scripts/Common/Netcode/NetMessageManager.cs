using JKFrame;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// 网络消息管理: 网络消息序列化与回调
/// </summary>
public class NetMessageManager : SingletonMono<NetMessageManager>
{
    private CustomMessagingManager messagingManager => NetManager.Instance.CustomMessagingManager;
    private Dictionary<MessageType, Action<ulong,INetworkSerializable>> receiveMessageCallbackDic = new Dictionary<MessageType, Action<ulong, INetworkSerializable>>();
    public void Init()
    {
        messagingManager.OnUnnamedMessage += ReceiveMessage; //使用的Action回调
    }
    /// <summary>
    /// 回调Callback，自定义回调方法内容
    /// </summary>
    /// <param name="clientID">客户端id，知道客户端是谁的</param>
    /// <param name="serializable"></param>
    //private void TestCallback(ulong clientID,INetworkSerializable serializable)
    //{
    //    TestData testData = (TestData)serializable; //序列化的强制转换回
    //    Debug.Log($"收到信息(回调):{clientID},{testData.name},{testData.lv}");
    //}


    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        try
        {
            //TODO 每次加入新消息类型-> 其实这里可以做成自动化的，例如：通过特性的方式，运行的时候就注册进来，这里是只有一次的反射？要是反射多了，那么考虑自动化
            reader.ReadValueSafe(out MessageType messageType);
            Debug.Log("收到网络信息:" + messageType);
            switch (messageType)
            {
                case MessageType.C_S_Register:
                    reader.ReadValueSafe(out C_S_Register C_S_Register);
                    TriggerMessageCallback(MessageType.C_S_Register, clientId, C_S_Register);
                    break;
                case MessageType.C_S_Login:
                    reader.ReadValueSafe(out C_S_Login C_S_Login);
                    TriggerMessageCallback(MessageType.C_S_Login, clientId, C_S_Login);
                    break;
                case MessageType.S_C_Register:
                    reader.ReadValueSafe(out S_C_Register S_C_Register);
                    TriggerMessageCallback(MessageType.S_C_Register, clientId, S_C_Register);
                    break;
                case MessageType.S_C_Login:
                    reader.ReadValueSafe(out S_C_Login S_C_Login);
                    TriggerMessageCallback(MessageType.S_C_Login, clientId, S_C_Login);
                    break;
                case MessageType.C_S_EnterGame:
                    reader.ReadValueSafe(out C_S_EnterGame C_S_EnterGame);
                    TriggerMessageCallback(MessageType.C_S_EnterGame, clientId, C_S_EnterGame);
                    break;
                case MessageType.C_S_Disconnect:
                    reader.ReadValueSafe(out C_S_Disconnect C_S_Disconnect);
                    TriggerMessageCallback(MessageType.C_S_Disconnect, clientId, C_S_Disconnect);
                    break;
                case MessageType.S_C_Disconnect:
                    reader.ReadValueSafe(out S_C_Disconnect S_C_Disconnect);
                    TriggerMessageCallback(MessageType.S_C_Disconnect, clientId, S_C_Disconnect);
                    break;
                case MessageType.C_S_ChatMessage:
                    reader.ReadValueSafe(out C_S_ChatMessage C_S_ChatMessage);
                    TriggerMessageCallback(MessageType.C_S_ChatMessage, clientId, C_S_ChatMessage);
                    break;
                case MessageType.S_C_ChatMessage:
                    reader.ReadValueSafe(out S_C_ChatMessage S_C_ChatMessage);
                    TriggerMessageCallback(MessageType.S_C_ChatMessage, clientId, S_C_ChatMessage);
                    break;
                case MessageType.C_S_GetBagData:
                    reader.ReadValueSafe(out C_S_GetBagData C_S_GetBagData);
                    TriggerMessageCallback(MessageType.C_S_GetBagData, clientId, C_S_GetBagData);
                    break;
                case MessageType.S_C_GetBagData:
                    reader.ReadValueSafe(out S_C_GetBagData S_C_GetBagData);
                    TriggerMessageCallback(MessageType.S_C_GetBagData, clientId, S_C_GetBagData);
                    break;
                case MessageType.C_S_BagUseItem:
                    reader.ReadValueSafe(out C_S_BagUseItem C_S_BagUseItem);
                    TriggerMessageCallback(MessageType.C_S_BagUseItem, clientId, C_S_BagUseItem);
                    break;
                case MessageType.S_C_BagUpdateItem:
                    reader.ReadValueSafe(out S_C_BagUpdateItem S_C_BagUpdateItem);
                    TriggerMessageCallback(MessageType.S_C_BagUpdateItem, clientId, S_C_BagUpdateItem);
                    break;
                case MessageType.C_S_BagSwapItem:
                    reader.ReadValueSafe(out C_S_BagSwapItem C_S_BagSwapItem);
                    TriggerMessageCallback(MessageType.C_S_BagSwapItem, clientId, C_S_BagSwapItem);
                    break;
                case MessageType.S_C_ShortcutBarUpdateItem:
                    reader.ReadValueSafe(out S_C_ShortcutBarUpdateItem S_C_ShortcutBarUpdateItem);
                    TriggerMessageCallback(MessageType.S_C_ShortcutBarUpdateItem, clientId, S_C_ShortcutBarUpdateItem);
                    break;
                case MessageType.C_S_ShortcutBarSetItem:
                    reader.ReadValueSafe(out C_S_ShortcutBarSetItem C_S_ShortcutBarSetItem);
                    TriggerMessageCallback(MessageType.C_S_ShortcutBarSetItem, clientId, C_S_ShortcutBarSetItem);
                    break;
                case MessageType.C_S_ShortcutBarSwapItem:
                    reader.ReadValueSafe(out C_S_ShortcutBarSwapItem C_S_ShortcutBarSwapItem);
                    TriggerMessageCallback(MessageType.C_S_ShortcutBarSwapItem, clientId, C_S_ShortcutBarSwapItem);
                    break;
                case MessageType.C_S_ShopBuyItem:
                    reader.ReadValueSafe(out C_S_ShopBuyItem C_S_ShopBuyItem);
                    TriggerMessageCallback(MessageType.C_S_ShopBuyItem, clientId, C_S_ShopBuyItem);
                    break;
                case MessageType.S_C_UpdateCoinCount:
                    reader.ReadValueSafe(out S_C_UpdateCoinCount S_C_UpdateCoinCount);
                    TriggerMessageCallback(MessageType.S_C_UpdateCoinCount, clientId, S_C_UpdateCoinCount);
                    break;
            }
        }
        catch(Exception e)
        {
            Debug.Log("消息接收失败!"+e.Message);
        }

    }
    /// <summary>
    /// 做一个消息的包装，方便给SendMessageToServer使用
    /// </summary>
    private FastBufferWriter WriteData<T>(MessageType messageType, T data) where T : INetworkSerializable
    {
        // 默认1024字节，当不足时候会在10240范围内自动扩容
        FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp, 10240);
        using (writer)
        {
            writer.WriteValueSafe(messageType); //协议头
            writer.WriteValueSafe(data); //协议主体
        }
        return writer;

    }

    //几种不同的消息发送
    public void SendMessageToServer<T>(MessageType messageType,T data) where T: INetworkSerializable //约束类型T为INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(NetManager.ServerClientId, WriteData(messageType,data));
    }
    public void SendMessageToClient<T>(MessageType messageType, T data,ulong clientID) where T : INetworkSerializable //约束类型T为INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(clientID, WriteData(messageType, data));
    }
    public void SendMessageToClients<T>(MessageType messageType, T data, IReadOnlyList<ulong> clientIDS) where T : INetworkSerializable //约束类型T为INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(clientIDS, WriteData(messageType, data));
    }
    public void SendMessageAllClient<T>(MessageType messageType, T data, IReadOnlyList<ulong> clientIDS) where T : INetworkSerializable //约束类型T为INetworkSerializable
    {
        messagingManager.SendUnnamedMessageToAll( WriteData(messageType, data));
    }

    //回调
    /// <summary>
    /// 注册的回调
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="callback"></param>
    public void RegisterMessageCallback(MessageType messageType,Action<ulong,INetworkSerializable> callback)
    {
        if (receiveMessageCallbackDic.ContainsKey(messageType))
        {
            receiveMessageCallbackDic[messageType] += callback;
        }
        else
        {
            receiveMessageCallbackDic.Add(messageType, callback);
        }
    }
    /// <summary>
    /// 取消注册的回调
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="callback"></param>
    public void UnRegisterMessageCallback(MessageType messageType, Action<ulong, INetworkSerializable> callback)
    {
        if (receiveMessageCallbackDic.ContainsKey(messageType))
        {
            receiveMessageCallbackDic[messageType] -= callback;
        }
    }

    /// <summary>
    /// 触发
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="data"></param>
    private void TriggerMessageCallback(MessageType messageType, ulong clientID, INetworkSerializable data)
    {
        if (receiveMessageCallbackDic.TryGetValue(messageType, out Action<ulong, INetworkSerializable> callback))
        {
            callback?.Invoke(clientID, data);
        }
    }


}
