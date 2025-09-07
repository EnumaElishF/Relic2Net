using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
public enum MessageType : byte
{
    //用byte比int好处就是，缩短消息串的长度并提升性能。->尤其在高频消息传输场景下效果会更明显
    Test
}
public class TestData:INetworkSerializable
{
    public string name;
    public int lv;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref lv);
    }
}
/// <summary>
/// 网络消息管理: 网络消息序列化与回调
/// </summary>
public class NetMessageManager : MonoBehaviour
{
    private CustomMessagingManager messagingManager => NetManager.Instance.CustomMessagingManager;
    private Dictionary<MessageType, Action<ulong,INetworkSerializable>> receiveMessageCallbackDic = new Dictionary<MessageType, Action<ulong, INetworkSerializable>>();
    public void Init()
    {
        messagingManager.OnUnnamedMessage += ReceiveMessage; //使用的Action回调
        NetManager.Instance.OnClientConnectedCallback += Instance_OnClientConnectedCallback;
        RegisterMessageCallback(MessageType.Test, TestCallback);
    }
    /// <summary>
    /// 回调Callback，自定义回调方法内容
    /// </summary>
    /// <param name="clientID">客户端id，知道客户端是谁的</param>
    /// <param name="serializable"></param>
    private void TestCallback(ulong clientID,INetworkSerializable serializable)
    {
        TestData testData = (TestData)serializable; //序列化的强制转换回
        Debug.Log($"收到信息(回调):{clientID},{testData.name},{testData.lv}");
    }

    private void Instance_OnClientConnectedCallback(ulong obj)
    {
        //登录以后发送一个消息给Server
        SendMessageToServer(MessageType.Test, new TestData
        {
            name = "客户端登录了",
            lv = 11
        });
    }

    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out MessageType messageType);
        switch (messageType)
        {
            case MessageType.Test:
                reader.ReadValueSafe(out TestData testData);
                Debug.Log($"收到信息: {testData.name},{testData.lv}");
                TriggerMessageCallback(messageType, clientId, testData); //接收到不同的消息以后，给他传出去
                break;
            default:
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
    private void SendMessageToServer<T>(MessageType messageType,T data) where T: INetworkSerializable //约束类型T为INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(NetManager.ServerClientId, WriteData(messageType,data));
    }
    private void SendMessageToClient<T>(MessageType messageType, T data,ulong clientID) where T : INetworkSerializable //约束类型T为INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(clientID, WriteData(messageType, data));
    }
    private void SendMessageToClients<T>(MessageType messageType, T data, IReadOnlyList<ulong> clientIDS) where T : INetworkSerializable //约束类型T为INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(clientIDS, WriteData(messageType, data));
    }
    private void SendMessageAllClients<T>(MessageType messageType, T data, IReadOnlyList<ulong> clientIDS) where T : INetworkSerializable //约束类型T为INetworkSerializable
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
