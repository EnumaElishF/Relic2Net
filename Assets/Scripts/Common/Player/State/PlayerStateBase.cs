
#if UNITY_SERVER || UNITY_EDITOR
using JKFrame;
/// <summary>
/// 只提供服务端使用
/// </summary>
public class PlayerStateBase : StateBase
{
    protected PlayerController player;
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        player = (PlayerController)owner;
    }
}
#endif