
using Unity.Netcode;

/// <summary>
/// 可堆叠物品的基类，count数量记录
/// </summary>
public class StackableItemDataBase: ItemDataBase
{
    public int count;
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref count);
    }
}