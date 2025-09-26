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
            obj.name = prefab.name;
        }
        return obj;
    }
}
