using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
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
/// 网络消息管理
/// </summary>
public class NetMessageManager : MonoBehaviour
{
    private CustomMessagingManager messagingManager => NetManager.Instance.CustomMessagingManager;
    public void Init()
    {
        messagingManager.OnUnnamedMessage += ReceiveMessage; //使用的Action回调
        NetManager.Instance.OnClientConnectedCallback += Instance_OnClientConnectedCallback;
    }

    private void Instance_OnClientConnectedCallback(ulong obj)
    {
        SendMessageToServer();
    }

    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe<TestData>(out TestData testData);
        Debug.Log($"收到信息: {testData.name},{testData.lv}");

    }
    private void SendMessageToServer()
    {
        FastBufferWriter writer = new FastBufferWriter(1024,Allocator.Temp);
        writer.WriteValueSafe(new TestData()
        {
            name="客户端发来的信息",
            lv=10
        });
        messagingManager.SendUnnamedMessage(NetManager.ServerClientId, writer);
    }
    [Button]
    private void SendMessageToAllClient(TestData testData)
    {
        FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp);
        writer.WriteValueSafe(testData);
        messagingManager.SendUnnamedMessageToAll(writer);
    }
}
