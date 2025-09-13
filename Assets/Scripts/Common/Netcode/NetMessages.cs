using Unity.Netcode;

public enum MessageType : byte
{
    //用byte比int好处就是，缩短消息串的长度并提升性能。->尤其在高频消息传输场景下效果会更明显
    None,
    C_S_Register, 
    S_C_Register,
    C_S_Login,
    S_C_Login,
    C_S_EnterGame,
    C_S_Disconnect,
    S_C_Disconnect,
    C_S_ChatMessage,
    S_C_ChatMessage
}
/// <summary>
/// 可以预知的完成的返回信息，做成Code的方式，做个可知的码。比如服务端返回码等
/// 反之，像是装备码id或者账号返回信息不可预知那就不用这种，通过做数据去做,例如AccountInfo
/// </summary>
public enum ErrorCode : byte
{
    None,                //意味着成功
    AccountFormat,       //账号格式错误
    NameDuplication,     //名称重复
    NameOrPassword,      //名称或密码错误
    AccountRepeatLogin,  // 账号重复登录

}

/// <summary>
/// 账号信息
/// </summary>
public struct AccountInfo : INetworkSerializable
{
    public string playerName;
    public string password;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref password);
    }
}

/// <summary>
/// 从Client发到Server的消息,C2S
/// INetworkSerializable网络序列化请求
/// </summary>
public struct C_S_Register : INetworkSerializable
{
    public AccountInfo accountInfo;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        accountInfo.NetworkSerialize(serializer);//初始化，直接使用序列化
    }
}
public struct S_C_Register : INetworkSerializable
{
    public ErrorCode errorCode;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);

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
public struct S_C_Login : INetworkSerializable
{
    public ErrorCode errorCode;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);

    }
}
public struct C_S_EnterGame : INetworkSerializable
{
    public ErrorCode errorCode; ///纯占位,以后还有可能再加一些需要的数据消息Code

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);

    }
}
public struct C_S_Disconnect : INetworkSerializable
{
    public ErrorCode errorCode;///纯占位

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);

    }
}
public struct S_C_Disconnect : INetworkSerializable
{
    public ErrorCode errorCode; 

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);

    }
}

public struct C_S_ChatMessage : INetworkSerializable
{
    public string message;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref message);

    }
}
public struct S_C_ChatMessage : INetworkSerializable
{
    //服务端发给客户端是广播的形式，需要知道让其他人是哪个玩家发的，加上playerName
    public string playerName;
    public string message;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref message);

    }
}