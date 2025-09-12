using JKFrame;
using UnityEngine;
/// <summary>
/// 鼠标控制
/// </summary>
public abstract class UI_CustomWindowBase : UI_WindowBase
{
    //控制鼠标显示
    [SerializeField] private bool activeMouse = true;
    //控制鼠标
    [SerializeField] private bool activeMouseConstraint = false;
    private static bool oldMouseLockState;
    public override void OnShow()
    {
        base.OnShow();
        oldMouseLockState = ClientGlobal.Instance.ActiveMouse;
        ClientGlobal.Instance.ActiveMouse = activeMouse;
    }
    protected virtual void Update()
    {
        if (activeMouseConstraint && ClientGlobal.Instance.ActiveMouse != activeMouse)
        {
            ClientGlobal.Instance.ActiveMouse = activeMouse;
        }
    }
    public override void OnClose()
    {
        base.OnClose();
        ClientGlobal.Instance.ActiveMouse = oldMouseLockState;
    }
}
