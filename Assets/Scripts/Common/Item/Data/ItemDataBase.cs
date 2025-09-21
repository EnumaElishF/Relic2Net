using Unity.Netcode;

public abstract class ItemDataBase: INetworkSerializable
{
    public string id; //对应物品的ID，同时也是配置在Addressables中的key
    public int count;

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        //序列化 、反序列化 id
        serializer.SerializeValue(ref id);
    }
}