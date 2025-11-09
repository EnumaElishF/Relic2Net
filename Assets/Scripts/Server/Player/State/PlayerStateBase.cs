
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
}
