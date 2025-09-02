
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public override void Update()
    {
        base.Update();
        if (player.inputData.moveDir != Vector2.zero)
        {
            player.ChangeState(PlayerState.Move);
            return;
        }
    }
}
