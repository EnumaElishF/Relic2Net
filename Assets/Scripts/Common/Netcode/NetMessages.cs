using Unity.Netcode;

public enum MessageType : byte
{
    //用byte比int好处就是，缩短消息串的长度并提升性能。->尤其在高频消息传输场景下效果会更明显
    None,
    //注册
    C_S_Register, 
    S_C_Register,
    //登录
    C_S_Login,
    S_C_Login,
    C_S_EnterGame,
    //断开
    C_S_Disconnect,
    S_C_Disconnect,
    //聊天消息
    C_S_ChatMessage,
    S_C_ChatMessage,
    //背包数据
    C_S_GetBagData,
    S_C_GetBagData,
    //客户端使用物品和服务端给更新物品
    C_S_UseItem,
    S_C_UpdateItem
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
        //序列化 / 反序列化 NetCode支持的一种可以两个都进行的方法功能
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


public struct C_S_GetBagData : INetworkSerializable
{
    public int dataVersion;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref dataVersion);
    }
}

public struct S_C_GetBagData : INetworkSerializable
{
    public bool haveBagData;
    public BagData bagData;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref haveBagData);
        if (!haveBagData) return;
        if(serializer.IsReader) //反序列化,这时如果haveBagData = true，意味着要保存背包数据
        {
            if (bagData == null) bagData = new BagData();
            bagData.NetworkSerialize(serializer);
        }
        else //if (serializer.IsWriter) //序列化，对象转数据
        {
            bagData.NetworkSerialize(serializer);
        }
    }
}

public struct C_S_UseItem : INetworkSerializable
{
    //使用物品，先确定物品索引
    public int itemIndex; //背包中的位置

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemIndex);
    }
}
public struct S_C_UpdateItem : INetworkSerializable
{
    public int bagDataVersion;
    public int itemIndex;
    public ItemType itemType;
    //TODO 武器使用后，相当于玩家要切换武器
    public ItemDataBase newItemData;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref bagDataVersion);
        serializer.SerializeValue(ref itemIndex);
        serializer.SerializeValue(ref itemType);
        if(serializer.IsReader)//反序列化，将数据转为对象，则要求数据不能为null
        {
            switch (itemType)
            {
                case ItemType.Weapon:
                    newItemData = new WeaponData();
                    break;
                case ItemType.Consumable:
                    newItemData = new ConsumableData();
                    break;
                case ItemType.Material:
                    newItemData = new MaterialData();
                    break;
            }
        }
        if (newItemData != null) newItemData.NetworkSerialize(serializer);

    }
}
