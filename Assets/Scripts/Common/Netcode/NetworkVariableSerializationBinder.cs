
using System;
using Unity.Netcode;

/// <summary>
/// 网络变量序列化绑定器
/// </summary>
public static class NetworkVariableSerializationBinder
{
    //统一管理哪些枚举类型需要在网络中传输，
    //并为它们绑定标准化的序列化 / 反序列化 / 复制逻辑，方便扩展新的枚举类型（只需在Init中添加注册），
    //确保网络通信中枚举数据的正确处理
    public static void Init()
    {
        // 未来 如果要加新的State，那就在这里继续加入
        BindUserNNetworkVariableSerialization<PlayerState>();

    }
    public static void BindUserNNetworkVariableSerialization<T>() where T : unmanaged, Enum
    {
        //写入
        UserNetworkVariableSerialization<T>.WriteValue = (FastBufferWriter writer, in T value) =>
        {
            writer.WriteValueSafe(value);
        };
        //读取
        UserNetworkVariableSerialization<T>.ReadValue = (FastBufferReader reader, out T value) =>
        {
            reader.ReadValueSafe(out value);
        };
        //复制
        UserNetworkVariableSerialization<T>.DuplicateValue = (in T value, ref T duplicateValue) =>
        {
            duplicateValue = value;
        };
    }
}
