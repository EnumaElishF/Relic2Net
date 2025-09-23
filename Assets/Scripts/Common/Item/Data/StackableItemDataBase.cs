
using Unity.Netcode;

/// <summary>
/// 可堆叠物品的基类，count数量记录
/// 继承自ItemDataBase不加abstract抽象，就必须去实现父类的全部抽象成员否则会错误
/// </summary>
public abstract class StackableItemDataBase: ItemDataBase
{
    public int count;
    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);
        serializer.SerializeValue(ref count);
    }
}