#if UNITY_SERVER || UNITY_EDITOR
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
        if(player.inputData.moveDir == Vector3.zero)
        {
            player.ChangeState(PlayerState.Idle);
            return;
        }
        //旋转
        player.View.transform.rotation = Quaternion.RotateTowards(player.View.transform.rotation, Quaternion.LookRotation(player.inputData.moveDir), Time.deltaTime * player.RotateSpeed);

 
    }
    public override void Exit()
    {
        player.View.CleanRootMotionAction();
    }
    /// <summary>
    /// 必须检查动画Clip 是否启用了Root Motion，需要单独修改移动动画，否则出问题
    /// </summary>
    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        player.Animator.speed = player.MoveSpeed;
        deltaPosition.y -= 9.8f * Time.deltaTime;  //模拟重力
        player.CharacterController.Move(deltaPosition);

        //更新AOI :因为有位移，就要更新AOI
        if (deltaPosition != Vector3.zero)
        {
            player.UpdateAOICoord();
        }
    }

}
#endif