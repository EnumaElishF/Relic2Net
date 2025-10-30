using JKFrame;
using UnityEngine;

public class ClientUtility
{
    public const string emptySlotPath = "UI_EmptySlot";
    /// <summary>
    /// 窗口UI界面判断是否激活打开状态
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool GetWindowActiveState<T>(out T window) where T : UI_WindowBase
    {
        window = UISystem.GetWindow<T>();
        return !(window == null || !window.gameObject.activeInHierarchy);
    }
    public static GameObject GetOrInstantiate(GameObject prefab,Transform parent)
    {
        GameObject obj = PoolSystem.GetGameObject(prefab.name, parent);
        if(obj == null)
        {
            obj = GameObject.Instantiate(prefab, parent);
            //实例化出来的预制体，会有括号克隆的情况，必须重新赋值一下原预制体名称，否则肯定会出问题的
            obj.name = prefab.name;
        }
        return obj;
    }
}
