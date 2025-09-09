using JKFrame;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;


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
        reader.ReadValueSafe(out MessageType messageType);
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
        }
    }
    /// <summary>
    /// 做一个消息的包装，方便给SendMessageToServer使用
    /// </summary>
    private FastBufferWriter WriteData<T>(MessageType messageType, T data) where T : INetworkSerializable
    {
        //默认1024字节，不足时内部会自动扩展
        FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp);
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
    public void SendMessageAllClients<T>(MessageType messageType, T data, IReadOnlyList<ulong> clientIDS) where T : INetworkSerializable //约束类型T为INetworkSerializable
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
        if(receiveMessageCallbackDic.TryGetValue(messageType,out Action<ulong,INetworkSerializable> callback))
        {
            callback?.Invoke(clientID, data);
        }
    }


}
