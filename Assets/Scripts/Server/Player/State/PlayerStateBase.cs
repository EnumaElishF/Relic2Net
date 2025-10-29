
using JKFrame;
using UnityEngine;
/// <summary>
/// 只提供服务端使用
/// </summary>
public class PlayerStateBase : StateBase
{
    protected PlayerServerController serverController;
    public PlayerController mainController { get => serverController.mainController; }
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        serverController = (PlayerServerController)owner;
    }
    /// <summary>
    /// 解决动作已经进入下一个动画，但是还处于上一个动画的最后结束帧的事件
    /// </summary>
    public bool CheckAnimationState(string stateName, out float normalizedTime)
    {
        //优先判断下一个动作状态
        AnimatorStateInfo nextInfo = serverController.animator.GetNextAnimatorStateInfo(0);
        if (nextInfo.IsName(stateName))
        {
            normalizedTime = nextInfo.normalizedTime;
            return true;
        }
        AnimatorStateInfo currentInfo = serverController.animator.GetCurrentAnimatorStateInfo(0);
        normalizedTime = currentInfo.normalizedTime;
        return currentInfo.IsName(stateName);
    }
}
