using System.Collections.Generic;
using Unity.Netcode;

public class BagData: INetworkSerializable
{
    //背包的格子是有固定的数量上限，空格子的表现为 itemList[i] = null
    public const int itemCount = 30;
    public List<ItemDataBase> itemList = new List<ItemDataBase>(itemCount);

    public BagData()
    {
        for(int i = 0; i < itemCount; i++)
        {
            itemList.Add(null);
        }
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        for(int i = 0; i < itemCount; i++)
        {
            if (serializer.IsReader) //反序列化, 数据转为对象
            {
                //ItemType itemType = default;
                //serializer.SerializeValue(ref itemType);
                //读取器
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValue(out ItemType itemType);
                //读出一个物品的物品类型
                switch (itemType)
                {
                    case ItemType.Empty:
                        itemList[i] = null;
                        break;
                    case ItemType.Weapon:
                        WeaponData weaponData = new WeaponData();
                        weaponData.NetworkSerialize(serializer);
                        itemList[i] = weaponData;
                        break;
                    case ItemType.Consumable:
                        ConsumableData consumableData = new ConsumableData();
                        consumableData.NetworkSerialize(serializer);
                        itemList[i] = consumableData;
                        break;
                    case ItemType.Material:
                        MaterialData materialData = new MaterialData();
                        materialData.NetworkSerialize(serializer);
                        itemList[i] = materialData;
                        break;
                }
            }
            else
            {
                //序列化
                // if (serializer.IsWriter) //序列化
                // 写入器
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                ItemDataBase itemData = itemList[i];
                if (itemData == null)
                {
                    writer.WriteValueSafe(ItemType.Empty);
                }
                else
                {
                    if (itemData is WeaponData) writer.WriteValueSafe(ItemType.Weapon);
                    else if (itemData is ConsumableData) writer.WriteValueSafe(ItemType.Consumable);
                    else if (itemData is MaterialData) writer.WriteValueSafe(ItemType.Material);
                    itemData.NetworkSerialize(serializer);
                }
            }
        }

    }

    #region Server 服务端背包数据->数据获取
#if UNITY_SERVER || UNITY_EDITOR
    public bool TryAddWeapon(string id,out int index)
    {
        if (TryGetFirstEmptyIndex(out index)) //拿出第一个itemList的空数据
        {
            WeaponData weaponData = new WeaponData();
            weaponData.id = id;
            return true;
        }
        return false;
    }
    public bool TryAddStackableItem<T>(string id, int count,out int index) where T: StackableItemDataBase, new()
    {
        //index在初始已定义int，后面无需定义
        ItemDataBase itemData = TryGetItem(id, out index);
        if (itemData != null) //已存在
        {
            if(itemData is T)
            {
                ((T)itemData).count += count;
                return true;
            }
            
        }
        else if(TryGetFirstEmptyIndex(out index))
        {
            T data = new T();
            //泛型可用参数取自： 由于T继承自StackableItemDataBase，而StackableItemDataBase又继承自ItemDataBase
            data.id = id;
            data.count = count;
            return true;
        }
        return false;
    }
    public void RemoveItem(int index)
    {
        itemList[index] = null;
    }
    public ItemDataBase TryGetItem(string id, out int index)
    {
        for(int i = 0; i< itemList.Count; i++)
        {
            ItemDataBase itemData = itemList[i];
            if(itemData!=null && itemData.id == id)
            {
                index = i;
                return itemData;
            }
        }
        index = -1;
        return null;
    }
    /// <summary>
    /// 获取第一个空格子的索引
    /// </summary>
    public bool TryGetFirstEmptyIndex(out int index)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == null)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }


#endif
    #endregion
}