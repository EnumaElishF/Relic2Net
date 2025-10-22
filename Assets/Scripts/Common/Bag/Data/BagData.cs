using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BagData: INetworkSerializable
{
    //背包的格子是有固定的数量上限，空格子的表现为 itemList[i] = null
    public const int itemCount = 30;
    [BsonIgnore] //避免保存到数据库标志
    public int dataVersion; // 背包数据的版本
    public List<ItemDataBase> itemList = new List<ItemDataBase>(itemCount);
    public int usedWeaponIndex; //正在使用的武器格子索引
    public int[] shortcutBarIndexs = new int[GlobalUtility.itemShortcutBarCount];
    public int coinCount;//金币数量
    public BagData()
    {
        for(int i = 0; i < itemCount; i++)
        {
            itemList.Add(null);
        }
        for(int i = 0; i < GlobalUtility.itemShortcutBarCount; i++)
        {
            shortcutBarIndexs[i] = -1; //-1代表默认是空格子
        }
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref usedWeaponIndex);//背包中标记当前武器格子
        serializer.SerializeValue(ref coinCount);
        serializer.SerializeValue(ref shortcutBarIndexs);
        for (int i = 0; i < itemCount; i++)
        {
            if (serializer.IsReader) //反序列化, 数据转为对象
            {
                //ItemType itemType = default;
                //serializer.SerializeValue(ref itemType);
                //读取器
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out ItemType itemType);
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
                    //写入使用WriteValueSafe方法比 WriteValue要安全，内部多了TryBeginWrite开始写入的检测
                    if (itemData is WeaponData) writer.WriteValueSafe(ItemType.Weapon);
                    else if (itemData is ConsumableData) writer.WriteValueSafe(ItemType.Consumable);
                    else if (itemData is MaterialData) writer.WriteValueSafe(ItemType.Material);
                    itemData.NetworkSerialize(serializer);
                }
            }
        }

    }
    /// <summary>
    /// 检查背包索引的有效性
    /// </summary>
    public bool CheckBagIndexRange(int index)
    {
        return index >= 0 && index < itemCount;
    }
    /// <summary>
    /// 检查快捷栏索引的有效性
    /// </summary>
    public bool CheckShortcutBarIndexRange(int index)
    {
        return index >= 0 && index < shortcutBarIndexs.Length;
    }

    #region Server 服务端背包数据->数据获取
    //暂时不使用 UNITY_SERVER ，这里调用出错误。暂时移除 TryUseItem 方法的条件编译限制
    //错误内容，Assets\Scripts\Server\Client\ClientsManager.cs(308,45): error CS1061: 'BagData' does not contain a definition for 'TryUseItem' and no accessible extension method 'TryUseItem' accepting a first argument of type 'BagData' could be found (are you missing a using directive or an assembly reference?)
    //#if UNITY_SERVER || UNITY_EDITOR
    public ItemDataBase TryUseItem(int index)
    {
        ItemDataBase itemData = itemList[index];
        // 只有武器与消耗品才可以使用
        if (itemData is WeaponData)
        {
            AddDataVersion();
            usedWeaponIndex = index;
            return itemData;
        }
        else if (itemData is ConsumableData)
        {
            AddDataVersion();
            //TODO 暂时虚拟一个消耗品减少的效果：临时逻辑
            ConsumableData consumableData = (ConsumableData)itemData;
            consumableData.count -= 1;
            if (consumableData.count <= 0) //物品全部使用完毕
            {
                itemList[index] = null;
                itemData = null;
            }
            return itemData;
        }
        return null;
    }


    public ItemDataBase TryGetItem(string id, out int index)
    {
        for (int i = 0; i< itemList.Count; i++)
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
        index = -1;
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

    public void SwapItem(int itemIndexA, int itemIndexB)
    {
        if (usedWeaponIndex == itemIndexA) usedWeaponIndex = itemIndexB;
        else if (usedWeaponIndex == itemIndexB) usedWeaponIndex = itemIndexA;
        ItemDataBase temp = itemList[itemIndexA];
        itemList[itemIndexA] = itemList[itemIndexB];
        itemList[itemIndexB] = temp;
        AddDataVersion();
    }
    /// <summary>
    /// 版本+1
    /// </summary>
    public void AddDataVersion()
    {
        dataVersion += 1;
    }

    public bool TryGetShortcutBarIndex(int bagIndex,out int shortCutBarIndex)
    {
        for(int i = 0; i < shortcutBarIndexs.Length; i++)
        {
            if (shortcutBarIndexs[i] == bagIndex)
            {
                shortCutBarIndex = i;
                return true;
            }
        }
        shortCutBarIndex = -1;
        return false;
    }

    public void UpdateShortcutBarItem(int shortcutIndex, int bagIndex)
    {
        shortcutBarIndexs[shortcutIndex] = bagIndex;
    }
    public void RemoveShortcutBarItem(int shortcutIndex)
    {
        UpdateShortcutBarItem(shortcutIndex, -1);
    }

    public void SwapShortcutBarItem(int shortcutBarIndexA, int shortcutBarIndexB)
    {
        int temp = shortcutBarIndexs[shortcutBarIndexA];
        shortcutBarIndexs[shortcutBarIndexA] = shortcutBarIndexs[shortcutBarIndexB];
        shortcutBarIndexs[shortcutBarIndexB] = temp;
    }
    /// <summary>
    /// 尝试添加物品，指定位置
    /// </summary>
    /// <param name="targetItemConfig">目标物品</param>
    /// <param name="stackableCount">堆叠数量</param>
    /// <param name="targetIndex">目标Index，如果没有就不给放置</param>
    /// <returns></returns>
    public bool TryAddItem(ItemConfigBase targetItemConfig,int stackableCount,int targetIndex)
    {
        bool isStackableItemData = targetItemConfig.GetDefaultItemData() is StackableItemDataBase;
        if (isStackableItemData)
        {
            if (itemList[targetIndex] == null) //空位
            {
                StackableItemDataBase newData = (StackableItemDataBase)targetItemConfig.GetDefaultItemData().Copy();
                newData.count = 1;
                itemList[targetIndex] = newData;
                return true;
            }
            else if (itemList[targetIndex].id == targetItemConfig.name)
            {
                ((StackableItemDataBase)itemList[targetIndex]).count += 1;
                return true;
            }
        }
        else
        {
            if (itemList[targetIndex] == null)
            {
                itemList[targetIndex] = targetItemConfig.GetDefaultItemData().Copy();
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 尝试添加物品，不指定位置_ 任务或合成产出的道具自动堆叠排放
    /// </summary>
    /// <param name="targetItemConfig">目标物品</param>
    /// <param name="stackableCount">堆叠数量</param>
    /// <param name="itemIndex">输出-最终添加的index</param>
    /// <returns></returns>
    public bool TryAddItem(ItemConfigBase targetItemConfig, int stackableCount,out int itemIndex)
    {
        itemIndex = -1;
        bool isStackableItemData = targetItemConfig.GetDefaultItemData() is StackableItemDataBase;
        if (isStackableItemData)
        {
            //对于可堆叠物品(消耗品和材料)来说，优先去判断堆放，然后再考虑占用新空位
            StackableItemDataBase existedItemData = TryGetItem(targetItemConfig.name, out itemIndex) as StackableItemDataBase;
            if(existedItemData != null) //堆叠
            {
                existedItemData.count += 1;
                return true;
            }
            else if(TryGetFirstEmptyIndex(out itemIndex))//需要空位
            {
                //放新的数据到空位格子
                StackableItemDataBase newData = (StackableItemDataBase)targetItemConfig.GetDefaultItemData().Copy();
                newData.count = 1;
                itemList[itemIndex] = newData;
                return true;
            }
        }
        else if(TryGetFirstEmptyIndex(out itemIndex))
        {
            //非可堆叠数据-武器等
            itemList[itemIndex] = targetItemConfig.GetDefaultItemData();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检查合成是否可行
    /// </summary>
    public bool CheckCraft(ItemConfigBase targetItem, out bool containUsedWeapon)
    {
        containUsedWeapon = false;
        foreach(KeyValuePair<string,int> item in targetItem.craftConfig.itemDic)
        {
            ItemDataBase itemData = TryGetItem(item.Key, out int itemIndex);
            if (itemData == null) return false;
            if (itemIndex == usedWeaponIndex) containUsedWeapon = true;
            if(itemData is StackableItemDataBase) //如果itemData是可堆叠物品
            {
                int curr = ((StackableItemDataBase)itemData).count;
                if (curr < item.Value) return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 移除一个格子的物品，无数量
    /// </summary>
    /// <param name="index"></param>
    public void RemoveItem(int index)
    {
        itemList[index] = null;
    }
    /// <summary>
    /// 移除一个格子的物品，按照数量
    /// </summary>
    public void RemoveItem(int itemIndex,int count)
    {
        ItemDataBase itemData = itemList[itemIndex];
        if (itemData == null) return;
        StackableItemDataBase stackableItemData = itemData as StackableItemDataBase;
        if(stackableItemData != null)
        {
            stackableItemData.count -= count;
            if(stackableItemData.count == 0) RemoveItem(itemIndex);
        }
        else RemoveItem(itemIndex); //武器的情况不考虑数量的问题，直接移除就行
    }
    /// <summary>
    /// 检查能否添加物品
    /// </summary>
    public bool CheckAddItem(ItemConfigBase targetItemConfig)
    {
        bool isStackableItemData = targetItemConfig.GetDefaultItemData() is StackableItemDataBase;
        if (isStackableItemData)
        {
            //对于可堆叠物品(消耗品和材料)来说，优先去判断堆放，然后再考虑占用新空位
            StackableItemDataBase existedItemData = TryGetItem(targetItemConfig.name, out int itemIndex) as StackableItemDataBase;
            return existedItemData != null || TryGetFirstEmptyIndex(out itemIndex);
        }
        else if (TryGetFirstEmptyIndex(out int itemIndex))
        {
            return true;
        }
        return false;
    }
    //#endif
    #endregion
}