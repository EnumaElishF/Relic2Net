using JKFrame;
using UnityEngine;

public class ClientUtility : MonoBehaviour
{
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
}
