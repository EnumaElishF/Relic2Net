using MongoDB.Bson.Serialization.Attributes;
using Unity.Netcode;
//因为MongoDB是支持抽象的，但是无法直接知道是什么类型的数据，我们给他做BsonKnownTypes标记,!如果这里数据类型标记错了，那么就会导致NetMessageManager背包类型序列化出现错误
[BsonKnownTypes(typeof(WeaponData), typeof(ConsumableData), typeof(MaterialData))]
public abstract class ItemDataBase: INetworkSerializable
{
    public string id; //对应物品的ID，同时也是配置在Addressables中的key

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        //序列化 、反序列化 id
        serializer.SerializeValue(ref id);
    }
}