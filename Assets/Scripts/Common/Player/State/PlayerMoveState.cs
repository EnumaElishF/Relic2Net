#if UNITY_SERVER || UNITY_EDITOR
using System;
using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("Move");
        player.View.SetRootMotionAction(OnRootMotion);
    }


    public override void Update()
    {
        if(player.inputData.moveDir == Vector2.zero)
        {
            player.ChangeState(PlayerState.Idle);
            return;
        }

 
    }
    public override void Exit()
    {
        player.View.CleanRootMotionAction();
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        player.Animator.speed = player.MoveSpeed;
        deltaPosition.y -= 9.8f * Time.deltaTime;  //模拟重力
        player.CharacterController.Move(deltaPosition);

        //更新AOI :因为有位移，就要更新AOI
        if(deltaPosition != Vector3.zero)
        {
            player.UpdateAOICoord();
        }
    }
}
#endif 