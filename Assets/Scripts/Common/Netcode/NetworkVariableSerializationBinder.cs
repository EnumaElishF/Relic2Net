
using System;
using Unity.Collections;
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
        //TODO 以后 如果要加新的State，那就在这里继续加入
        BindUserNetworkVariableSerialization<PlayerState>();

        //为 FixedString32Bytes 显式注册网络序列化逻辑，确保 AOT 环境下能正确处理。
        BindFixedString32BytesSerialization();
        BindFloatSerialization();

    }
    public static void BindFloatSerialization()
    {
        UserNetworkVariableSerialization<float>.WriteValue = (FastBufferWriter writer, in float value) =>
        {
            writer.WriteValueSafe(value);
        };
        UserNetworkVariableSerialization<float>.ReadValue = (FastBufferReader reader, out float value) =>
        {
            reader.ReadValueSafe(out value);
        };
        UserNetworkVariableSerialization<float>.DuplicateValue = (in float value, ref float duplicateValue) =>
        {
            duplicateValue = value;
        };
    }
    public static void BindFixedString32BytesSerialization()
    {
        UserNetworkVariableSerialization<FixedString32Bytes>.WriteValue = (FastBufferWriter writer, in FixedString32Bytes value) =>
        {
            writer.WriteValueSafe(value);
        };
        UserNetworkVariableSerialization<FixedString32Bytes>.ReadValue = (FastBufferReader reader, out FixedString32Bytes value) =>
        {
            reader.ReadValueSafe(out value);
        };
        UserNetworkVariableSerialization<FixedString32Bytes>.DuplicateValue = (in FixedString32Bytes value, ref FixedString32Bytes duplicateValue) =>
        {
            duplicateValue = value;
        };
    }
    public static void BindUserNetworkVariableSerialization<T>() where T : unmanaged, Enum
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
