#if UNITY_SERVER || UNITY_EDITOR
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("Idle");
    }
    public override void Update()
    {
        if (player.inputData.moveDir != Vector2.zero)
        {
            player.ChangeState(PlayerState.Move);
            return;
        }
    }
}
#endif