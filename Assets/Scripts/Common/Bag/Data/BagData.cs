using System.Collections.Generic;

public class BagData //背包的格子是有固定的数量上限，空格子的表现为 itemList[i] = null
{
    public const int itemCount = 30;
    public List<ItemDataBase> itemList = new List<ItemDataBase>(itemCount);

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