using JKFrame;
using UnityEngine;

public static class GlobalUtility
{
    public const int itemShortcutBarCount = 8; //物品快捷栏格子数量
    public static ItemType GetItemType(ItemDataBase data)
    {
        if (data == null) return ItemType.Empty;
        return data.GetItemType();
    }

    public static GameObject GetOrInstantiate(GameObject prefab, Transform parent)
    {
        GameObject obj = PoolSystem.GetGameObject(prefab.name, parent);
        if (obj == null)
        {
            obj = GameObject.Instantiate(prefab, parent);
            //实例化出来的预制体，会有括号克隆的情况，必须重新赋值一下原预制体名称，否则肯定会出问题的
            obj.name = prefab.name;
        }
        return obj;
    }
}
