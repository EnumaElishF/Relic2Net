using Unity.Netcode;

public enum MessageType : byte
{
    //用byte比int好处就是，缩短消息串的长度并提升性能。->尤其在高频消息传输场景下效果会更明显
    None,
    C_S_Register, 
    C_S_Login
}
/// <summary>
/// 从Client发到Server的消息,C2S
/// </summary>
public struct C_S_Register : INetworkSerializable
{
    public AccountInfo accountInfo;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        accountInfo.NetworkSerialize(serializer);//初始化，直接使用序列化
    }
}
public struct C_S_Login : INetworkSerializable
{
    public AccountInfo accountInfo;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        accountInfo.NetworkSerialize(serializer);//初始化，直接使用序列化
    }
}
public struct AccountInfo: INetworkSerializable
{
    public string playerName;
    public string password;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref password);
    }
}